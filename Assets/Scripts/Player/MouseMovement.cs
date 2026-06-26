using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [Header("Sensitivity")]
    [Tooltip("Sensibilidad real usada por la cámara. Si useAjustesSensitivity está activo, se sobrescribe con Ajustes.MouseSensitivity.")]
    public float mouseSensitivity = 500f;

    [SerializeField] private bool useAjustesSensitivity = true;
    [SerializeField] private bool debugSensitivity = false;

    [Header("References")]
    public Camera camera;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private float topClamp = -90f;
    private float botClamp = 90f;

    private PlayerCameraMotion cameraMotion;
    private Ajustes ajustes;

    private void OnEnable()
    {
        if (!useAjustesSensitivity)
        {
            return;
        }

        ajustes = Ajustes.EnsureExists();

        if (ajustes != null)
        {
            ajustes.Changed += ApplyAjustesSensitivity;
            ApplyAjustesSensitivity();
        }
    }

    private void OnDisable()
    {
        if (ajustes != null)
        {
            ajustes.Changed -= ApplyAjustesSensitivity;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera != null)
        {
            cameraMotion = camera.GetComponent<PlayerCameraMotion>();

            if (cameraMotion == null)
            {
                cameraMotion = camera.gameObject.AddComponent<PlayerCameraMotion>();
            }
        }

        ApplyAjustesSensitivity();
    }

    private void Update()
    {
        if (camera == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topClamp, botClamp);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        Quaternion cameraRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (cameraMotion != null)
        {
            cameraRotation = cameraMotion.ApplyAnimatedRotation(cameraRotation);
        }

        camera.transform.localRotation = cameraRotation;
    }

    private void ApplyAjustesSensitivity()
    {
        if (!useAjustesSensitivity || Ajustes.Instance == null)
        {
            return;
        }

        mouseSensitivity = Ajustes.Instance.MouseSensitivity;

        if (debugSensitivity)
        {
            Debug.Log($"MouseMovement sensibilidad aplicada: {mouseSensitivity}", this);
        }
    }
}
