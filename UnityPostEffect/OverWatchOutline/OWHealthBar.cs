using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Image", 11)]
public class Image : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
{
    public enum Type
    {
        Normal,
        Progress,
        Frame,
    }

    [SerializeField] private Type m_Type = Type.Normal;
    [SerializeField] private float m_DeltaX; //上边缘顶点x的偏移，可用于模拟平行四边形效果

    [SerializeField] private int m_MaxHp = 200; //最大血量
    [SerializeField] private int m_HpDelta = 25; //血量分割大小， 默认25点血一格

    [SerializeField] private float m_Gap = 1; //格子之间的间隔

    [SerializeField] private int m_WhiteHp; //白色血量
    [SerializeField] private int m_BlueHp; //蓝色血量
    [SerializeField] private int m_YellowHp; //黄色血量

    public Type type
    {
        get { return m_Type; }
        set { if (SetPropertyUtility.SetStruct(ref m_Type, value)) SetVerticesDirty(); }
    }

    public float DeltaX
    {
        get { return m_DeltaX; }
        set { if (SetPropertyUtility.SetStruct(ref m_DeltaX, value)) SetVerticesDirty(); }
    }

    public int MaxHp
    {
        get { return m_MaxHp; }
        set
        {
            if (SetPropertyUtility.SetStruct(ref m_MaxHp, value))
            {
                if (value == 0)
                {
                    m_MaxHp = 200;
                }
                SetVerticesDirty();
            }
        }
    }

    public int HpDelta
    {
        get { return m_HpDelta; }
        set { if (SetPropertyUtility.SetStruct(ref m_HpDelta, value)) SetVerticesDirty(); }
    }

    public float Gap
    {
        get { return m_Gap; }
        set { if (SetPropertyUtility.SetStruct(ref m_Gap, value)) SetVerticesDirty(); }
    }

    public int WhiteHp
    {
        get { return m_WhiteHp; }
        set { if (SetPropertyUtility.SetStruct(ref m_WhiteHp, value)) SetVerticesDirty(); }
    }

    public int BlueHp
    {
        get { return m_BlueHp; }
        set { if (SetPropertyUtility.SetStruct(ref m_BlueHp, value)) SetVerticesDirty(); }
    }

    public int YellowHp
    {
        get { return m_YellowHp; }
        set { if (SetPropertyUtility.SetStruct(ref m_YellowHp, value)) SetVerticesDirty(); }
    }

    protected Image()
    {
        useLegacyMeshGeneration = false;
    }

    /// <summary>
    /// Image's texture comes from the UnityEngine.Image.
    /// </summary>
    public override Texture mainTexture
    {
        get { return s_WhiteTexture; }
    }

