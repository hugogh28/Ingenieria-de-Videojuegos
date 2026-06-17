using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Detection")]
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private bool requireInteractableTag = false;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private LayerMask interactionLayers = ~0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private bool createPromptIfMissing = true;
    [SerializeField] private Vector2 promptAnchoredPosition = new Vector2(0f, -70f);
    [SerializeField] private float promptFontSize = 24f;
    [SerializeField] private string promptFormat = "Presiona {0} para {1}";

    [Header("Player")]
    [SerializeField] private PlayerController player;
    [SerializeField] private string playerTag = "Player";

    private IInteractable currentInteractable;
    private Outline currentOutline;
    private GameObject currentObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (interactionCamera == null)
        {
            interactionCamera = Camera.main;
        }

        ResolvePlayer();
        CreatePromptIfNeeded();
        HidePrompt();
    }

    private void Update()
    {
        ResolvePlayer();
        DetectInteractable();

        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            currentInteractable.Interact(player);
        }
    }

    private void DetectInteractable()
    {
        if (interactionCamera == null)
        {
            ClearCurrentInteractable();
            return;
        }

        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayers, QueryTriggerInteraction.Ignore))
        {
            ClearCurrentInteractable();
            return;
        }

        GameObject hitObject = hit.collider.gameObject;
        IInteractable interactable = FindInteractableInParents(hit.collider.transform);
        GameObject interactableObject = interactable is Component component
            ? component.gameObject
            : hitObject;

        if (interactable == null)
        {
            ClearCurrentInteractable();
            return;
        }

        if (requireInteractableTag && hitObject.tag != interactableTag && interactableObject.tag != interactableTag)
        {
            ClearCurrentInteractable();
            return;
        }

        if (!interactable.CanInteract(player))
        {
            ClearCurrentInteractable();
            return;
        }

        SetCurrentInteractable(interactable, interactableObject);
    }

    private void SetCurrentInteractable(IInteractable interactable, GameObject interactableObject)
    {
        if (currentInteractable == interactable)
        {
            ShowPrompt(interactable.InteractionActionText);
            return;
        }

        ClearCurrentInteractable();

        currentInteractable = interactable;
        currentObject = interactableObject;
        currentOutline = currentObject.GetComponentInChildren<Outline>();

        if (currentOutline != null)
        {
            currentOutline.enabled = true;
        }

        ShowPrompt(interactable.InteractionActionText);
    }

    private void ClearCurrentInteractable()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        currentInteractable = null;
        currentOutline = null;
        currentObject = null;
        HidePrompt();
    }

    private void ShowPrompt(string actionText)
    {
        if (interactionText == null)
        {
            return;
        }

        interactionText.gameObject.SetActive(true);
        interactionText.text = string.Format(promptFormat, interactionKey, actionText);
    }

    private void HidePrompt()
    {
        if (interactionText == null)
        {
            return;
        }

        interactionText.text = "";
        interactionText.gameObject.SetActive(false);
    }

    private void CreatePromptIfNeeded()
    {
        if (interactionText != null || !createPromptIfMissing)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Interaction Prompt Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("Interaction Prompt Text");
        textObject.transform.SetParent(canvasObject.transform, false);

        interactionText = textObject.AddComponent<TextMeshProUGUI>();
        interactionText.alignment = TextAlignmentOptions.Center;
        interactionText.fontSize = promptFontSize;
        interactionText.raycastTarget = false;

        RectTransform rect = interactionText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = promptAnchoredPosition;
        rect.sizeDelta = new Vector2(700f, 70f);
    }

    private void ResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
        {
            return;
        }

        player = playerObject.GetComponent<PlayerController>();

        if (player == null)
        {
            player = playerObject.GetComponentInParent<PlayerController>();
        }

        if (player == null)
        {
            player = playerObject.GetComponentInChildren<PlayerController>();
        }
    }

    private IInteractable FindInteractableInParents(Transform start)
    {
        Transform current = start;

        while (current != null)
        {
            MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IInteractable interactable)
                {
                    return interactable;
                }
            }

            current = current.parent;
        }

        return null;
    }
}
