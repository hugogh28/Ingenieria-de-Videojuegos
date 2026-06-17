using System.Collections;
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

    [Header("Gain Feedback")]
    [SerializeField] private TextMeshProUGUI gainedPointsText;
    [SerializeField] private bool createGainTextIfMissing = true;
    [SerializeField] private Vector2 gainTextOffset = new Vector2(95f, 0f);
    [SerializeField] private float gainAnimationDuration = 0.75f;
    [SerializeField] private float gainMoveUpDistance = 35f;
    [SerializeField] private float gainStartScale = 1.15f;
    [SerializeField] private float gainEndScale = 0.65f;
    [SerializeField] private float gainRotationDegrees = 8f;

    [Header("Debug")]
    [SerializeField] private bool debugObserver = false;

    private PlayerController subscribedPlayer;
    private Coroutine gainAnimationCoroutine;
    private RectTransform gainedPointsRect;
    private Vector2 gainedPointsStartPosition;

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

        CreateGainTextIfNeeded();
        CacheGainTextState();
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

        if (delta > 0)
        {
            PlayGainFeedback(delta);
        }

        if (debugObserver)
        {
            Debug.Log($"PlayerPointsObserver: PointsChanged recibido -> puntos: {currentPoints}, delta: {delta}.", this);
        }
    }

    private void CreateGainTextIfNeeded()
    {
        if (gainedPointsText != null || !createGainTextIfMissing || pointsText == null)
        {
            return;
        }

        Transform parent = pointsText.transform.parent != null
            ? pointsText.transform.parent
            : pointsText.transform;

        GameObject gainObject = new GameObject("Gained Points Text");
        gainObject.transform.SetParent(parent, false);

        gainedPointsText = gainObject.AddComponent<TextMeshProUGUI>();
        gainedPointsText.alignment = TextAlignmentOptions.Center;
        gainedPointsText.fontSize = pointsText.fontSize;
        gainedPointsText.fontStyle = FontStyles.Bold;
        gainedPointsText.raycastTarget = false;
        gainedPointsText.color = pointsText.color;

        if (pointsText.font != null)
        {
            gainedPointsText.font = pointsText.font;
        }

        RectTransform pointsRect = pointsText.rectTransform;
        RectTransform gainRect = gainedPointsText.rectTransform;
        gainRect.anchorMin = pointsRect.anchorMin;
        gainRect.anchorMax = pointsRect.anchorMax;
        gainRect.pivot = pointsRect.pivot;
        gainRect.sizeDelta = new Vector2(170f, pointsRect.sizeDelta.y);
        gainRect.anchoredPosition = pointsRect.anchoredPosition + gainTextOffset;
    }

    private void CacheGainTextState()
    {
        if (gainedPointsText == null)
        {
            return;
        }

        gainedPointsRect = gainedPointsText.rectTransform;
        gainedPointsStartPosition = gainedPointsRect.anchoredPosition;
        gainedPointsText.gameObject.SetActive(false);
    }

    private void PlayGainFeedback(int delta)
    {
        if (gainedPointsText == null)
        {
            CreateGainTextIfNeeded();
            CacheGainTextState();
        }

        if (gainedPointsText == null || gainedPointsRect == null)
        {
            return;
        }

        if (gainAnimationCoroutine != null)
        {
            StopCoroutine(gainAnimationCoroutine);
        }

        gainAnimationCoroutine = StartCoroutine(AnimateGainText(delta));
    }

    private IEnumerator AnimateGainText(int delta)
    {
        gainedPointsText.gameObject.SetActive(true);
        gainedPointsText.text = $"+{delta}";
        gainedPointsText.fontStyle = FontStyles.Bold;

        Color baseColor = gainedPointsText.color;
        baseColor.a = 1f;
        gainedPointsText.color = baseColor;

        gainedPointsRect.anchoredPosition = gainedPointsStartPosition;
        gainedPointsRect.localScale = Vector3.one * gainStartScale;
        gainedPointsRect.localRotation = Quaternion.Euler(0f, 0f, -gainRotationDegrees);

        float elapsed = 0f;
        float duration = Mathf.Max(0.001f, gainAnimationDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);

            gainedPointsRect.anchoredPosition = gainedPointsStartPosition + Vector2.up * (gainMoveUpDistance * eased);
            gainedPointsRect.localScale = Vector3.one * Mathf.Lerp(gainStartScale, gainEndScale, eased);
            gainedPointsRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-gainRotationDegrees, gainRotationDegrees, Mathf.Sin(t * Mathf.PI)));

            Color color = baseColor;
            color.a = 1f - t;
            gainedPointsText.color = color;

            yield return null;
        }

        gainedPointsText.gameObject.SetActive(false);
        gainedPointsRect.anchoredPosition = gainedPointsStartPosition;
        gainedPointsRect.localScale = Vector3.one;
        gainedPointsRect.localRotation = Quaternion.identity;
        gainAnimationCoroutine = null;
    }
}
