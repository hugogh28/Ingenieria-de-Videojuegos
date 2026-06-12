using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class TrapezoidMaskGraphic : MaskableGraphic
{
    [Header("Top Edge Insets")]
    [SerializeField] [Range(-0.49f, 0.49f)] private float topLeftInset01 = 0.08f;
    [SerializeField] [Range(-0.49f, 0.49f)] private float topRightInset01 = 0.08f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        Color32 vertexColor = color;

        float xMin = rect.xMin;
        float xMax = rect.xMax;
        float yMin = rect.yMin;
        float yMax = rect.yMax;

        float topLeftX = xMin + rect.width * topLeftInset01;
        float topRightX = xMax - rect.width * topRightInset01;

        vh.AddVert(new Vector3(xMin, yMin), vertexColor, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(topLeftX, yMax), vertexColor, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(topRightX, yMax), vertexColor, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(xMax, yMin), vertexColor, new Vector2(1f, 0f));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
