using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class ButtonDropdownIntEvent : UnityEvent<int> { }

[System.Serializable]
public class ButtonDropdownStringEvent : UnityEvent<string> { }

/// <summary>
/// Convierte un Button normal en un dropdown funcional.
/// Pensado para Canvas World Space, por ejemplo una carta física.
/// No usa TMP_Dropdown, Template, Blocker ni Dropdown List.
/// 
/// Uso:
/// 1. Añade este script al mismo GameObject que tiene el Button.
/// 2. Escribe las opciones en "Options".
/// 3. Pulsa Play.
/// 4. El botón abre/cierra un panel de botones creado automáticamente.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonToWorldDropdown : MonoBehaviour
{
    [Header("Opciones")]
    [SerializeField] private List<string> options = new List<string>
    {
        "Opción 1",
        "Opción 2",
        "Opción 3"
    };

    [Header("Selección")]
    [SerializeField] private int startIndex = 0;
    [SerializeField] private bool setButtonTextToSelectedOption = true;
    [SerializeField] private string captionPrefix = "";

    [Header("Panel generado")]
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private bool closeOnStart = true;
    [SerializeField] private bool closeOnSelection = true;
    [SerializeField] private bool destroyOldGeneratedPanel = true;

    [Header("Tamaño")]
    [SerializeField] private float optionHeight = 42f;
    [SerializeField] private float panelMaxHeight = 180f;
    [SerializeField] private float panelSpacing = 3f;
    [SerializeField] private int fontSize = 24;

    [Header("Colores")]
    [SerializeField] private Color panelColor = new Color(0.06f, 0.06f, 0.06f, 0.98f);
    [SerializeField] private Color optionColor = new Color(0.22f, 0.22f, 0.22f, 0.98f);
    [SerializeField] private Color textColor = Color.white;

    [Header("Eventos")]
    public ButtonDropdownIntEvent onValueChangedIndex = new ButtonDropdownIntEvent();
    public ButtonDropdownStringEvent onValueChangedText = new ButtonDropdownStringEvent();

    private const string GeneratedPanelName = "__Generated_DropdownPanel";

    private Button mainButton;
    private TMP_Text mainButtonText;
    private RectTransform buttonRect;
    private RectTransform panelRect;
    private RectTransform contentRect;
    private readonly List<Button> generatedOptionButtons = new List<Button>();

    private int currentIndex;
    private bool isOpen;

    public int Value => currentIndex;

    public string CurrentText
    {
        get
        {
            if (options == null || options.Count == 0)
                return string.Empty;

            int safeIndex = Mathf.Clamp(currentIndex, 0, options.Count - 1);
            return options[safeIndex];
        }
    }

    private void Awake()
    {
        mainButton = GetComponent<Button>();
        buttonRect = GetComponent<RectTransform>();
        mainButtonText = GetComponentInChildren<TMP_Text>(true);

        mainButton.onClick.RemoveListener(ToggleDropdown);
        mainButton.onClick.AddListener(ToggleDropdown);

        currentIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, options.Count - 1));

        if (createOnStart)
            BuildDropdown();

        RefreshMainButtonText();

        if (closeOnStart)
            CloseDropdown();
    }

    private void OnDestroy()
    {
        if (mainButton != null)
            mainButton.onClick.RemoveListener(ToggleDropdown);
    }

    [ContextMenu("Rebuild Dropdown")]
    public void BuildDropdown()
    {
        if (buttonRect == null)
            buttonRect = GetComponent<RectTransform>();

        if (destroyOldGeneratedPanel)
            DestroyOldPanel();

        GameObject panelObject = new GameObject(GeneratedPanelName, typeof(RectTransform));
        panelObject.transform.SetParent(transform.parent, false);
        panelObject.layer = gameObject.layer;

        panelRect = panelObject.GetComponent<RectTransform>();

        CopyButtonAnchorsToPanel();
        PositionPanelUnderButton();

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = true;

        Canvas panelCanvas = panelObject.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 5000;

        GraphicRaycaster panelRaycaster = panelObject.AddComponent<GraphicRaycaster>();
        panelRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        ScrollRect scrollRect = panelObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 18f;

        GameObject viewportObject = CreateUIObject("Viewport", panelObject.transform);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        Stretch(viewportRect);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewportImage.raycastTarget = true;

        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = CreateUIObject("Content", viewportObject.transform);
        contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = contentObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = panelSpacing;
        layout.padding = new RectOffset(4, 4, 4, 4);

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        RebuildOptions();

        panelObject.SetActive(false);
        isOpen = false;
    }

    public void SetOptions(List<string> newOptions)
    {
        options = newOptions != null ? new List<string>(newOptions) : new List<string>();
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, options.Count - 1));

        if (panelRect != null)
            RebuildOptions();

        RefreshMainButtonText();
    }

    public void SetValueWithoutNotify(int index)
    {
        if (options == null || options.Count == 0)
        {
            currentIndex = 0;
            RefreshMainButtonText();
            return;
        }

        currentIndex = Mathf.Clamp(index, 0, options.Count - 1);
        RefreshMainButtonText();
    }

    public void SetValue(int index)
    {
        if (options == null || options.Count == 0)
        {
            currentIndex = 0;
            RefreshMainButtonText();
            onValueChangedIndex.Invoke(currentIndex);
            onValueChangedText.Invoke(string.Empty);
            return;
        }

        currentIndex = Mathf.Clamp(index, 0, options.Count - 1);
        RefreshMainButtonText();

        onValueChangedIndex.Invoke(currentIndex);
        onValueChangedText.Invoke(options[currentIndex]);
    }

    public void ToggleDropdown()
    {
        if (panelRect == null)
            BuildDropdown();

        if (isOpen)
            CloseDropdown();
        else
            OpenDropdown();
    }

    public void OpenDropdown()
    {
        if (panelRect == null)
            BuildDropdown();

        panelRect.gameObject.SetActive(true);

        transform.SetAsLastSibling();
        panelRect.SetAsLastSibling();

        isOpen = true;
    }

    public void CloseDropdown()
    {
        if (panelRect != null)
            panelRect.gameObject.SetActive(false);

        isOpen = false;
    }

    private void RebuildOptions()
    {
        ClearGeneratedOptions();

        if (contentRect == null || options == null)
            return;

        float totalHeight = 8f + options.Count * optionHeight + Mathf.Max(0, options.Count - 1) * panelSpacing;
        float finalHeight = Mathf.Min(panelMaxHeight, Mathf.Max(optionHeight + 8f, totalHeight));

        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, finalHeight);

        for (int i = 0; i < options.Count; i++)
        {
            int capturedIndex = i;
            string optionText = options[i];

            GameObject optionObject = CreateUIObject("Option_" + i + "_" + optionText, contentRect);
            RectTransform optionRect = optionObject.GetComponent<RectTransform>();
            optionRect.sizeDelta = new Vector2(0f, optionHeight);

            LayoutElement layoutElement = optionObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = optionHeight;
            layoutElement.preferredHeight = optionHeight;

            Image optionImage = optionObject.AddComponent<Image>();
            optionImage.color = optionColor;
            optionImage.raycastTarget = true;

            Button optionButton = optionObject.AddComponent<Button>();
            optionButton.targetGraphic = optionImage;

            GameObject labelObject = CreateUIObject("Label", optionObject.transform);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            Stretch(labelRect);
            labelRect.offsetMin = new Vector2(12f, 0f);
            labelRect.offsetMax = new Vector2(-12f, 0f);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = optionText;
            label.fontSize = fontSize;
            label.color = textColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;

            optionButton.onClick.AddListener(() =>
            {
                SetValue(capturedIndex);

                if (closeOnSelection)
                    CloseDropdown();
            });

            generatedOptionButtons.Add(optionButton);
        }
    }

    private void ClearGeneratedOptions()
    {
        for (int i = generatedOptionButtons.Count - 1; i >= 0; i--)
        {
            Button button = generatedOptionButtons[i];

            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();

            if (Application.isPlaying)
                Destroy(button.gameObject);
            else
                DestroyImmediate(button.gameObject);
        }

        generatedOptionButtons.Clear();
    }

    private void RefreshMainButtonText()
    {
        if (!setButtonTextToSelectedOption || mainButtonText == null)
            return;

        if (options == null || options.Count == 0)
        {
            mainButtonText.text = captionPrefix;
            return;
        }

        int safeIndex = Mathf.Clamp(currentIndex, 0, options.Count - 1);

        if (string.IsNullOrEmpty(captionPrefix))
            mainButtonText.text = options[safeIndex];
        else
            mainButtonText.text = captionPrefix + options[safeIndex];
    }

    private void DestroyOldPanel()
    {
        if (transform.parent == null)
            return;

        Transform oldPanel = transform.parent.Find(GeneratedPanelName);

        if (oldPanel == null)
            return;

        if (Application.isPlaying)
            Destroy(oldPanel.gameObject);
        else
            DestroyImmediate(oldPanel.gameObject);
    }

    private void CopyButtonAnchorsToPanel()
    {
        if (buttonRect == null || panelRect == null)
            return;

        panelRect.anchorMin = buttonRect.anchorMin;
        panelRect.anchorMax = buttonRect.anchorMax;
        panelRect.pivot = new Vector2(buttonRect.pivot.x, 1f);
        panelRect.sizeDelta = new Vector2(buttonRect.rect.width, panelMaxHeight);
    }

    private void PositionPanelUnderButton()
    {
        if (buttonRect == null || panelRect == null)
            return;

        Vector2 buttonPosition = buttonRect.anchoredPosition;
        float offsetY = -buttonRect.rect.height - 4f;

        panelRect.anchoredPosition = new Vector2(buttonPosition.x, buttonPosition.y + offsetY);
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.layer = gameObject.layer;
        return obj;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
