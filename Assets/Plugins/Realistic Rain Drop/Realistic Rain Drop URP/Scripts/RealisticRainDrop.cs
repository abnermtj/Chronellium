using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RealisticRainDrop : ScriptableRendererFeature
{
    [System.Serializable]
    public class RealisticRainDropSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        public Material blurMaterial = null;
        public Material rainMaterial = null;

        public bool blur = false;
        [Range(0, 6)]
        public int blurDownSampleNum = 2;
        [Range(0.0f, 20.0f)]
        public float blurSpreadSize = 3.0f;
        [Range(0, 8)]
        public int blurIterations = 3;

        [Range(1, 4)]
        public int rtQuality = 3;
        [Range(0, 1)]
        public float fade = 1;
        [Range(0.2f, 4)]
        public float intensity = 1.75f;
    }

    public RealisticRainDropSettings settings = new RealisticRainDropSettings();

    class CustomRenderPass : ScriptableRenderPass
    {
        public RealisticRainDropSettings settings = null;

        string profilerTag;

        private int sourceId_copy;
        private RenderTargetIdentifier sourceRT_copy;
        private int sourceRT_width = 0;
        private int sourceRT_height = 0;
        private RenderTextureFormat sourceRT_format;

        private int blurBufferId;
        private int blurTempBufferId;

        private int rainDropBufferId;

        private int srcTexPropId = 0;
        private int paramsPropId = 0;

        private RenderTargetIdentifier source { get; set; }

        public void Setup(RenderTargetIdentifier source) {
            this.source = source;
        }

        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var width = cameraTextureDescriptor.width;
            var height = cameraTextureDescriptor.height;
            sourceRT_width = width;
            sourceRT_height = height;
            sourceRT_format = cameraTextureDescriptor.colorFormat;

            sourceId_copy = Shader.PropertyToID("_SourceRT_Copy");
            cmd.GetTemporaryRT(sourceId_copy, width, height, 0, FilterMode.Bilinear, cameraTextureDescriptor.colorFormat);
            sourceRT_copy = new RenderTargetIdentifier(sourceId_copy);
            ConfigureTarget(sourceRT_copy);

            blurBufferId = Shader.PropertyToID("_BlurBuffer");
            blurTempBufferId = Shader.PropertyToID("_BlurTempBuffer");
            rainDropBufferId = Shader.PropertyToID("_RainDropBuffer");

            srcTexPropId = Shader.PropertyToID("_SrcTex");
            paramsPropId = Shader.PropertyToID("_Params");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            //
            cmd.Blit(source, sourceRT_copy);

            // blur
            if(settings.blur)
            {
                float widthMod = 1.0f / (1.0f * (1 << settings.blurDownSampleNum));
                cmd.SetGlobalFloat("_DownSampleValue", settings.blurSpreadSize * widthMod);
                int renderWidth = sourceRT_width >> settings.blurDownSampleNum;
                int renderHeight = sourceRT_height >> settings.blurDownSampleNum;

                cmd.GetTemporaryRT(blurBufferId, renderWidth, renderHeight, 0, FilterMode.Bilinear, sourceRT_format);
                cmd.Blit(sourceRT_copy, blurBufferId, settings.blurMaterial, 0);

                for (int i = 0; i < settings.blurIterations; i++)
                {
                    float iterationOffs = (i * 1.0f);
                    cmd.SetGlobalFloat("_DownSampleValue", settings.blurSpreadSize * widthMod + iterationOffs);

                    cmd.GetTemporaryRT(blurTempBufferId, renderWidth, renderHeight, 0, FilterMode.Bilinear, sourceRT_format);
                    cmd.Blit(blurBufferId, blurTempBufferId, settings.blurMaterial, 1);
                    cmd.ReleaseTemporaryRT(blurBufferId);
                    blurBufferId = blurTempBufferId;

                    cmd.GetTemporaryRT(blurTempBufferId, renderWidth, renderHeight, 0, FilterMode.Bilinear, sourceRT_format);
                    cmd.Blit(blurBufferId, blurTempBufferId, settings.blurMaterial, 2);
                    cmd.ReleaseTemporaryRT(blurBufferId);
                    blurBufferId = blurTempBufferId;

                }


            }

            //
            {
                cmd.SetGlobalTexture(srcTexPropId, sourceRT_copy);
                cmd.SetGlobalVector(paramsPropId, new Vector4(settings.fade, settings.intensity, 0, 0));
                int rtSize = 4 - settings.rtQuality + 1;

                cmd.GetTemporaryRT(rainDropBufferId, sourceRT_width / rtSize, sourceRT_height / rtSize, 0, FilterMode.Bilinear, sourceRT_format);
                cmd.Blit(sourceRT_copy, rainDropBufferId, settings.rainMaterial, 0);
                if (settings.blur)
                {
                    cmd.SetGlobalTexture(srcTexPropId, blurBufferId);
                }
                cmd.Blit(rainDropBufferId, source, settings.rainMaterial, 1);
            }

            // release buffers
            {
                if (settings.blur)
                {
                    cmd.ReleaseTemporaryRT(blurBufferId);
                }
                cmd.ReleaseTemporaryRT(rainDropBufferId);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new CustomRenderPass("RealisticRainDrop");
        scriptablePass.settings = settings;

        scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        scriptablePass.Setup(src);
        renderer.EnqueuePass(scriptablePass);
    }
}


