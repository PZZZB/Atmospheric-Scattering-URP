using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class LightShaftsPass : ScriptableRenderPass
    {
        public Material material;

        private RenderTargetHandle _lightShaftsLut;

        //private  RenderTargetHandle m_TemporaryColorTexture;

        // Light Shafts

        public LightShaftsPass(Material material)
        {
            //TODO 这里不懂为啥用BeforeRenderingOpaques会导致Opaques物体渲染到LightShaftsLUT上，ColorGradingLUTPass为什么不会。
            renderPassEvent = RenderPassEvent.BeforeRenderingPrepasses;// BeforeRenderingOpaques; // RenderPassEvent.AfterRenderingShadows;
            this.material = material;
            _lightShaftsLut.Init("_LightShaft");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.shadowData.supportsMainLightShadows) return;
            
            CommandBuffer cmd = CommandBufferPool.Get("LightShafts");

            //int old = RenderTargetHandle.CameraTarget.id;

            int width = renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;

            //TODO 需要Release RT 吗？
            cmd.GetTemporaryRT(_lightShaftsLut.id, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            cmd.Blit(_lightShaftsLut.id, _lightShaftsLut.id, material, 0);

            context.ExecuteCommandBuffer(cmd);

            CommandBufferPool.Release(cmd);
        }


        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_lightShaftsLut.id);
        }
    }
}