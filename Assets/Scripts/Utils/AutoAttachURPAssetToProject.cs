using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Atmosphere_Scattering.AC.Scripts
{
    [ExecuteInEditMode]
    public class AutoAttachURPAssetToProject : MonoBehaviour
    {
        public UniversalRenderPipelineAsset asset;

        void Update()
        {
            GraphicsSettings.renderPipelineAsset = asset;
        }
    }
}
