using System.Reflection;
using UnityEngine;

public class AjustesMouseSensitivityBinder : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("Arrastra aquí el componente que tenga la variable de sensibilidad, por ejemplo MouseMovement.")]
    [SerializeField] private MonoBehaviour targetComponent;

    [Tooltip("Nombre del campo o propiedad de sensibilidad en el componente destino.")]
    [SerializeField] private string memberName = "mouseSensitivity";

    [SerializeField] private bool autoFindTargetOnSameObject = true;
    [SerializeField] private bool warnIfMemberNotFound = true;

    private Ajustes settings;
    private FieldInfo cachedField;
    private PropertyInfo cachedProperty;
    private System.Type cachedTargetType;

    private void OnEnable()
    {
        settings = Ajustes.EnsureExists();
        settings.Changed += ApplySensitivity;

        AutoFindTargetIfNeeded();
        ResolveMember();
        ApplySensitivity();
    }

    private void OnDisable()
    {
        if (settings != null)
        {
            settings.Changed -= ApplySensitivity;
        }
    }

    public void ApplySensitivity()
    {
        if (settings == null)
        {
            settings = Ajustes.EnsureExists();
        }

        if (targetComponent == null || string.IsNullOrEmpty(memberName))
        {
            return;
        }

        ResolveMember();

        float realSensitivity = settings.MouseSensitivity;

        if (cachedField != null)
        {
            SetFieldValue(cachedField, targetComponent, realSensitivity);
            return;
        }

        if (cachedProperty != null && cachedProperty.CanWrite)
        {
            SetPropertyValue(cachedProperty, targetComponent, realSensitivity);
            return;
        }

        if (warnIfMemberNotFound)
        {
            Debug.LogWarning($"AjustesMouseSensitivityBinder: no se encontró '{memberName}' en {targetComponent.GetType().Name}.", this);
        }
    }

    private void AutoFindTargetIfNeeded()
    {
        if (!autoFindTargetOnSameObject || targetComponent != null)
        {
            return;
        }

        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour == this)
            {
                continue;
            }

            if (HasWritableMember(behaviour.GetType(), memberName))
            {
                targetComponent = behaviour;
                return;
            }
        }
    }

    private void ResolveMember()
    {
        if (targetComponent == null)
        {
            cachedField = null;
            cachedProperty = null;
            cachedTargetType = null;
            return;
        }

        System.Type targetType = targetComponent.GetType();
        if (cachedTargetType == targetType && (cachedField != null || cachedProperty != null))
        {
            return;
        }

        cachedTargetType = targetType;
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        cachedField = targetType.GetField(memberName, flags);
        cachedProperty = targetType.GetProperty(memberName, flags);
    }

    private bool HasWritableMember(System.Type type, string name)
    {
        if (type == null || string.IsNullOrEmpty(name))
        {
            return false;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = type.GetField(name, flags);
        if (field != null && IsSupportedNumericType(field.FieldType))
        {
            return true;
        }

        PropertyInfo property = type.GetProperty(name, flags);
        return property != null && property.CanWrite && IsSupportedNumericType(property.PropertyType);
    }

    private void SetFieldValue(FieldInfo field, object target, float value)
    {
        if (!IsSupportedNumericType(field.FieldType))
        {
            return;
        }

        if (field.FieldType == typeof(float)) field.SetValue(target, value);
        else if (field.FieldType == typeof(double)) field.SetValue(target, (double)value);
        else if (field.FieldType == typeof(int)) field.SetValue(target, Mathf.RoundToInt(value));
    }

    private void SetPropertyValue(PropertyInfo property, object target, float value)
    {
        if (!IsSupportedNumericType(property.PropertyType))
        {
            return;
        }

        if (property.PropertyType == typeof(float)) property.SetValue(target, value);
        else if (property.PropertyType == typeof(double)) property.SetValue(target, (double)value);
        else if (property.PropertyType == typeof(int)) property.SetValue(target, Mathf.RoundToInt(value));
    }

    private bool IsSupportedNumericType(System.Type type)
    {
        return type == typeof(float) || type == typeof(double) || type == typeof(int);
    }
}
