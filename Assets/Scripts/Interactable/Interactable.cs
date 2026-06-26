using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    [Header("Texto de interacción")]
    [SerializeField] private string actionText = "interactuar";
    [SerializeField] private bool appendCostToActionText = true;
    [SerializeField] private string costTextFormat = " ({0} quesos)";

    [Header("Uso")]
    [SerializeField] private bool interactOnce = false;
    [SerializeField] private bool hideAfterUse = true;
    [SerializeField] private bool disableComponentAfterUse = false;

    [Header("Coste de puntos")]
    [SerializeField] private bool usePointCost = false;
    [SerializeField] private int pointCost = 100;

    [Tooltip("Arrastra aquí el componente que gestiona los puntos. Si lo dejas vacío, el script intentará encontrarlo en el PlayerController recibido por InteractionManager.")]
    [SerializeField] private MonoBehaviour pointsComponent;

    [Tooltip("Método recomendado para gastar puntos. Firma ideal: public bool TrySpendPoints(int amount).")]
    [SerializeField] private string spendMethodName = "TrySpendPoints";

    [Tooltip("Campo o propiedad donde se guardan los puntos si no usas método. Ejemplos: Points, CurrentPoints, Score, score.")]
    [SerializeField] private string pointsValueName = "Points";

    [Tooltip("Nombres alternativos que se prueban si Points Value Name no existe.")]
    [SerializeField] private string fallbackPointsValueNames = "CurrentPoints,Score,score,currentScore,Puntos,puntos";

    [Tooltip("Si está activado, el prompt aparece aunque no tengas puntos. Al pulsar, muestra el aviso de puntos insuficientes.")]
    [SerializeField] private bool allowInteractionAttemptWithoutEnoughPoints = true;

    [Header("Mensajes de interfaz")]
    [SerializeField] private bool showFeedbackMessages = true;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float feedbackDuration = 1.5f;
    [SerializeField] private string notEnoughPointsMessage = "No tienes puntos suficientes";
    [SerializeField] private string alreadyUsedMessage = "Ya se ha usado";
    [SerializeField] private string missingPointsSystemMessage = "No se ha encontrado el sistema de puntos";

    [Header("Eventos tipo botón")]
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onNotEnoughPoints;
    [SerializeField] private UnityEvent onAlreadyUsed;
    [SerializeField] private UnityEvent onMissingPointsSystem;
    [SerializeField] private StringEvent onFeedbackMessage;

    private bool alreadyUsed;
    private Coroutine feedbackRoutine;
    private MonoBehaviour cachedResolvedPointsComponent;

    public string InteractionActionText
    {
        get
        {
            if (usePointCost && appendCostToActionText && pointCost > 0)
            {
                return actionText + string.Format(costTextFormat, pointCost);
            }

            return actionText;
        }
    }

    public bool AlreadyUsed => alreadyUsed;
    public int PointCost => pointCost;
    public bool UsesPointCost => usePointCost;

    private void Reset()
    {
        tag = "Interactable";

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // InteractionManager usa Physics.Raycast con QueryTriggerInteraction.Ignore.
            // Por eso el collider debe ser físico normal, no trigger.
            col.isTrigger = false;
        }

        Outline outline = GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public bool CanInteract(PlayerController player)
    {
        if (!enabled || !gameObject.activeInHierarchy)
        {
            return false;
        }

        if (interactOnce && alreadyUsed && hideAfterUse)
        {
            return false;
        }

        if (!usePointCost || pointCost <= 0)
        {
            return true;
        }

        if (allowInteractionAttemptWithoutEnoughPoints)
        {
            return true;
        }

        return HasEnoughPoints(player);
    }

    public void Interact(PlayerController player)
    {
        if (interactOnce && alreadyUsed)
        {
            ShowFeedback(alreadyUsedMessage);
            onAlreadyUsed?.Invoke();
            return;
        }

        PointPaymentResult paymentResult = TryPayCost(player);

        if (paymentResult == PointPaymentResult.MissingPointsSystem)
        {
            ShowFeedback(missingPointsSystemMessage);
            onMissingPointsSystem?.Invoke();
            return;
        }

        if (paymentResult == PointPaymentResult.NotEnoughPoints)
        {
            ShowFeedback(notEnoughPointsMessage);
            onNotEnoughPoints?.Invoke();
            return;
        }

        alreadyUsed = true;
        onInteract?.Invoke();

        if (disableComponentAfterUse)
        {
            enabled = false;
        }
    }

    public void ResetUsage()
    {
        alreadyUsed = false;
        enabled = true;
    }

    public void ForceInteractWithoutCost()
    {
        if (interactOnce && alreadyUsed)
        {
            onAlreadyUsed?.Invoke();
            return;
        }

        alreadyUsed = true;
        onInteract?.Invoke();
    }

    private bool HasEnoughPoints(PlayerController player)
    {
        MonoBehaviour points = ResolvePointsComponent(player);
        if (points == null)
        {
            return false;
        }

        if (!TryReadPoints(points, out int currentPoints))
        {
            return false;
        }

        return currentPoints >= pointCost;
    }

    private PointPaymentResult TryPayCost(PlayerController player)
    {
        if (!usePointCost || pointCost <= 0)
        {
            return PointPaymentResult.Paid;
        }

        MonoBehaviour points = ResolvePointsComponent(player);
        if (points == null)
        {
            Debug.LogWarning($"[{nameof(Interactable)}] Use Point Cost está activado, pero no se ha asignado ni encontrado un componente de puntos.", this);
            return PointPaymentResult.MissingPointsSystem;
        }

        SpendMethodResult spendResult = TryCallSpendMethod(points, spendMethodName, pointCost);

        if (spendResult == SpendMethodResult.Paid)
        {
            return PointPaymentResult.Paid;
        }

        if (spendResult == SpendMethodResult.NotEnoughPoints)
        {
            return PointPaymentResult.NotEnoughPoints;
        }

        if (!TryReadPoints(points, out int currentPoints))
        {
            Debug.LogWarning($"[{nameof(Interactable)}] No se pudo leer el valor de puntos en {points.GetType().Name}.", this);
            return PointPaymentResult.MissingPointsSystem;
        }

        if (currentPoints < pointCost)
        {
            return PointPaymentResult.NotEnoughPoints;
        }

        if (spendResult == SpendMethodResult.VoidMethodNeedsPointCheck)
        {
            TryCallVoidSpendMethod(points, spendMethodName, pointCost);
            return PointPaymentResult.Paid;
        }

        if (!TryWritePoints(points, currentPoints - pointCost))
        {
            Debug.LogWarning($"[{nameof(Interactable)}] Hay puntos suficientes, pero no se pudo restar el coste. Lo más seguro es crear un método bool TrySpendPoints(int amount) en tu gestor de puntos.", this);
            return PointPaymentResult.MissingPointsSystem;
        }

        return PointPaymentResult.Paid;
    }

    private MonoBehaviour ResolvePointsComponent(PlayerController player)
    {
        if (pointsComponent != null)
        {
            return pointsComponent;
        }

        if (cachedResolvedPointsComponent != null)
        {
            return cachedResolvedPointsComponent;
        }

        if (player == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = player.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            if (FindMethod(behaviour, spendMethodName) != null)
            {
                cachedResolvedPointsComponent = behaviour;
                return cachedResolvedPointsComponent;
            }
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            if (HasReadablePointsValue(behaviour))
            {
                cachedResolvedPointsComponent = behaviour;
                return cachedResolvedPointsComponent;
            }
        }

        return null;
    }

    private enum PointPaymentResult
    {
        Paid,
        NotEnoughPoints,
        MissingPointsSystem
    }

    private enum SpendMethodResult
    {
        MethodNotFound,
        Paid,
        NotEnoughPoints,
        VoidMethodNeedsPointCheck
    }

    private SpendMethodResult TryCallSpendMethod(MonoBehaviour target, string methodName, int amount)
    {
        MethodInfo method = FindMethod(target, methodName);
        if (method == null)
        {
            return SpendMethodResult.MethodNotFound;
        }

        if (method.ReturnType == typeof(bool))
        {
            object result = method.Invoke(target, new object[] { amount });
            return (bool)result ? SpendMethodResult.Paid : SpendMethodResult.NotEnoughPoints;
        }

        if (method.ReturnType == typeof(void))
        {
            return SpendMethodResult.VoidMethodNeedsPointCheck;
        }

        Debug.LogWarning($"[{nameof(Interactable)}] El método {methodName} debe devolver bool o void.", this);
        return SpendMethodResult.MethodNotFound;
    }

    private void TryCallVoidSpendMethod(MonoBehaviour target, string methodName, int amount)
    {
        MethodInfo method = FindMethod(target, methodName);
        method?.Invoke(target, new object[] { amount });
    }

    private MethodInfo FindMethod(MonoBehaviour target, string methodName)
    {
        if (target == null || string.IsNullOrEmpty(methodName) || methodName.Trim().Length == 0)
        {
            return null;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return target.GetType().GetMethod(methodName, flags, null, new[] { typeof(int) }, null);
    }

    private bool HasReadablePointsValue(MonoBehaviour target)
    {
        return TryReadPoints(target, out _);
    }

    private bool TryReadPoints(MonoBehaviour target, out int points)
    {
        points = 0;

        foreach (string valueName in GetPointValueNames())
        {
            if (TryReadNamedValue(target, valueName, out points))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryReadNamedValue(MonoBehaviour target, string valueName, out int points)
    {
        points = 0;

        if (target == null || string.IsNullOrEmpty(valueName) || valueName.Trim().Length == 0)
        {
            return false;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();

        PropertyInfo property = type.GetProperty(valueName.Trim(), flags);
        if (property != null && property.CanRead)
        {
            object value = property.GetValue(target);
            return TryConvertToInt(value, out points);
        }

        FieldInfo field = type.GetField(valueName.Trim(), flags);
        if (field != null)
        {
            object value = field.GetValue(target);
            return TryConvertToInt(value, out points);
        }

        return false;
    }

    private bool TryWritePoints(MonoBehaviour target, int newValue)
    {
        foreach (string valueName in GetPointValueNames())
        {
            if (TryWriteNamedValue(target, valueName, newValue))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryWriteNamedValue(MonoBehaviour target, string valueName, int newValue)
    {
        if (target == null || string.IsNullOrEmpty(valueName) || valueName.Trim().Length == 0)
        {
            return false;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();
        string cleanName = valueName.Trim();

        PropertyInfo property = type.GetProperty(cleanName, flags);
        if (property != null && property.CanWrite)
        {
            object convertedValue = Convert.ChangeType(newValue, property.PropertyType);
            property.SetValue(target, convertedValue);
            return true;
        }

        FieldInfo field = type.GetField(cleanName, flags);
        if (field != null && !field.IsInitOnly)
        {
            object convertedValue = Convert.ChangeType(newValue, field.FieldType);
            field.SetValue(target, convertedValue);
            return true;
        }

        return false;
    }

    private string[] GetPointValueNames()
    {
        string joinedNames = pointsValueName;

        if (!string.IsNullOrEmpty(fallbackPointsValueNames))
        {
            joinedNames += "," + fallbackPointsValueNames;
        }

        return joinedNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private bool TryConvertToInt(object value, out int result)
    {
        result = 0;

        if (value == null)
        {
            return false;
        }

        try
        {
            result = Convert.ToInt32(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ShowFeedback(string message)
    {
        if (!showFeedbackMessages || string.IsNullOrEmpty(message))
        {
            return;
        }

        onFeedbackMessage?.Invoke(message);
        TryShowThroughInteractionManager(message);

        if (feedbackText != null)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(FeedbackTextRoutine(message));
        }
    }

    private void TryShowThroughInteractionManager(string message)
    {
        InteractionManager manager = InteractionManager.Instance;
        if (manager == null)
        {
            return;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo method = typeof(InteractionManager).GetMethod("ShowTemporaryMessage", flags, null, new[] { typeof(string), typeof(float) }, null);

        if (method != null)
        {
            method.Invoke(manager, new object[] { message, feedbackDuration });
        }
    }

    private IEnumerator FeedbackTextRoutine(string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;

        yield return new WaitForSeconds(feedbackDuration);

        feedbackText.text = string.Empty;
        feedbackText.gameObject.SetActive(false);
        feedbackRoutine = null;
    }
}
