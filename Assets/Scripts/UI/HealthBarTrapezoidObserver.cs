using UnityEngine;

public sealed class HealthBarTrapezoidObserver : MonoBehaviour
{
    [Header("Observed Subject")]
    [SerializeField] private PlayerController observedPlayer;
    [SerializeField] private bool findPlayerByTagIfEmpty = true;

    [Header("UI References")]
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private RectTransform fillRect;

    [Header("Animation")]
    [SerializeField] private bool animate = true;
    [SerializeField] [Min(0.1f)] private float animationSpeed = 4f;

    private float fullWidth;
    private float currentFill01 = 1f;
    private float targetFill01 = 1f;
    private bool initialized;

    private void Awake()
    {
        InitializeWidth();
    }

    private void OnEnable()
    {
        ResolveObservedPlayer();

        if (observedPlayer == null)
        {
            Debug.LogWarning($"{nameof(HealthBarTrapezoidObserver)} no tiene PlayerController asignado.", this);
            return;
        }

        observedPlayer.HealthChanged += OnHealthChanged;
        OnHealthChanged(observedPlayer.CurrentHealth, observedPlayer.MaxHealth);
    }

    private void OnDisable()
    {
        if (observedPlayer != null)
        {
            observedPlayer.HealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (!animate)
        {
            return;
        }

        if (Mathf.Approximately(currentFill01, targetFill01))
        {
            return;
        }

        currentFill01 = Mathf.MoveTowards(
            currentFill01,
            targetFill01,
            animationSpeed * Time.unscaledDeltaTime
        );

        ApplyFill(currentFill01);
    }

    private void ResolveObservedPlayer()
    {
        if (observedPlayer != null || !findPlayerByTagIfEmpty)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            observedPlayer = playerObject.GetComponent<PlayerController>();
        }
    }

    private void InitializeWidth()
    {
        if (initialized)
        {
            return;
        }

        if (fillRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        if (maskRect != null && maskRect.rect.width > 0f)
        {
            fullWidth = maskRect.rect.width;
        }
        else
        {
            fullWidth = fillRect.rect.width;
        }

        if (fullWidth <= 0f)
        {
            return;
        }

        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth);
        initialized = true;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        InitializeWidth();

        targetFill01 = maxHealth <= 0f
            ? 0f
            : Mathf.Clamp01(currentHealth / maxHealth);

        if (!animate)
        {
            currentFill01 = targetFill01;
            ApplyFill(currentFill01);
        }
    }

    private void ApplyFill(float fill01)
    {
        if (fillRect == null)
        {
            return;
        }

        InitializeWidth();

        float newWidth = fullWidth * Mathf.Clamp01(fill01);
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }
}
