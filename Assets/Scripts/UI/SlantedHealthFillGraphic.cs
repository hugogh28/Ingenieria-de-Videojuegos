using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI fill graphic whose right edge keeps a constant slanted angle while the fill amount changes.
/// Put this component on the red Bar object, inside a fixed trapezoid Mask.
/// Do not animate or resize the RectTransform width at runtime.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class SlantedHealthFillGraphic : MaskableGraphic
{
    [Header("Fill")]
    [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;

    [Header("Shape")]
    [Tooltip("Horizontal offset, in UI pixels, between the bottom and top point of the moving right edge. Positive values make the top point move left.")]
    [SerializeField, Min(0f)] private float rightSideTopInset = 12f;

    [Tooltip("Extra geometry drawn to the left so the fixed trapezoid mask, not this mesh, defines the left border.")]
    [SerializeField, Min(0f)] private float leftOverflow = 64f;

    [Tooltip("Enable if your trapezoid right edge leans in the opposite direction.")]
    [SerializeField] private bool invertSlant;

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            float clamped = Mathf.Clamp01(value);

            if (Mathf.Approximately(fillAmount, clamped))
                return;

            fillAmount = clamped;
            SetVerticesDirty();
        }
    }

    public float RightSideTopInset
    {
        get => rightSideTopInset;
        set
        {
            rightSideTopInset = Mathf.Max(0f, value);
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (fillAmount <= 0.0001f)
            return;

        Rect rect = GetPixelAdjustedRect();

        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;
        float width = rect.width;

        float bottomRightX = xMin + width * fillAmount;
        float topRightX = invertSlant
            ? bottomRightX + rightSideTopInset
            : bottomRightX - rightSideTopInset;

        // The left side is intentionally oversized. The parent Mask keeps the visible left side trapezoidal.
        float bottomLeftX = xMin - leftOverflow;
        float topLeftX = invertSlant
            ? bottomLeftX + rightSideTopInset
            : bottomLeftX - rightSideTopInset;

        AddQuad(vh,
            new Vector2(bottomLeftX, yMin),
            new Vector2(topLeftX, yMax),
            new Vector2(topRightX, yMax),
            new Vector2(bottomRightX, yMin));
    }

    private void AddQuad(VertexHelper vh, Vector2 bottomLeft, Vector2 topLeft, Vector2 topRight, Vector2 bottomRight)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = bottomLeft;
        vertex.uv0 = new Vector2(0f, 0f);
        vh.AddVert(vertex);

        vertex.position = topLeft;
        vertex.uv0 = new Vector2(0f, 1f);
        vh.AddVert(vertex);

        vertex.position = topRight;
        vertex.uv0 = new Vector2(1f, 1f);
        vh.AddVert(vertex);

        vertex.position = bottomRight;
        vertex.uv0 = new Vector2(1f, 0f);
        vh.AddVert(vertex);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        fillAmount = Mathf.Clamp01(fillAmount);
        rightSideTopInset = Mathf.Max(0f, rightSideTopInset);
        leftOverflow = Mathf.Max(0f, leftOverflow);
        SetVerticesDirty();
    }
#endif
}
