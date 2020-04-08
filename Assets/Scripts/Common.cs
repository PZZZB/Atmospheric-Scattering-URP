using UnityEngine;

namespace AtmosphericScattering
{
    public enum LUTUpdateMode
    {
        PrecomputeOnStart,
        EveryFrame,
    }

    public enum APDebugMode
    {
        None,
        Extinction,
        Inscattering
    }

    public static class Keys
    {
        public static readonly int kRWintergalCPDensityLUT = Shader.PropertyToID("_RWintegralCPDensityLUT");
        public static readonly int kIntergalCPDensityLUT = Shader.PropertyToID("_IntegralCPDensityLUT");
        public static readonly int kRWhemiSphereRandomNormlizedVecLUT = Shader.PropertyToID("_RWhemiSphereRandomNormlizedVecLUT");
        public static readonly int kRWambientLUT = Shader.PropertyToID("_RWambientLUT");
        public static readonly int kRWinScatteringLUT = Shader.PropertyToID("_RWinScatteringLUT");
        public static readonly int kInScatteringLUT = Shader.PropertyToID("_InScatteringLUT");
        public static readonly int kRWsunOnSurfaceLUT = Shader.PropertyToID("_RWsunOnSurfaceLUT");
        
        public static readonly int kDensityScaleHeight = Shader.PropertyToID("_DensityScaleHeight");
        public static readonly int kPlanetRadius = Shader.PropertyToID("_PlanetRadius");
        public static readonly int kAtmosphereHeight = Shader.PropertyToID("_AtmosphereHeight");
        public static readonly int kSurfaceHeight = Shader.PropertyToID("_SurfaceHeight");
        public static readonly int kDistanceScale = Shader.PropertyToID("_DistanceScale");
        //public static readonly int kSunOnSurface = Shader.PropertyToID("_SunOnSurface");
        public static readonly int kScatteringR = Shader.PropertyToID("_ScatteringR");
        public static readonly int kScatteringM = Shader.PropertyToID("_ScatteringM");
        public static readonly int kExtinctionR = Shader.PropertyToID("_ExtinctionR");
        public static readonly int kExtinctionM = Shader.PropertyToID("_ExtinctionM");
        public static readonly int kIncomingLight = Shader.PropertyToID("_LightFromOuterSpace");
        public static readonly int kSunIntensity = Shader.PropertyToID("_SunIntensity");
        public static readonly int kSunMieG = Shader.PropertyToID("_SunMieG");
        public static readonly int kMieG = Shader.PropertyToID("_MieG");
        public static readonly int kFrustumCorners = Shader.PropertyToID("_FrustumCorners");
        
        public const string kDebugExtinction = "_AP_DEBUG_EXTINCTION";
        public const string kDebugInscattering = "_AP_DEBUG_INSCATTERING";
        public const string kLightShaft = "_LIGHT_SHAFT";
    }
    
    public static class Utils
    {

        // Helper Function
        public static void CheckOrCreateLUT(ref RenderTexture targetLUT, Vector2Int size, RenderTextureFormat format)
        {
            if (targetLUT == null || (targetLUT.width != size.x && targetLUT.height != size.y)  )
            {
                if(targetLUT != null) targetLUT.Release();

                var rt = new RenderTexture(size.x, size.y, 0,
                    format, RenderTextureReadWrite.Linear);
                rt.useMipMap = false;
                rt.filterMode = FilterMode.Bilinear;
                rt.enableRandomWrite = true;
                rt.Create();
                targetLUT = rt;
            }
        }

        public static void ReadRTpixelsBackToCPU(RenderTexture src, Texture2D dst)
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = src;
            dst.ReadPixels(new Rect(0, 0, dst.width, dst.height), 0, 0);
            RenderTexture.active = currentActiveRT;
        }

        public static void Dispatch(ComputeShader cs, int kernel, Vector2Int lutSize)
        {
            if (cs == null)
            {
                Debug.LogWarningFormat("Computer shader for precompute scattering lut is empty");
                return;
            }
            
            uint threadNumX, threadNumY, threadNumZ;
            cs.GetKernelThreadGroupSizes(kernel, out threadNumX, out threadNumY, out threadNumZ);
            cs.Dispatch(kernel, lutSize.x / (int) threadNumX,
                lutSize.y / (int) threadNumY, 1);
        }

        public static void HDRToColorIntendity(Color hdr,out Color color,out float intensity)
        {
            intensity = Mathf.Ceil(Mathf.Max(hdr.r, Mathf.Max(hdr.g, hdr.b)));
            color = hdr / intensity;
        }
    }
}