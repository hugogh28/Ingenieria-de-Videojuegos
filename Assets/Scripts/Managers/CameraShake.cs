using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Coroutine shakeRoutine;
    private Vector3 originalLocalPosition;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    public void Shake(float intensity, float duration)
    {
        if (intensity <= 0f || duration <= 0f)
        {
            return;
        }

        PlayerCameraMotion cameraMotion = GetComponent<PlayerCameraMotion>();

        if (cameraMotion != null)
        {
            cameraMotion.PlayShake(intensity, duration);
            return;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalLocalPosition;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / duration);
            Vector3 offset = Random.insideUnitSphere * (intensity * fade);
            offset.z = 0f;
            transform.localPosition = originalLocalPosition + offset;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        shakeRoutine = null;
    }
}
