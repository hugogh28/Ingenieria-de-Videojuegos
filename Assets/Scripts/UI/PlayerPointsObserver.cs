using TMPro;
using UnityEngine;

public sealed class PlayerPointsObserver : MonoBehaviour
{
    [Header("Observed Subject")]
    [SerializeField] private PlayerController observedPlayer;
    [SerializeField] private bool findPlayerByTagIfEmpty = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    [Header("Debug")]
    [SerializeField] private bool debugObserver = false;

    private PlayerController subscribedPlayer;

    private void Reset()
    {
        pointsText = GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        if (pointsText == null)
        {
            pointsText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        ResolveObservedPlayer();
        SubscribeToObservedPlayer();
    }

    private void Start()
    {
        if (observedPlayer != null)
        {
            OnPointsChanged(observedPlayer.CurrentPoints, 0);
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromObservedPlayer();
    }

    private void ResolveObservedPlayer()
    {
        if (observedPlayer != null || !findPlayerByTagIfEmpty)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        observedPlayer = playerObject.GetComponent<PlayerController>();

        if (observedPlayer == null)
        {
            observedPlayer = playerObject.GetComponentInParent<PlayerController>();
        }

        if (observedPlayer == null)
        {
            observedPlayer = playerObject.GetComponentInChildren<PlayerController>();
        }
    }

    private void SubscribeToObservedPlayer()
    {
        if (observedPlayer == null || subscribedPlayer == observedPlayer)
        {
            return;
        }

        UnsubscribeFromObservedPlayer();

        subscribedPlayer = observedPlayer;
        subscribedPlayer.PointsChanged += OnPointsChanged;
        OnPointsChanged(subscribedPlayer.CurrentPoints, 0);
    }

    private void UnsubscribeFromObservedPlayer()
    {
        if (subscribedPlayer == null)
        {
            return;
        }

        subscribedPlayer.PointsChanged -= OnPointsChanged;
        subscribedPlayer = null;
    }

    private void OnPointsChanged(int currentPoints, int delta)
    {
        if (pointsText != null)
        {
            pointsText.text = $"{prefix}{currentPoints}{suffix}";
        }

        if (debugObserver)
        {
            Debug.Log($"PlayerPointsObserver: PointsChanged recibido -> puntos: {currentPoints}, delta: {delta}.", this);
        }
    }
}
