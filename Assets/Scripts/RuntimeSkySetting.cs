using System;
using UnityEngine;

namespace AtmosphericScattering
{
    [RequireComponent(typeof(ScatteringSetting))]
    [ExecuteInEditMode]
    public class RuntimeSkySetting : MonoBehaviour
    {
        // Look up table update mode, it's better to use everyframe mode when you're in edit mode, need change params frequently.
        public LUTUpdateMode lutUpdateMode = LUTUpdateMode.EveryFrame;

        [Header("Environments")]
        public Light mainLight;

        [ColorUsage(false, true)]
        public Color lightFromOuterSpace = Color.white;

        public float planetRadius = 6357000.0f;
        public float atmosphereHeight = 12000f;
        public float surfaceHeight;

        [Header("Particles")]
        public float rDensityScale = 7994.0f;

        public float mDensityScale = 1200;

        [Header("Sun Disk")]
        public float sunIntensity = 0.75f;

        [Range(-1, 1)]
        public float sunMieG = 0.98f;

        [Header("Precomputation")]
        public ComputeShader computerShader;

        public Vector2Int integrateCPDensityLUTSize = new Vector2Int(512, 512);
        public Vector2Int sunOnSurfaceLUTSize = new Vector2Int(512, 512);
        public int ambientLUTSize = 512;
        public Vector2Int inScatteringLUTSize = new Vector2Int(1024, 1024);

        [Header("Debug/Output")] [NonSerialized]
        private bool m_ShowFrustumCorners = false;

        [NonSerialized] [ColorUsage(false, true)]
        private Color m_MainLightColor;

        [NonSerialized] [ColorUsage(false, true)]
        private Color m_AmbientColor;

        // x : dot(-mianLightDir,worldUp)，y：height
        [NonSerialized]
        private RenderTexture m_IntegrateCPDensityLUT;

        // x : dot(-mianLightDir,worldUp)，y：height
        [NonSerialized]
        private RenderTexture m_SunOnSurfaceLUT;

        // x : dot(-mianLightDir,worldUp)，y：height
        [NonSerialized]
        private RenderTexture m_AmbientLUT;

        [NonSerialized]
        private RenderTexture m_InScatteringLUT;

        private Texture2D m_SunOnSurfaceLUTReadToCPU;
        private Texture2D m_HemiSphereRandomNormlizedVecLUT;
        private Texture2D m_AmbientLUTReadToCPU;

        private Camera m_Camera;
        private Vector3[] m_FrustumCorners = new Vector3[4];
        private Vector4[] m_FrustumCornersVec4 = new Vector4[4];

        private void SetCommonParams()
        {
            Shader.SetGlobalTexture(Keys.kIntergalCPDensityLUT, m_IntegrateCPDensityLUT);
            //Shader.SetGlobalTexture(Keys.kSunOnSurface, m_SunOnSurfaceLUT);
            Shader.SetGlobalVector(Keys.kDensityScaleHeight, new Vector4(rDensityScale, mDensityScale));
            Shader.SetGlobalFloat(Keys.kPlanetRadius, planetRadius);
            Shader.SetGlobalFloat(Keys.kAtmosphereHeight, atmosphereHeight);
            Shader.SetGlobalFloat(Keys.kSurfaceHeight, surfaceHeight);
            Shader.SetGlobalVector(Keys.kIncomingLight, lightFromOuterSpace);
            Shader.SetGlobalFloat(Keys.kSunIntensity, sunIntensity);
            Shader.SetGlobalFloat(Keys.kSunMieG, sunMieG);
            m_Camera.CalculateFrustumCorners(m_Camera.rect, m_Camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, m_FrustumCorners);
            for (int i = 0; i < 4; i++)
            {
                m_FrustumCorners[i] = m_Camera.transform.TransformDirection(m_FrustumCorners[i]);
                m_FrustumCornersVec4[i] = m_FrustumCorners[i];
                if (m_ShowFrustumCorners)
                    Debug.DrawRay(m_Camera.transform.position, m_FrustumCorners[i], Color.blue);
            }

            Shader.SetGlobalVectorArray(Keys.kFrustumCorners, m_FrustumCornersVec4);
        }

        private void PreComputeAll()
        {
            if (computerShader == null)
            {
                Debug.LogWarningFormat("Computer shader for precompute scattering lut is empty");
                return;
            }

            SetCommonParams();
            ComputeIntegrateCPdensity();
            ComputeSunOnSurface();
            ComputeInScattering();
            ComputeHemiSphereRandomVectorLUT();
            ComputeAmbient();
        }

        private void ComputeIntegrateCPdensity()
        {
            Utils.CheckOrCreateLUT(ref m_IntegrateCPDensityLUT, integrateCPDensityLUTSize, RenderTextureFormat.RGFloat);

            int index = computerShader.FindKernel("CSIntergalCPDensity");

            // Set Params
            computerShader.SetTexture(index, Keys.kRWintergalCPDensityLUT, m_IntegrateCPDensityLUT);

            Utils.Dispatch(computerShader, index, integrateCPDensityLUTSize);
        }

