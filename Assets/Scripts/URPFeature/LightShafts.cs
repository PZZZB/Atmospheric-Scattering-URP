using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class LightShafts : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public Material lightShaftMaterial = null;
        }

        public Settings settings = new Settings();

        private LightShaftsPass _lightShaftsPass;

        public override void Create()
        {
            _lightShaftsPass = new LightShaftsPass(settings.lightShaftMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.lightShaftMaterial == null)
            {
                Debug.LogWarningFormat("Missing LightShafts Material. {0} pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            renderer.EnqueuePass(_lightShaftsPass);
        }
    }
}