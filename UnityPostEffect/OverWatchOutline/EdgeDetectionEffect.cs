using UnityEngine;

/// <summary>
/// 后期特效---全屏边缘检测实现
/// 1、使用一个单独相机渲染出一个需要画边对象的深度图
/// 2、比较场景深度图和对象深度图，剔除对象深度被场景深度遮挡的深度到单独的一张纹理中
/// 3、绘制场景原始图并混合绘制边缘，使用Sobel算子来画边，边缘的检测深度从挑选后的深度图中获取，左右，上下像素深度从原始深度图中读取
/// </summary>
public class EdgeDetectionEffect : MonoBehaviour
{
    private Camera m_Camera;
    private Camera m_SceneDepthCamera;
    private Shader m_RenderDepthShader;

    private Material m_OcclusionMaterial;
    private Material m_EdgeDectectionMaterial;

    [SerializeField] protected Color m_OutlineColor = Color.red;
    [SerializeField] protected float m_EdgeExp = 1.0f;
    [SerializeField] protected float m_SampleDist = 1.0f;
    [SerializeField] protected LayerMask m_RenderLayer;

    public Color OutlineColor
    {
        get { return m_OutlineColor;}
        set { m_OutlineColor = value; }
    }

    public float EdgeExp
    {
        get { return m_EdgeExp; }
        set { m_EdgeExp = value; }
    }

    public float SampleDist
    {
        get { return m_SampleDist; }
        set { m_SampleDist = value; }
    }

    public LayerMask RenderLayer
    {
        get { return m_RenderLayer; }
        set
        {
            m_RenderLayer = value;
            if (m_Camera != null)
            {
                m_Camera.cullingMask = m_RenderLayer;
            }
        }
    }

    void Awake ()
    {
        m_RenderDepthShader = Shader.Find("RenderDepth");

        GameObject cameraGo = new GameObject("PostEffectCamera");
	    cameraGo.transform.parent = Camera.main.transform;
	    cameraGo.transform.localPosition = Vector3.zero;
	    cameraGo.transform.localScale = Vector3.one;
        cameraGo.transform.localRotation = Quaternion.identity;


        //需要设置主相机生成深度图，否则在质量切换为Fast,Fastest的时候，主相机就不会生成深度图，导致无法绘制边缘
        //注释，不需要相机自动生成深度图，使用手动的方式来生成深度图，因为部分shader会导致渲染的物体不会把深度写入到深度图中
        //Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        m_Camera = cameraGo.AddComponent<Camera>();
        m_Camera.clearFlags = CameraClearFlags.Depth;
        m_Camera.orthographic = false;
        m_Camera.nearClipPlane = Camera.main.nearClipPlane;
        m_Camera.farClipPlane = Camera.main.farClipPlane;
        m_Camera.rect = Camera.main.rect;
        m_Camera.depthTextureMode = DepthTextureMode.None;
	    m_Camera.cullingMask = m_RenderLayer;
	    m_Camera.enabled = false;

        GameObject sceneDepthCameraGo = new GameObject("SceneDepthCamera");
        sceneDepthCameraGo.transform.parent = Camera.main.transform;
        sceneDepthCameraGo.transform.localPosition = Vector3.zero;
        sceneDepthCameraGo.transform.localScale = Vector3.one;
        sceneDepthCameraGo.transform.localRotation = Quaternion.identity;
        m_SceneDepthCamera = sceneDepthCameraGo.AddComponent<Camera>();
        m_SceneDepthCamera.clearFlags = CameraClearFlags.Depth;
        m_SceneDepthCamera.orthographic = false;
        m_SceneDepthCamera.nearClipPlane = Camera.main.nearClipPlane;
        m_SceneDepthCamera.farClipPlane = Camera.main.farClipPlane;
        m_SceneDepthCamera.rect = Camera.main.rect;
        m_SceneDepthCamera.depthTextureMode = DepthTextureMode.None;
        m_SceneDepthCamera.cullingMask = Camera.main.cullingMask;
        m_SceneDepthCamera.enabled = false;

        m_OcclusionMaterial = new Material(Shader.Find("DepthCull"));
        m_OcclusionMaterial.hideFlags = HideFlags.DontSave;

        m_EdgeDectectionMaterial = new Material(Shader.Find("SobelEdgeDectection"));
        m_EdgeDectectionMaterial.hideFlags = HideFlags.DontSave;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var sceneDepthRenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        var depthRenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        var occlusionRenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
       
        m_Camera.targetTexture = depthRenderTexture;
        m_Camera.fieldOfView = Camera.main.fieldOfView;
        m_Camera.aspect = Camera.main.aspect;
        m_Camera.RenderWithShader(m_RenderDepthShader, string.Empty);

        m_SceneDepthCamera.targetTexture = sceneDepthRenderTexture;
        m_SceneDepthCamera.fieldOfView = Camera.main.fieldOfView;
        m_SceneDepthCamera.aspect = Camera.main.aspect;
        m_SceneDepthCamera.RenderWithShader(m_RenderDepthShader, string.Empty);

        m_OcclusionMaterial.SetTexture("_SceneDepthTexture", sceneDepthRenderTexture);
        Graphics.Blit(depthRenderTexture, occlusionRenderTexture, m_OcclusionMaterial);

        m_EdgeDectectionMaterial.SetFloat("_SampleDistance", m_SampleDist);
        m_EdgeDectectionMaterial.SetColor("_OutlineColor", m_OutlineColor);
        m_EdgeDectectionMaterial.SetFloat("_Exponent", EdgeExp);
        m_EdgeDectectionMaterial.SetTexture("_DepthTexture", depthRenderTexture);
        m_EdgeDectectionMaterial.SetTexture("_PartDepthTexture", occlusionRenderTexture);
        Graphics.Blit(source, destination, m_EdgeDectectionMaterial);

        RenderTexture.ReleaseTemporary(sceneDepthRenderTexture);
        RenderTexture.ReleaseTemporary(depthRenderTexture);
        RenderTexture.ReleaseTemporary(occlusionRenderTexture);
    }
}
