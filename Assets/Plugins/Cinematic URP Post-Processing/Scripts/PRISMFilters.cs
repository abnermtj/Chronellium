using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using System;
using System.Linq;
using PRISM.Utils;


// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Cinematic URP Post Processing/CUPP - PRISM Filters")]
public class PRISMFilters : VolumeComponent
{

    [Tooltip("Use a fullscreen blur")]
    public BoolParameter useFullscreenBlur = new BoolParameter(false);// (0, 0, 1);
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("CUPP - PrismFilters", CustomPostProcessInjectionPoint.BeforePostProcess)]
public class PRISMFilters_URP : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private PRISMFilters m_VolumeComponent;

    // The postprocessing material
    private Material m_Material;

    private RenderTargetHandle _intermediate = default;
    private RenderTargetHandle _Main = default;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int DownsampleAmount = Shader.PropertyToID("_DownsampleAmount");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    /// Specifies the input needed by this custom post process. Default is Color only.
    public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Depth;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/PRISMStack");
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<PRISMFilters>();

        _Main = new RenderTargetHandle();
        _Main.Init("_MainTex");

        _intermediate = new RenderTargetHandle();
        _intermediate.Init("_IntermediateTex");

        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.useFullscreenBlur.value == true;
    }

    void CalculateWeights()
    {
        float[] weights;

        // Only 5, 9, 13 and 17 taps are supported in the fast shader
        int sampledTaps = 17;

        float[] offsets;
        GaussianKernel.GetFastWeights(sampledTaps, GaussianKernel.IdealStandardDeviation(7),
            out weights, out offsets);

        if (sampledTaps != 5)
            m_Material.SetFloat("centerTapWeight", weights[0]);

        switch (sampledTaps)
        {
            case 5:
                m_Material.SetVector("tapWeights", new Vector4(weights[1], weights[0], weights[1], 0f));
                m_Material.SetVector("tapOffsets", new Vector4(-offsets[1], offsets[0], offsets[1], 0f));
                break;
            case 9:
                m_Material.SetVector("tapWeights", new Vector4(weights[2], weights[1], weights[1], weights[2]));
                m_Material.SetVector("tapOffsets", new Vector4(-offsets[2], -offsets[1], offsets[1], offsets[2]));
                break;
            case 13:
                m_Material.SetVector("tapWeights", new Vector4(weights[1], weights[2], weights[3], 0f));
                m_Material.SetVector("tapOffsets", new Vector4(offsets[1], offsets[2], offsets[3], 0f));
                break;
            case 17:
                m_Material.SetVector("tapWeights", new Vector4(weights[1], weights[2], weights[3], weights[4]));
                m_Material.SetVector("tapOffsets", new Vector4(offsets[1], offsets[2], offsets[3], offsets[4]));
                break;
        }
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Intensity, 1f);
        }
        // set source texture

        RenderTextureDescriptor descriptorOne = GetTempRTDescriptor(renderingData);
        cmd.GetTemporaryRT(_Main.id, descriptorOne);
        cmd.Blit(source, ShaderIDs.Input);

        // set source texture
        cmd.SetGlobalTexture(ShaderIDs.Input, source);

        CalculateWeights();

        if (m_VolumeComponent.useFullscreenBlur.value == true)
        {
            // Create a temporary target
            RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);

            float initialDofDownsample = 2f;
            descriptor.height /= (int)initialDofDownsample;
            descriptor.width /= (int)initialDofDownsample;

            cmd.GetTemporaryRT(_intermediate.id, descriptor, FilterMode.Bilinear);
            cmd.GetTemporaryRT(_intermediate.id+1, descriptor, FilterMode.Bilinear);

            m_Material.SetInt(ShaderIDs.DownsampleAmount, 0);

            cmd.Blit(source, _intermediate.Identifier(), m_Material, 2);

            cmd.SetGlobalTexture(ShaderIDs.Input, _intermediate.id);
            cmd.Blit(_intermediate.Identifier(), _intermediate.id + 1, m_Material, 6);

            cmd.SetGlobalTexture(ShaderIDs.Input, _intermediate.id+1);
            cmd.Blit(_intermediate.id + 1, _intermediate.Identifier(), m_Material, 7);

            m_Material.SetInt(ShaderIDs.DownsampleAmount, 1);

            cmd.SetGlobalTexture(ShaderIDs.Input, _intermediate.id);
            cmd.Blit(_intermediate.Identifier(), destination, m_Material, 2);

            cmd.ReleaseTemporaryRT(_intermediate.id);
            cmd.ReleaseTemporaryRT(_intermediate.id+1);
        } else
        {
            CoreUtils.DrawFullScreen(cmd, m_Material, destination);
        }

    }

}
