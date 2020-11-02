using UnityEngine;
using UnityEngine.UI;

public class SkewedImage : Image
{
    [SerializeField]
    public float skewX;
    [SerializeField]
    public float skewY;

    Vector4 utilV4 = Vector4.zero;
    Vector3 utilV3 = Vector3.zero;
    Vector2 utilV2 = Vector2.zero;

    Color32 color32;

    Rect rect;


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        rect = GetPixelAdjustedRect();

        utilV4.x = rect.x;
        utilV4.y = rect.y;
        utilV4.z = rect.x + rect.width;
        utilV4.w = rect.y + rect.height;

        color32 = color;

        vh.Clear();

        utilV3.x = utilV4.x - skewX;
        utilV3.y = utilV4.y - skewY;

        utilV2.x = 0F;
        utilV2.y = 0F;
        vh.AddVert(utilV3, color32, utilV2);

        utilV3.x = utilV4.x + skewX;
        utilV3.y = utilV4.w - skewY;

        utilV2.x = 0F;
        utilV2.y = 1F;
        vh.AddVert(utilV3, color32, utilV2);

        utilV3.x = utilV4.z + skewX;
        utilV3.y = utilV4.w + skewY;

        utilV2.x = 1F;
        utilV2.y = 1F;
        vh.AddVert(utilV3, color32, utilV2);
                
        utilV3.x = utilV4.z - skewX;
        utilV3.y = utilV4.y + skewY;

        utilV2.x = 1F;
        utilV2.y = 0F;
        vh.AddVert(utilV3, color32, utilV2);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}
