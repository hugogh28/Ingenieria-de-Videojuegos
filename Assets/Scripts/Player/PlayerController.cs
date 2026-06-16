using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IHealth
{
    private CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isMoving;
    private Vector3 lastPosition = Vector3.zero;
    private PlayerCameraMotion cameraMotion;
    private float lastAirVerticalVelocity;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private bool debugHealthObserver = false;

    [Header("Points")]
    [SerializeField] private int currentPoints = 0;
    [SerializeField] private bool debugPointsObserver = false;

    // SUBJECT del patrón Observer para la vida.
    public event Action<float, float> HealthChanged;

    // SUBJECT del patrón Observer para los puntos.
    // Primer parámetro: puntos actuales.
    // Segundo parámetro: diferencia aplicada. Positivo al ganar, negativo al gastar.
    public event Action<int, int> PointsChanged;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);

    public int CurrentPoints => currentPoints;

    // Compatibilidad con IHealth y con scripts antiguos que todavía usen player.health.
    // Pasar por esta propiedad también notifica al Observer de vida.
    public float health
    {
        get => currentHealth;
        set => SetHealth(value);
    }

    // Compatibilidad con scripts antiguos que todavía usen player.points += X o player.points -= X.
    // Importante: al ser propiedad, cualquier cambio dispara PointsChanged.
    public int points
    {
        get => currentPoints;
        set => SetPoints(value);
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentPoints = Mathf.Max(0, currentPoints);
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            cameraMotion = mainCamera.GetComponent<PlayerCameraMotion>();

            if (cameraMotion == null)
            {
                cameraMotion = mainCamera.gameObject.AddComponent<PlayerCameraMotion>();
            }
        }

        NotifyHealthChanged();
        NotifyPointsChanged(0);
    }

    private void Update()
    {
        if (groundCheck == null || controller == null)
        {
            return;
        }

        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && !wasGrounded)
        {
            cameraMotion?.PlayLandImpulse(Mathf.Abs(lastAirVerticalVelocity));
        }

        if (!isGrounded)
        {
            lastAirVerticalVelocity = velocity.y;
        }

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            cameraMotion?.PlayJumpImpulse();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        isMoving = lastPosition != transform.position && isGrounded;
        lastPosition = transform.position;

        float movementAmount = Mathf.Clamp01(new Vector2(x, z).magnitude);
        cameraMotion?.SetMovementState(isGrounded, isMoving, velocity.y, movementAmount);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        float previousHealth = currentHealth;
        SetHealth(currentHealth - damage);

        if (currentHealth < previousHealth)
        {
            GameFeelManager.Instance.PlayPlayerDamageFeedback(damage);
            cameraMotion?.PlayDamageImpulse();
        }

        if (debugHealthObserver)
        {
            Debug.Log($"PlayerController.TakeDamage({damage}) -> {currentHealth}/{maxHealth}. Observer de vida notificado.", this);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetHealth(currentHealth + amount);
    }

    public void SetHealth(float newHealth)
    {
        float clampedHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

        if (Mathf.Approximately(currentHealth, clampedHealth))
        {
            return;
        }

        currentHealth = clampedHealth;
        NotifyHealthChanged();
    }

    public void SetMaxHealth(float newMaxHealth, bool keepCurrentPercentage = true)
    {
        float previousPercentage = maxHealth > 0f ? currentHealth / maxHealth : 1f;

        maxHealth = Mathf.Max(1f, newMaxHealth);
        currentHealth = keepCurrentPercentage
            ? maxHealth * previousPercentage
            : Mathf.Clamp(currentHealth, 0f, maxHealth);

        NotifyHealthChanged();
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetPoints(currentPoints + amount);
    }

    public bool TrySpendPoints(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (currentPoints < amount)
        {
            return false;
        }

        SetPoints(currentPoints - amount);
        return true;
    }

    public bool HasEnoughPoints(int amount)
    {
        return currentPoints >= amount;
    }

    public void SetPoints(int newPoints)
    {
        int clampedPoints = Mathf.Max(0, newPoints);

        if (currentPoints == clampedPoints)
        {
            return;
        }

        int delta = clampedPoints - currentPoints;
        currentPoints = clampedPoints;
        NotifyPointsChanged(delta);
    }

    public void ForceNotifyHealthChanged()
    {
        NotifyHealthChanged();
    }

    public void ForceNotifyPointsChanged()
    {
        NotifyPointsChanged(0);
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void NotifyPointsChanged(int delta)
    {
        PointsChanged?.Invoke(currentPoints, delta);

        if (debugPointsObserver)
        {
            Debug.Log($"PlayerController.PointsChanged -> puntos: {currentPoints}, delta: {delta}.", this);
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(0);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentPoints = Mathf.Max(0, currentPoints);
    }
#endif
}
