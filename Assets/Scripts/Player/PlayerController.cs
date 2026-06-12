using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool isMoving;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    public event Action<float, float> HealthChanged;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    // Mantengo esta propiedad porque tu interfaz IHealth la exige.
    // Cualquier script que haga player.health = X actualizará también la UI.
    public float health
    {
        get => currentHealth;
        set
        {
            SetHealth(value);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }
    }

    public int points = 0;

    private void Start()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        controller = GetComponent<CharacterController>();

        NotifyHealthChanged();
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
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
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (lastPosition != gameObject.transform.position && isGrounded == true)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        lastPosition = gameObject.transform.position;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        Debug.Log($"El jugador ha recibido {damage} puntos de daño");

        SetHealth(currentHealth - damage);

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

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
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
    }
#endif
}