    /// <summary>
    /// Whether the Image has a border to work with.
    /// </summary>
    public bool hasBorder
    {
        get { return false; }
    }

    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            float referencePixelsPerUnit = 100;
            if (canvas)
                referencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return spritePixelsPerUnit/referencePixelsPerUnit;
        }
    }

    /// <summary>
    /// Update the UI renderer mesh.
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        switch (type)
        {
            case Type.Normal:
                GenerateNormalVertices(toFill);
                break;
            case Type.Progress:
                GenerateProgressVertices(toFill);
                break;
            case Type.Frame:
                GenerateFrameVertices(toFill);
                break;
        }
        
    }

    protected virtual void GenerateProgressVertices(VertexHelper vh)
    {
        Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
        Vector4 vector4 = new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
        vh.Clear();

        float unitDelta = pixelAdjustedRect.width / MaxHp;
        float delta = unitDelta * HpDelta;

        var startX = vector4.x;
        var curHpCount = 0;

        if (m_WhiteHp > 0)
        {
            AddBarVerteices(m_WhiteHp, ref curHpCount, ref startX, unitDelta, delta, vh, vector4, new Color32(222,222,222,200));
        }
        
        if (m_BlueHp > 0)
        {
            AddBarVerteices(m_BlueHp, ref curHpCount, ref startX, unitDelta, delta, vh, vector4, new Color32(0, 0, 188, 200));
        }

        if (m_YellowHp > 0)
        {
            AddBarVerteices(m_YellowHp, ref curHpCount, ref startX, unitDelta, delta, vh, vector4, new Color32(188, 188, 0, 200));
        }
    }

    protected void AddBarVerteices(int hpCount, ref int curHpCount, ref float startX, float unitDelta, float delta, VertexHelper vh, Vector4 vector4, Color32 color)
    {
        var firstGridDelta = m_HpDelta - curHpCount % m_HpDelta;
        if (hpCount > firstGridDelta)
        {
            hpCount -= firstGridDelta;
            curHpCount += firstGridDelta;
            var tempDelta = firstGridDelta * unitDelta;
            AddQuad(vh, vector4, startX, tempDelta, color);
            startX += tempDelta;
        }

        var isNotComplete = hpCount % HpDelta > 0;
        var count = hpCount / HpDelta + (isNotComplete ? 1 : 0);
        for (int i = 0; i < count; ++i)
        {
            var tempDelta = delta;
            var needGap = true;
            if (isNotComplete && i == count - 1)
            {
                curHpCount += hpCount % HpDelta;
                tempDelta = (hpCount % HpDelta) * unitDelta;
                needGap = false;
            }
            else
            {
                curHpCount += m_HpDelta;
            }

            AddQuad(vh, vector4, startX, tempDelta, color, needGap);
            startX += tempDelta;
        }
    }

    protected virtual void GenerateFrameVertices(VertexHelper vh)
    {
        Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
        Vector4 vector4 = new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
        Color32 color = (Color32)this.color;
        vh.Clear();

        float delta = pixelAdjustedRect.width / MaxHp * HpDelta;
        var startX = vector4.x;
        var count = MaxHp / HpDelta + (MaxHp % HpDelta > 0 ? 1 : 0);
        for (int i = 0; i < count; ++i)
        {
            AddFrameQuad(vh, vector4, startX, delta, pixelAdjustedRect, color);
            startX += delta;
        }
    }

    protected void AddFrameQuad(VertexHelper vh, Vector4 vector4, float startX, float delta, Rect pixelAdjustedRect, Color32 color)
    {
        var rightX = Mathf.Min(startX + delta - Gap, vector4.z);
        vh.AddVert(new Vector3(startX, vector4.y), color, new Vector2(0.0f, 0.0f));
        vh.AddVert(new Vector3(startX + DeltaX, vector4.w), color, new Vector2(0.0f, 1f));
        vh.AddVert(new Vector3(startX + 1 + DeltaX, vector4.w), color, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(startX + 1, vector4.y), color, new Vector2(1f, 0.0f));
        vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
        vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);

        vh.AddVert(new Vector3(rightX - 1, vector4.y), color, new Vector2(0.0f, 0.0f));
        vh.AddVert(new Vector3(rightX - 1 + DeltaX, vector4.w), color, new Vector2(0.0f, 1f));
        vh.AddVert(new Vector3(rightX + DeltaX, vector4.w), color, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(rightX, vector4.y), color, new Vector2(1f, 0.0f));
        vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
        vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);


        vh.AddVert(new Vector3(startX + DeltaX + 1 - DeltaX / pixelAdjustedRect.height, vector4.w - 1), color, new Vector2(0.0f, 0.0f));
        vh.AddVert(new Vector3(startX + DeltaX + 1, vector4.w), color, new Vector2(0.0f, 1f));
        vh.AddVert(new Vector3(rightX + DeltaX - 1, vector4.w), color, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(rightX + DeltaX - 1 - DeltaX / pixelAdjustedRect.height, vector4.w - 1), color, new Vector2(1f, 0.0f));
        vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
        vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);


        vh.AddVert(new Vector3(startX + 1, vector4.y), color, new Vector2(0.0f, 0.0f));
        vh.AddVert(new Vector3(startX + 1 + DeltaX / pixelAdjustedRect.height, vector4.y + 1), color, new Vector2(0.0f, 1f));
        vh.AddVert(new Vector3(rightX - 1 + DeltaX / pixelAdjustedRect.height, vector4.y + 1), color, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(rightX - 1, vector4.y), color, new Vector2(1f, 0.0f));
        vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
        vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);
    }

    protected virtual void GenerateNormalVertices(VertexHelper vh)
    {
        Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
        Vector4 vector4 = new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
        Color32 color = (Color32)this.color;
        vh.Clear();

        float delta = pixelAdjustedRect.width / MaxHp * HpDelta;
        var startX = vector4.x;
        var count = MaxHp / HpDelta + (MaxHp % HpDelta > 0 ? 1 : 0);
        for (int i = 0; i < count; ++i)
        {
            AddQuad(vh, vector4, startX, delta, color);
            startX += delta;
        }
    }

    protected void AddQuad(VertexHelper vh, Vector4 vector4, float startX, float delta, Color32 color, bool needGap = true)
    {
        var rightX = Mathf.Min(startX + delta - (needGap ? Gap : 0), vector4.z);
        vh.AddVert(new Vector3(startX, vector4.y), color, new Vector2(0.0f, 0.0f));
        vh.AddVert(new Vector3(startX + DeltaX, vector4.w), color, new Vector2(0.0f, 1f));
        vh.AddVert(new Vector3(rightX + DeltaX, vector4.w), color, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(rightX, vector4.y), color, new Vector2(1f, 0.0f));

        vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
        vh.AddTriangle(vh.currentVertCount - 2, vh.currentVertCount - 1, vh.currentVertCount - 4);
    }

    public virtual void CalculateLayoutInputHorizontal()
    {
    }

    public virtual void CalculateLayoutInputVertical()
    {
    }

    public virtual float minWidth
    {
        get { return 0; }
    }

    public virtual float preferredWidth
    {
        get { return rectTransform.rect.width + m_DeltaX; }
    }

    public virtual float flexibleWidth
    {
        get { return -1; }
    }

    public virtual float minHeight
    {
        get { return 0; }
    }

    public virtual float preferredHeight
    {
        get { return rectTransform.rect.height; }
    }

    public virtual float flexibleHeight
    {
        get { return -1; }
    }

    public virtual int layoutPriority
    {
        get { return 0; }
    }

    public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        return true;
    }
}

