using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float lifetime = 0.75f;
    [SerializeField] private float riseSpeed = 1.7f;
    [SerializeField] private float sideDrift = 0.35f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0.75f, 1f, 1f);

    private TextMeshPro text;
    private Camera targetCamera;
    private Vector3 drift;
    private float elapsed;
    private float baseScale = 1f;
    private Renderer textRenderer;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();

        if (text == null)
        {
            text = gameObject.AddComponent<TextMeshPro>();
        }

        textRenderer = GetComponent<Renderer>();
    }

    public void Show(
        int damage,
        bool isCritical,
        Color color,
        float scale,
        Camera cameraToFace,
        TMP_FontAsset fontAsset,
        bool forceBold,
        int sortingOrder
    )
    {
        targetCamera = cameraToFace != null ? cameraToFace : Camera.main;
        baseScale = scale;
        drift = new Vector3(Random.Range(-sideDrift, sideDrift), riseSpeed, Random.Range(-sideDrift, sideDrift));

        text.text = isCritical ? $"{damage}!" : damage.ToString();
        text.color = color;
        text.fontStyle = forceBold || isCritical ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;

        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        if (textRenderer == null)
        {
            textRenderer = GetComponent<Renderer>();
        }

        if (textRenderer != null)
        {
            textRenderer.sortingOrder = sortingOrder;
        }

        transform.localScale = Vector3.one * baseScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        transform.position += drift * Time.deltaTime;

        if (targetCamera != null)
        {
            Vector3 direction = transform.position - targetCamera.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        float scale = baseScale * scaleCurve.Evaluate(t);
        transform.localScale = Vector3.one * scale;

        Color color = text.color;
        color.a = 1f - t;
        text.color = color;

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
