using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Versión corregida para TMP_Dropdown.
/// No toca la jerarquía en PointerDown, porque eso puede hacer que el botón se oscurezca
/// pero la lista no se despliegue.
/// Espera a que TMP_Dropdown procese el click, busca la "Dropdown List" generada,
/// y entonces la pone por delante.
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownBringToFront : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [Header("Lista desplegada")]
    [SerializeField] private bool reparentListToRootCanvas = true;
    [SerializeField] private bool addOverrideCanvasToList = true;
    [SerializeField] private int popupSortingOrder = 5000;

    [Header("Fallback")]
    [SerializeField] private bool forceOpenIfListWasNotCreated = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private TMP_Dropdown dropdown;
    private Coroutine routine;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ScheduleFixAfterClick();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        ScheduleFixAfterClick();
    }

    private void ScheduleFixAfterClick()
    {
        if (!isActiveAndEnabled)
            return;

        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown == null || !dropdown.interactable)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FixAfterDropdownClick());
    }

    private IEnumerator FixAfterDropdownClick()
    {
        // Espera a que TMP_Dropdown procese internamente el click y cree la lista.
        yield return null;
        yield return new WaitForEndOfFrame();

        Transform list = FindDropdownList();

        // Si por cualquier motivo el click no ha llegado a abrir la lista, la fuerza.
        if (list == null && forceOpenIfListWasNotCreated)
        {
            if (debugLogs)
                Debug.Log("[DropdownBringToFront] No se encontró Dropdown List. Forzando dropdown.Show().", this);

            dropdown.Show();

            yield return null;
            yield return new WaitForEndOfFrame();

            list = FindDropdownList();
        }

        if (list == null)
        {
            if (debugLogs)
                Debug.LogWarning("[DropdownBringToFront] No se pudo encontrar Dropdown List. Revisa el Template del TMP_Dropdown.", this);

            routine = null;
            yield break;
        }

        PutListInFront(list);

        // Espera hasta que la lista se cierre o se destruya.
        while (isActiveAndEnabled && list != null && list.gameObject != null)
            yield return null;

        routine = null;
    }

    private Transform FindDropdownList()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        // Caso normal: TMP_Dropdown crea la lista en el padre del Template.
        if (dropdown != null && dropdown.template != null && dropdown.template.parent != null)
        {
            Transform templateParent = dropdown.template.parent;
            Transform found = FindChildStartingWith(templateParent, "Dropdown List");

            if (found != null)
                return found;
        }

        // Fallback: buscar en el padre del dropdown.
        if (transform.parent != null)
        {
            Transform found = FindChildStartingWith(transform.parent, "Dropdown List");

            if (found != null)
                return found;
        }

        // Fallback extra: buscar en el Canvas raíz.
        Canvas rootCanvas = GetRootCanvas();

        if (rootCanvas != null)
        {
            Transform found = FindChildStartingWith(rootCanvas.transform, "Dropdown List");

            if (found != null)
                return found;
        }

        return null;
    }

    private Transform FindChildStartingWith(Transform parent, string nameStart)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child != null && child.name.StartsWith(nameStart))
                return child;
        }

        return null;
    }

    private void PutListInFront(Transform list)
    {
        Canvas rootCanvas = GetRootCanvas();

        // Sacar la lista al Canvas raíz evita que la recorte un panel con Mask/RectMask2D
        // y evita que quede detrás de otros elementos del menú.
        if (reparentListToRootCanvas && rootCanvas != null && list.parent != rootCanvas.transform)
        {
            list.SetParent(rootCanvas.transform, true);
        }

        list.SetAsLastSibling();

        if (addOverrideCanvasToList)
        {
            Canvas listCanvas = list.GetComponent<Canvas>();

            if (listCanvas == null)
                listCanvas = list.gameObject.AddComponent<Canvas>();

            listCanvas.overrideSorting = true;
            listCanvas.sortingOrder = popupSortingOrder;

            if (rootCanvas != null)
            {
                listCanvas.sortingLayerID = rootCanvas.sortingLayerID;
            }

            if (list.GetComponent<GraphicRaycaster>() == null)
                list.gameObject.AddComponent<GraphicRaycaster>();
        }

        if (debugLogs)
            Debug.Log("[DropdownBringToFront] Dropdown List puesta por delante.", this);
    }

    private Canvas GetRootCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return null;

        return canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        // No llamar aquí a SetSiblingIndex ni SetAsLastSibling.
        // Unity puede estar activando/desactivando el padre.
    }
}
