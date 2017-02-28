using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveButton : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler
{
    private bool   m_NeedProcessUpdate;
    private float  m_DivideHalfDegress;
    private int    m_DivideNum;
    private int    m_CurSelectedGrid = -1;

    [Range(0,1)]
    public float AlphaOffset = 0.8f;

    public int DivideNum
    {
        get { return m_DivideNum;  }
        set
        {
            m_DivideNum = value;
            if (m_DivideNum != 0)
            {
                m_DivideHalfDegress = 180f / m_DivideNum;
            }
        }
    }

    public int CurSelectedGrid
    {
        get { return m_CurSelectedGrid; }
        set
        {
            if (m_CurSelectedGrid != value)
            {
                m_CurSelectedGrid = value;
                SetVerticesDirty();
            }
        }
    }

    public GameObject lineGo;

    protected override void OnEnable()
    {
        base.OnEnable();
        CurSelectedGrid = -1;
    }

    protected virtual void OnDiable()
    {
        base.OnEnable();
        CurSelectedGrid = -1;
    }

    protected override void Start()
    {
        base.Start();

        DivideNum = 8;
        var initDegress = -m_DivideHalfDegress;
        for (int i = 0; i < DivideNum; ++i)
        {
            var go = Instantiate(lineGo);
            go.transform.parent = lineGo.transform.parent;
            go.transform.localPosition = new Vector3(0, 0, 0);
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.AngleAxis(initDegress, Vector3.forward);
            initDegress += m_DivideHalfDegress * 2;
            go.SetActive(true);  
        }
    }

    void Update()
    {
        if (m_NeedProcessUpdate)
        {
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var dir = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - center;
            var curDegress = dir.y < 0 ? 360f - Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir);

            var initDegress = -m_DivideHalfDegress;
            for (int i = 0; i < DivideNum; ++i)
            {
                var startDegress = initDegress < 0 ? initDegress + 360 : initDegress;
                initDegress += m_DivideHalfDegress * 2;
                var endDegress = initDegress < 0 ? initDegress + 360 : initDegress;

                if (i == 0)
                {
                    if ((startDegress <= curDegress && curDegress <= 360) || (0 <= curDegress && curDegress <= endDegress))
                    {
                        CurSelectedGrid = i;
                        break;
                    }
                }
                else if(startDegress <= curDegress && curDegress <= endDegress)
                {
                    CurSelectedGrid = i;
                    break;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_NeedProcessUpdate = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_NeedProcessUpdate = false;
        CurSelectedGrid = -1;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        GenerateProgressVertices(toFill);
    }

    protected virtual void GenerateProgressVertices(VertexHelper vh)
    {
        Rect pixelAdjustedRect = this.GetPixelAdjustedRect();
        vh.Clear();

        if (CurSelectedGrid == -1)
            return;

        var halfWidth  = pixelAdjustedRect.width * 0.5f;
        var halfHeight = pixelAdjustedRect.height * 0.5f;
        var ratio      = halfHeight/halfWidth;
        var length     = new Vector2(halfWidth, halfHeight).magnitude;
        var unitVector = new Vector2(length, 0);
  
        var startDegress  = -m_DivideHalfDegress + 2 * m_CurSelectedGrid * m_DivideHalfDegress;
        var endDegress    = startDegress + 2 * m_DivideHalfDegress;

        var startVector  = Quaternion.Euler(0, 0, startDegress) * unitVector;
        var endVector = Quaternion.Euler(0, 0, endDegress)*unitVector;

        var startSmallerRatio = false;
        startVector = GetPoint(startVector, ratio, halfHeight, halfWidth, out startSmallerRatio);
        var endSmallerRatio = false;
        endVector = GetPoint(endVector, ratio, halfHeight, halfWidth, out endSmallerRatio);
        var middleVector = (startVector + endVector) * 0.5f;

        if (startSmallerRatio != endSmallerRatio)
        {
            middleVector = new Vector2(middleVector.x < 0 ? -halfWidth : halfWidth, middleVector.y < 0 ? -halfHeight : halfHeight);
        }

        vh.Clear();
        vh.AddVert(new Vector3(0, 0), color, new Vector2(0, 0));

        var color32 = color;
        color32.a = 1 - startVector.magnitude / length * AlphaOffset;
        vh.AddVert(new Vector3(startVector.x, startVector.y), color32, new Vector2(0, 1));

        color32 = color;
        color32.a = 1 - middleVector.magnitude/length*AlphaOffset;
        vh.AddVert(new Vector3(middleVector.x, middleVector.y), color32, new Vector2(1, 0));

        color32.a = 1 - endVector.magnitude / length * AlphaOffset;
        vh.AddVert(new Vector3(endVector.x, endVector.y), color32, new Vector2(1, 1));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    Vector2 GetPoint(Vector2 vector, float ratio, float halfHeight, float halfWidth, out bool smallerRatio)
    {
        var curRatio  = Mathf.Abs(vector.y) / Mathf.Abs(vector.x);
        var newVector = vector;
        if (curRatio > ratio)
        {
            smallerRatio = false;
            newVector = new Vector3(halfHeight / Mathf.Abs(vector.y) * Mathf.Abs(vector.x), halfHeight);
        }
        else
        {
            smallerRatio = true;
            newVector = new Vector3(halfWidth, halfWidth / Mathf.Abs(vector.x) * Mathf.Abs(vector.y));
        }

        if (vector.x < 0)
            newVector.x *= -1;

        if (vector.y < 0)
            newVector.y *= -1;

        return newVector;
    }
} 
