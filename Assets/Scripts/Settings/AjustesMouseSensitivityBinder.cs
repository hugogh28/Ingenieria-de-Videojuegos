using UnityEngine;

public class AjustesMouseSensitivityBinder : MonoBehaviour
{
    [SerializeField] private MouseMovement mouseMovement;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private bool updateEveryFrame;

    private Ajustes ajustes;

    private void Awake()
    {
        if (mouseMovement == null)
        {
            mouseMovement = GetComponent<MouseMovement>();
        }

        if (mouseMovement == null)
        {
            mouseMovement = FindFirstObjectByType<MouseMovement>();
        }
    }

    private void OnEnable()
    {
        ajustes = Ajustes.EnsureExists();
        ajustes.Changed += ApplySensitivity;
        ApplySensitivity();
    }

    private void OnDisable()
    {
        if (ajustes != null)
        {
            ajustes.Changed -= ApplySensitivity;
        }
    }

    private void Update()
    {
        if (updateEveryFrame)
        {
            ApplySensitivity();
        }
    }

    public void ApplySensitivity()
    {
        if (mouseMovement == null)
        {
            return;
        }

        if (ajustes == null)
        {
            ajustes = Ajustes.EnsureExists();
        }

        mouseMovement.mouseSensitivity = ajustes.MouseSensitivity * multiplier;
    }
}
