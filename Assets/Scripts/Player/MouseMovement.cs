using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{

    public float mouseSensitivity = 100f;

    public Camera camera;

    float xRotation = 0f;
    float yRotation = 0f;

    float topClamp = -90f;
    float botClamp = 90f;
    private PlayerCameraMotion cameraMotion;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (camera != null)
        {
            cameraMotion = camera.GetComponent<PlayerCameraMotion>();

            if (cameraMotion == null)
            {
                cameraMotion = camera.gameObject.AddComponent<PlayerCameraMotion>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
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
}
