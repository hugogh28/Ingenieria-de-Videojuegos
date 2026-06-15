using TMPro;
using UnityEngine;

public sealed class WaveCountObserver : MonoBehaviour
{
    [Header("Observed Subject")]
    [SerializeField] private WaveManager observedWaveManager;
    [SerializeField] private bool findWaveManagerIfEmpty = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    [Header("Debug")]
    [SerializeField] private bool debugObserver = false;

    private WaveManager subscribedWaveManager;

    private void Reset()
    {
        waveText = GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        if (waveText == null)
        {
            waveText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        ResolveObservedWaveManager();
        SubscribeToObservedWaveManager();
    }

    private void Start()
    {
        if (observedWaveManager != null)
        {
            OnWaveChanged(observedWaveManager.CurrentWave);
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromObservedWaveManager();
    }

    private void ResolveObservedWaveManager()
    {
        if (observedWaveManager != null || !findWaveManagerIfEmpty)
        {
            return;
        }

        observedWaveManager = FindFirstObjectByType<WaveManager>();
    }

    private void SubscribeToObservedWaveManager()
    {
        if (observedWaveManager == null || subscribedWaveManager == observedWaveManager)
        {
            return;
        }

        UnsubscribeFromObservedWaveManager();

        subscribedWaveManager = observedWaveManager;
        subscribedWaveManager.WaveChanged += OnWaveChanged;
        OnWaveChanged(subscribedWaveManager.CurrentWave);
    }

    private void UnsubscribeFromObservedWaveManager()
    {
        if (subscribedWaveManager == null)
        {
            return;
        }

        subscribedWaveManager.WaveChanged -= OnWaveChanged;
        subscribedWaveManager = null;
    }

    private void OnWaveChanged(int currentWave)
    {
        if (waveText != null)
        {
            waveText.text = $"{prefix}{currentWave}{suffix}";
        }

        if (debugObserver)
        {
            Debug.Log($"WaveCountObserver: WaveChanged recibido -> oleada: {currentWave}.", this);
        }
    }
}
