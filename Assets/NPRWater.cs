using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class NPRWater : MonoBehaviour
{
    public enum Quality
    {
        Low,        // 512分辨率 折射\没有反射
        High,       // 1024分辨率 折射\反射
    }

    public class RenderCamera
    {
        public Camera ReflectionCamera;
        public Camera RefractionCamera;
    }

    [SerializeField]
    protected Quality m_quality = Quality.High;

    [SerializeField]
    protected LayerMask m_refractionMask = -1;

    [SerializeField]
    protected LayerMask m_highReflectionMask = -1;

    [SerializeField]
    protected LayerMask m_mediumReflectionMask = -1;

    [SerializeField]
    protected Shader m_refractShader;

    [SerializeField]
    protected Color m_refractBackgroundColor = new Color(0.122f, 0.231f, 0.353f, 1);

    [SerializeField]
    protected Color m_reflectBackgroundColor = new Color(0.337f, 0.589f, 0.714f, 1);

    [SerializeField]
    protected float m_clipPlaneOffset = 0;

    [SerializeField]
    protected float m_seaLevel = -6;

    Dictionary<int, RenderCamera> m_cameraDic = new Dictionary<int, RenderCamera>();
    RenderTexture m_reflectionRT = null;
    RenderTexture m_refractionRT = null;
    int m_reflectionRTSize = 0;
    int m_refractionRTSize = 0;
    [SerializeField]
    Renderer[] m_renderer;
    [SerializeField]
    LayerMask ReflectionMask;

    // Whether rendering is already in progress (stops recursive rendering)
    static bool m_isRendering = false;

    public int RefractionTextureSize
    {
        get
        {
            switch (m_quality)
            {
                case Quality.Low:
                    return 512;
                case Quality.High:
                    return 1024;
            }
            return 0;
        }
    }

    public int ReflectionTextureSize
    {
        get
        {
            switch (m_quality)
            {
                case Quality.High: return 1024;
                case Quality.Low: return 512;
            }
            return 0;
        }
    }

    /// <summary>
    /// Extended sign: returns -1, 0 or 1
    /// </summary>

    static float SignExt(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    /// <summary>
    /// Adjusts the given m_projectionMat matrix so that near plane is the given clipPlane
    /// clipPlane is given in camera space. See article in Game Programming Gems 5 and
    /// http://aras-p.info/texts/obliqueortho.html
    /// </summary>

    static void CalculateObliqueMatrix(ref Matrix4x4 m_projectionMat, Vector4 clipPlane)
    {
        Vector4 q = m_projectionMat.inverse * new Vector4(SignExt(clipPlane.x), SignExt(clipPlane.y), 1.0f, 1.0f);
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));

        // third row = clip plane - fourth row
        m_projectionMat[2] = c.x - m_projectionMat[3];
        m_projectionMat[6] = c.y - m_projectionMat[7];
        m_projectionMat[10] = c.z - m_projectionMat[11];
        m_projectionMat[14] = c.w - m_projectionMat[15];
    }

    /// <summary>
    /// Calculates m_reflectionMat matrix around the given plane.
    /// </summary>

    static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    void Awake()
    {
        m_renderer = GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Cleanup all the objects we possibly have created.
    /// </summary>
    void OnDisable()
    {
        ClearReflectionRT();
        ClearRefractionRT();

        foreach (KeyValuePair<int, RenderCamera> item in m_cameraDic)
        {
            if (null != item.Value.ReflectionCamera)
            {
                DestroyImmediate(item.Value.ReflectionCamera.gameObject);
            }
            if (null != item.Value.RefractionCamera)
            {
                DestroyImmediate(item.Value.RefractionCamera.gameObject);
            }
        }
        m_cameraDic.Clear();
        m_hasCalculateReflection = false;
        m_hasCalculateRefraction = false;
    }

    /// <summary>
    /// Release the texture and the temporary cameras
    /// </summary>
    void ClearReflectionRT()
    {
        if (m_reflectionRT)
        {
            DestroyImmediate(m_reflectionRT);
            m_reflectionRT = null;
        }
    }

    void ClearRefractionRT()
    {
        if (m_refractionRT)
        {
            DestroyImmediate(m_refractionRT);
            m_refractionRT = null;
        }
    }

    /// <summary>
    /// Copy camera settings from source to destination.
    /// </summary>
    bool m_hasInitCamera = false;
    void CopyCamera(Camera src, Camera dest)
    {
        if (!m_hasInitCamera)
        {
            m_hasInitCamera = true;
        }
        else
        {
            return;
        }
        dest.CopyFrom(src);
        dest.clearFlags = CameraClearFlags.Skybox;
        dest.backgroundColor = new Color(0, 0, 0, 0);
        dest.enabled = false;
    }

    Camera GetRefractionCamera(Camera current, int textureSize)
    {
        if (!m_refractionRT || m_refractionRTSize != textureSize)
        {
            if (m_refractionRT) DestroyImmediate(m_refractionRT);
            m_refractionRT = new RenderTexture(textureSize, textureSize, 16);
            m_refractionRT.name = "__Refraction" + GetInstanceID();
            m_refractionRT.isPowerOfTwo = true;
            m_refractionRT.hideFlags = HideFlags.DontSave;
            m_refractionRTSize = textureSize;
        }

        Camera cam = null;
        RenderCamera rc;
        if (m_cameraDic.TryGetValue(current.GetInstanceID(), out rc))
        {
            cam = rc.RefractionCamera;
        }

        if (null == cam)
        {
            GameObject go = new GameObject();
            go.name = "RefractionCamera" + GetInstanceID() + "for" + current.GetInstanceID();
            go.hideFlags = HideFlags.DontSave;

            cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = m_refractBackgroundColor;
            cam.enabled = false;

            if (rc == null)
            {
                rc = new RenderCamera();
                rc.RefractionCamera = cam;
                m_cameraDic.Add(current.GetInstanceID(), rc);
            }
            else
            {
                rc.RefractionCamera = cam;
            }
        }

        Shader.SetGlobalTexture("_RefractionTex", m_refractionRT);

        return cam;
    }

    /// <summary>
    /// Get or create the camera used for m_reflectionMat.
    /// </summary>
    Camera GetReflectionCamera(Camera current, int textureSize)
    {
        if (!m_reflectionRT || m_reflectionRTSize != textureSize)
        {
            if (m_reflectionRT) DestroyImmediate(m_reflectionRT);
            m_reflectionRT = new RenderTexture(textureSize, textureSize, 16);
            m_reflectionRT.name = "__MirrorReflection" + GetInstanceID()+"for"+current.GetInstanceID();
            m_reflectionRT.isPowerOfTwo = true;
            m_reflectionRT.hideFlags = HideFlags.DontSave;
            m_reflectionRTSize = textureSize;
        }

        Camera cam = null;
        RenderCamera rc;
        if (m_cameraDic.TryGetValue(current.GetInstanceID(), out rc))
        {
            cam = rc.ReflectionCamera;
        }

        if (null == cam)
        {
           
            GameObject go = new GameObject();
            go.name = "ReflectionCamera" + GetInstanceID() + "for" + current.GetInstanceID();
            go.hideFlags = HideFlags.DontSave;

            cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;//CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.enabled = false;

            if (rc == null)
            {
                rc = new RenderCamera();
                rc.ReflectionCamera = cam;
                m_cameraDic.Add(current.GetInstanceID(), rc);
            }
            else
            {
                rc.ReflectionCamera = cam;
            }
        }

        Shader.SetGlobalTexture("_ReflectionTex", m_reflectionRT);

        return cam;
    }

    /// <summary>
    /// Given position/m_normal of the plane, calculates plane in camera space.
    /// </summary>
    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 offsetPos = pos + normal * m_clipPlaneOffset;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    /// <summary>
    /// Called when the object is being renderered.
    /// </summary>
    bool m_hasCalculateReflection = false;
    bool m_hasCalculateRefraction = false;

    Vector3 m_pos;
    Vector3 m_normal;
    float d;
    Vector4 m_reflectionPlane;
    Matrix4x4 m_reflectionMat = Matrix4x4.zero;
    Matrix4x4 m_projectionMat;

    void OnWillRenderObject()
    {
        // Safeguard from recursive reflections
        if (m_isRendering) return;
        
        if (!enabled || m_renderer.Length <= 0)
        {
            ClearReflectionRT();
            ClearRefractionRT();
            return;
        }

        Camera cam = Camera.current;
        if (null == cam) return;

        m_isRendering = true;

        //TODO: 渲染RefractionTexture
        LayerMask mask = m_refractionMask;
        int textureSize = RefractionTextureSize;
        Camera refractionCamera = GetRefractionCamera(cam, textureSize);
        if (!m_hasCalculateRefraction)
        {
            m_hasCalculateRefraction = true;
            CopyCamera(cam, refractionCamera);
        }
        refractionCamera.aspect = cam.aspect;
        refractionCamera.transform.position = cam.transform.position;
        refractionCamera.transform.rotation = cam.transform.rotation;
        refractionCamera.cullingMask = mask.value;//~(1 << 4) & mask.value
        refractionCamera.targetTexture = m_refractionRT;
        refractionCamera.backgroundColor = m_refractBackgroundColor;
        //
        Shader.SetGlobalFloat("_SeaLevel", m_seaLevel);
        //refractionCamera.RenderWithShader(m_refractShader, "");
        refractionCamera.Render();

        //渲染ReflectionTexture
        mask = ReflectionMask;
        textureSize = ReflectionTextureSize;

        Camera reflectionCamera = GetReflectionCamera(cam, textureSize);

        if (!m_hasCalculateReflection)
        {
 
            m_hasCalculateReflection = true;
            m_pos = new Vector3(0, m_seaLevel, 0);
            m_normal = Vector3.up;

            CopyCamera(cam, reflectionCamera);

            // Reflect camera around the m_reflectionMat plane
            d = -Vector3.Dot(m_normal, m_pos);
            m_reflectionPlane = new Vector4(m_normal.x, m_normal.y, m_normal.z, d);

            CalculateReflectionMatrix(ref m_reflectionMat, m_reflectionPlane);
            m_projectionMat = cam.projectionMatrix;
        }

        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = m_reflectionMat.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * m_reflectionMat;

        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, m_pos, m_normal, 1.0f);

        CalculateObliqueMatrix(ref m_projectionMat, clipPlane);

        reflectionCamera.projectionMatrix = m_projectionMat;
        reflectionCamera.cullingMask = mask.value;//~(1 << 4) & mask.value;
        reflectionCamera.targetTexture = m_reflectionRT;
        reflectionCamera.backgroundColor = m_reflectBackgroundColor;


        GL.invertCulling = true;
        {
            reflectionCamera.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
            reflectionCamera.Render();
            reflectionCamera.transform.position = oldpos;
        }
        GL.invertCulling  = false;
        
        m_isRendering = false;
    }
}