        //TODO need HDR format?
        private void ComputeSunOnSurface()
        {
            Utils.CheckOrCreateLUT(ref m_SunOnSurfaceLUT, sunOnSurfaceLUTSize, RenderTextureFormat.DefaultHDR);

            int index = computerShader.FindKernel("CSsunOnSurface");

            // Set Params
            computerShader.SetTexture(index, Keys.kRWsunOnSurfaceLUT, m_SunOnSurfaceLUT);
            computerShader.SetTexture(index, Keys.kIntergalCPDensityLUT, m_IntegrateCPDensityLUT);

            Utils.Dispatch(computerShader, index, sunOnSurfaceLUTSize);
        }

        private void UpdateMainLight()
        {
            if (mainLight == null) return;

            if (m_SunOnSurfaceLUTReadToCPU == null) m_SunOnSurfaceLUTReadToCPU = new Texture2D(m_SunOnSurfaceLUT.width, m_SunOnSurfaceLUT.height, TextureFormat.RGBAHalf, false, true);
            Utils.ReadRTpixelsBackToCPU(m_SunOnSurfaceLUT, m_SunOnSurfaceLUTReadToCPU);

            var lightDir = -mainLight.transform.forward;
            var cosAngle01 = Vector3.Dot(Vector3.up, lightDir) * 0.5 + 0.5;
            var height01 = surfaceHeight / atmosphereHeight;

            var col = m_SunOnSurfaceLUTReadToCPU.GetPixel((int) (cosAngle01 * m_SunOnSurfaceLUTReadToCPU.width), (int) (height01 * m_SunOnSurfaceLUTReadToCPU.height));
            Color lightColor;
            float intensity;
            Utils.HDRToColorIntendity(col, out lightColor, out intensity);

            mainLight.color = lightColor.gamma;
            mainLight.intensity = intensity;
            m_MainLightColor = col;
        }

        private void ComputeInScattering()
        {
            // Need HDR?
            Utils.CheckOrCreateLUT(ref m_InScatteringLUT, inScatteringLUTSize, RenderTextureFormat.DefaultHDR);

            int index = computerShader.FindKernel("CSInScattering");

            //Set Params
            computerShader.SetTexture(index, Keys.kRWinScatteringLUT, m_InScatteringLUT);
            computerShader.SetTexture(index, Keys.kIntergalCPDensityLUT, m_IntegrateCPDensityLUT);

            Utils.Dispatch(computerShader, index, inScatteringLUTSize);
        }

        private void ComputeHemiSphereRandomVectorLUT()
        {
            if (m_HemiSphereRandomNormlizedVecLUT == null)
            {
                m_HemiSphereRandomNormlizedVecLUT = new Texture2D(512, 1, TextureFormat.RGB24, false, true);
                m_HemiSphereRandomNormlizedVecLUT.filterMode = FilterMode.Point;
                ;
                m_HemiSphereRandomNormlizedVecLUT.Apply();
                for (int i = 0; i < m_HemiSphereRandomNormlizedVecLUT.width; ++i)
                {
                    var randomVec = UnityEngine.Random.onUnitSphere;
                    m_HemiSphereRandomNormlizedVecLUT.SetPixel(i, 0, new Color(randomVec.x, Mathf.Abs(randomVec.y), randomVec.z));
                }
            }
        }

        private void ComputeAmbient()
        {
            var size = new Vector2Int(ambientLUTSize, 1);
            Utils.CheckOrCreateLUT(ref m_AmbientLUT, size, RenderTextureFormat.DefaultHDR);

            int index = computerShader.FindKernel("CSAmbient");

            //Set Params
            computerShader.SetTexture(index, Keys.kRWhemiSphereRandomNormlizedVecLUT, m_HemiSphereRandomNormlizedVecLUT);
            computerShader.SetTexture(index, Keys.kInScatteringLUT, m_InScatteringLUT);
            computerShader.SetTexture(index, Keys.kRWambientLUT, m_AmbientLUT);

            Utils.Dispatch(computerShader, index, size);
        }

        private void UpdateAmbient()
        {
            if (m_AmbientLUTReadToCPU == null) m_AmbientLUTReadToCPU = new Texture2D(ambientLUTSize, 1, TextureFormat.RGB24, false, true);

            Utils.ReadRTpixelsBackToCPU(m_AmbientLUT, m_AmbientLUTReadToCPU);

            var lightDir = -mainLight.transform.forward;
            var cosAngle01 = Vector3.Dot(Vector3.up, lightDir) * 0.5 + 0.5;

            var ambient = m_AmbientLUTReadToCPU.GetPixel((int) (cosAngle01 * m_AmbientLUTReadToCPU.width), 0);

            RenderSettings.ambientLight = ambient.gamma;
            m_AmbientColor = ambient;
        }

        private void Awake()
        {
            m_Camera = Camera.main;
        }

        private void Start()
        {
            if (lutUpdateMode == LUTUpdateMode.PrecomputeOnStart)
            {
                SetCommonParams();

                PreComputeAll();
                UpdateMainLight();
                UpdateAmbient();
            }
        }

        private void OnDisable()
        {
            if (m_IntegrateCPDensityLUT != null) m_IntegrateCPDensityLUT.Release();
        }

        private void Update()
        {
            if (lutUpdateMode == LUTUpdateMode.EveryFrame)
            {
                SetCommonParams();

                PreComputeAll();
                UpdateMainLight();
                UpdateAmbient();
            }
        }
    }
}