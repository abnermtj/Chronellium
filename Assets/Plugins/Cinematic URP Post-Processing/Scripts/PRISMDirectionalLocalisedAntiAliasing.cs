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
[System.Serializable, VolumeComponentMenu("Cinematic URP Post Processing/CUPP - PRISM DLAA")]
public class PRISMDirectionalLocalisedAntiAliasing : VolumeComponent
{
    [Tooltip("Controls the blending between the original and the grayscale color.")]
    public BoolParameter enableDLAA = new BoolParameter(false);
    public BoolParameter retainSharpness = new BoolParameter(true);
}


// Define the renderer for the custom post processing effect
[CustomPostProcess("CUPP - PrismDLAA", CustomPostProcessInjectionPoint.BeforePostProcess)]
public class PRISMDLAA_URP : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private PRISMDirectionalLocalisedAntiAliasing m_VolumeComponent;

    // The postprocessing material
    private Material m_Material;

    private RenderTargetHandle _intermediate = default;
    private RenderTargetHandle _Main = default;
    public RenderTexture intTexture;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    /// Specifies the input needed by this custom post process. Default is Color only.
    public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Color;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        m_Material = CoreUtils.CreateEngineMaterial("PRISM/PRISMDLAA");
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<PRISMDirectionalLocalisedAntiAliasing>();

        _intermediate = new RenderTargetHandle();
        _intermediate.Init("_IntermediateTex");

        _Main = new RenderTargetHandle();
        _Main.Init("_MainTex");

        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.enableDLAA.value == true;
    }


    // Dispose of the allocated resources
    public override void Dispose(bool disposing)
    {
        intTexture.Release();
        base.Dispose(disposing);
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Create a temporary target
        RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);
        cmd.GetTemporaryRT(_intermediate.id, descriptor, FilterMode.Bilinear);
        cmd.GetTemporaryRT(_Main.id, descriptor);

        cmd.Blit(source, ShaderIDs.Input);
        cmd.SetGlobalTexture(ShaderIDs.Input, source);

        cmd.Blit(ShaderIDs.Input, _intermediate.Identifier(), m_Material, 0);

        int nextpass = 1;

        if(m_VolumeComponent.retainSharpness.value == false)
        {
            nextpass = 2;
        }

        cmd.SetGlobalTexture(ShaderIDs.Input, _intermediate.id);
        cmd.Blit(_intermediate.Identifier(), destination, m_Material, nextpass);

        cmd.ReleaseTemporaryRT(_intermediate.id);
        cmd.ReleaseTemporaryRT(_Main.id);
    }

}
