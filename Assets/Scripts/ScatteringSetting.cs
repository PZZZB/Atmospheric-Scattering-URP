using UnityEngine;

namespace AtmosphericScattering
{    
    [ExecuteInEditMode]
    public class ScatteringSetting : MonoBehaviour
    {
        public float distanceScale = 1.0f;
        
        public Vector3 rCoef = new Vector3(5.8f, 13.5f, 33.1f);
        public float rScatterStrength = 1f;
        public float rExtinctionStrength = 1f;

        public Vector3 mCoef = new Vector3(2.0f, 2.0f, 2.0f);
        public float mScatterStrength = 1f;
        public float mExtinctionStrength = 1f;
        public float mieG = 0.625f;
        
        [Header("Debug")]
        public APDebugMode apDebugMode;

        public bool lightShaft = true;
        
        public void UpdateParams()
        {
            Shader.DisableKeyword(Keys.kDebugExtinction);
            Shader.DisableKeyword(Keys.kDebugInscattering);
            switch (apDebugMode)
            {
                case APDebugMode.Extinction:
                    Shader.EnableKeyword(Keys.kDebugExtinction);
                    break;
                case APDebugMode.Inscattering:
                    Shader.EnableKeyword(Keys.kDebugInscattering);
                    break;
            }

            Shader.SetGlobalFloat(Keys.kDistanceScale, distanceScale);
            //地球的数据：
            //private readonly Vector4 _rayleighSct = new Vector4(5.8f, 13.5f, 33.1f, 0.0f) * 0.000001f; 
            //private readonly Vector4 _mieSct = new Vector4(2.0f, 2.0f, 2.0f, 0.0f) * 0.00001f; 
            var rCoef = this.rCoef * 0.000001f;
            var mCoef = this.mCoef * 0.00001f;
            Shader.SetGlobalVector(Keys.kScatteringR, rCoef * rScatterStrength);
            Shader.SetGlobalVector(Keys.kScatteringM, mCoef * mScatterStrength);
            Shader.SetGlobalVector(Keys.kExtinctionR, rCoef * rExtinctionStrength);
            Shader.SetGlobalVector(Keys.kExtinctionM, mCoef * mExtinctionStrength);
            Shader.SetGlobalFloat(Keys.kMieG, mieG);

            if (lightShaft) Shader.EnableKeyword(Keys.kLightShaft);
            else Shader.DisableKeyword(Keys.kLightShaft);
        }
        
        private void Update()
        {
            UpdateParams();
        }
    }
}