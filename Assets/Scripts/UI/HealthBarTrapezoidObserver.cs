using UnityEngine;

/// <summary>
/// Observer for PlayerController.HealthChanged.
/// It updates a procedural slanted fill without changing RectTransform width.
/// </summary>
public sealed class HealthBarTrapezoidObserver : MonoBehaviour
{
    [Header("Observed Subject")]
    [SerializeField] private PlayerController observedPlayer;

    [Header("UI References")]
    [SerializeField] private SlantedHealthFillGraphic fillGraphic;

    [Header("Animation")]
    [SerializeField] private bool animate = true;
    [SerializeField, Min(0.1f)] private float animationSpeed = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugObserver;

    private float currentFill01 = 1f;
    private float targetFill01 = 1f;

    private void Awake()
    {
        if (fillGraphic != null)
        {
            currentFill01 = fillGraphic.FillAmount;
            targetFill01 = currentFill01;
        }
    }

    private void OnEnable()
    {
        if (observedPlayer == null)
        {
            Debug.LogWarning($"{nameof(HealthBarTrapezoidObserver)}: observedPlayer is not assigned.", this);
            return;
        }

        observedPlayer.HealthChanged += OnHealthChanged;
        OnHealthChanged(observedPlayer.CurrentHealth, observedPlayer.MaxHealth);
    }

    private void OnDisable()
    {
        if (observedPlayer == null)
            return;

        observedPlayer.HealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (!animate)
            return;

        if (Mathf.Approximately(currentFill01, targetFill01))
            return;

        currentFill01 = Mathf.MoveTowards(
            currentFill01,
            targetFill01,
            animationSpeed * Time.unscaledDeltaTime
        );

        ApplyFill(currentFill01);
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        targetFill01 = maxHealth <= 0f
            ? 0f
            : Mathf.Clamp01(currentHealth / maxHealth);

        if (debugObserver)
            Debug.Log($"HealthBar observer: {currentHealth}/{maxHealth} -> {targetFill01:0.00}", this);

        if (!animate)
        {
            currentFill01 = targetFill01;
            ApplyFill(currentFill01);
        }
    }

    private void ApplyFill(float value)
    {
        if (fillGraphic == null)
            return;

        fillGraphic.FillAmount = Mathf.Clamp01(value);
    }
}
