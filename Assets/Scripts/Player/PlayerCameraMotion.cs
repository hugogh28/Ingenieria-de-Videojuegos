using System.Collections;
using UnityEngine;

public class PlayerCameraMotion : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float positionSmooth = 12f;
    [SerializeField] private float rotationSmooth = 12f;

    [Header("Run Bob")]
    [SerializeField] private float runBobSpeed = 10f;
    [SerializeField] private float runBobAmountY = 0.055f;
    [SerializeField] private float runBobAmountX = 0.025f;
    [SerializeField] private float runRollAmount = 1.25f;

    [Header("Jump")]
    [SerializeField] private float jumpKickY = -0.06f;
    [SerializeField] private float jumpPitch = -1.6f;
    [SerializeField] private float jumpImpulseReturnSpeed = 7f;

    [Header("Fall")]
    [SerializeField] private float fallOffsetY = -0.075f;
    [SerializeField] private float fallPitch = 1.4f;
    [SerializeField] private float fallVelocityForFullEffect = -12f;

    [Header("Land")]
    [SerializeField] private float landKickY = -0.12f;
    [SerializeField] private float landPitch = 2.2f;
    [SerializeField] private float landImpulseReturnSpeed = 8f;

    [Header("Damage")]
    [SerializeField] private float damageKickX = 0.06f;
    [SerializeField] private float damageKickY = -0.045f;
    [SerializeField] private float damageRoll = 2.4f;
    [SerializeField] private float damageImpulseReturnSpeed = 10f;

    private Vector3 baseLocalPosition;
    private Vector3 currentPositionOffset;
    private Vector3 targetPositionOffset;
    private Vector3 impulsePositionOffset;
    private Vector3 currentEulerOffset;
    private Vector3 targetEulerOffset;
    private Vector3 impulseEulerOffset;
    private Vector3 shakeOffset;
    private Coroutine shakeRoutine;

    private float bobTimer;
    private float verticalVelocity;
    private float movementAmount;
    private bool isGrounded = true;
    private bool isMoving;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        UpdateContinuousMotion();
        UpdateImpulseMotion();

        currentPositionOffset = Vector3.Lerp(
            currentPositionOffset,
            targetPositionOffset + impulsePositionOffset,
            Time.deltaTime * positionSmooth
        );

        currentEulerOffset = Vector3.Lerp(
            currentEulerOffset,
            targetEulerOffset + impulseEulerOffset,
            Time.deltaTime * rotationSmooth
        );

        transform.localPosition = baseLocalPosition + currentPositionOffset + shakeOffset;
    }

    public void SetMovementState(bool grounded, bool moving, float yVelocity, float moveAmount)
    {
        isGrounded = grounded;
        isMoving = moving;
        verticalVelocity = yVelocity;
        movementAmount = Mathf.Clamp01(moveAmount);
    }

    public Quaternion ApplyAnimatedRotation(Quaternion baseRotation)
    {
        return baseRotation * Quaternion.Euler(currentEulerOffset);
    }

    public void PlayJumpImpulse()
    {
        impulsePositionOffset += Vector3.up * jumpKickY;
        impulseEulerOffset += Vector3.right * jumpPitch;
    }

    public void PlayLandImpulse(float fallSpeed)
    {
        float strength = Mathf.InverseLerp(2f, 14f, fallSpeed);
        impulsePositionOffset += Vector3.up * (landKickY * strength);
        impulseEulerOffset += Vector3.right * (landPitch * strength);
    }

    public void PlayDamageImpulse()
    {
        float side = Random.value < 0.5f ? -1f : 1f;
        impulsePositionOffset += new Vector3(damageKickX * side, damageKickY, 0f);
        impulseEulerOffset += Vector3.forward * (damageRoll * -side);
    }

    public void PlayShake(float intensity, float duration)
    {
        if (intensity <= 0f || duration <= 0f)
        {
            return;
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private void UpdateContinuousMotion()
    {
        targetPositionOffset = Vector3.zero;
        targetEulerOffset = Vector3.zero;

        if (isGrounded && isMoving)
        {
            bobTimer += Time.deltaTime * runBobSpeed * Mathf.Lerp(0.55f, 1.25f, movementAmount);
            float bobSin = Mathf.Sin(bobTimer);
            float bobCos = Mathf.Cos(bobTimer * 0.5f);

            targetPositionOffset += new Vector3(
                bobCos * runBobAmountX,
                Mathf.Abs(bobSin) * runBobAmountY,
                0f
            );

            targetEulerOffset += Vector3.forward * (-bobCos * runRollAmount);
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * 4f);
        }

        if (!isGrounded && verticalVelocity < -0.1f)
        {
            float fallStrength = Mathf.InverseLerp(-0.1f, fallVelocityForFullEffect, verticalVelocity);
            targetPositionOffset += Vector3.up * (fallOffsetY * fallStrength);
            targetEulerOffset += Vector3.right * (fallPitch * fallStrength);
        }
    }

    private void UpdateImpulseMotion()
    {
        float returnSpeed = Mathf.Max(jumpImpulseReturnSpeed, landImpulseReturnSpeed);
        impulsePositionOffset = Vector3.Lerp(impulsePositionOffset, Vector3.zero, Time.deltaTime * Mathf.Max(returnSpeed, damageImpulseReturnSpeed));
        impulseEulerOffset = Vector3.Lerp(impulseEulerOffset, Vector3.zero, Time.deltaTime * Mathf.Max(returnSpeed, damageImpulseReturnSpeed));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / duration);
            shakeOffset = Random.insideUnitSphere * (intensity * fade);
            shakeOffset.z = 0f;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}
