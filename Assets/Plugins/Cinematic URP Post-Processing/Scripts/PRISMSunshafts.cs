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
[System.Serializable, VolumeComponentMenu("Cinematic URP Post Processing/CUPP - PRISM Sunshafts")]
public class PRISMSunshafts : VolumeComponent
{
    [Tooltip("Intensity of the effect")]
    public ClampedFloatParameter rayIntensity = new ClampedFloatParameter(0f, 0f, 1f);

    public ClampedFloatParameter rayDecay = new ClampedFloatParameter(0.5f, 0, 1);

    public ClampedFloatParameter rayDensity = new ClampedFloatParameter(0.25f, 0f, .5f);

    public Vector3Parameter sunTransformPosition = new Vector3Parameter(new Vector3(0f, 0f, 0f));

    public ColorParameter sunColor = new ColorParameter(Color.white);

    public BoolParameter useUltraQuality = new BoolParameter(false);
    public BoolParameter useMobileDownsampling = new BoolParameter(false);


    public ObjectParameter<Transform> sunTransform = new ObjectParameter<Transform>(null);
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("CUPP - PRISMSunshafts", CustomPostProcessInjectionPoint.AfterOpaqueAndSky)]
public class PRISMSunshafts_URP : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private PRISMSunshafts m_VolumeComponent;

    // The postprocessing material
    private Material m_Material;

    public RenderTargetHandle _intermediate = default;
    private RenderTargetHandle _Main = default;

    public static Transform raysTransform;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Intensity = Shader.PropertyToID("_Intensity");

        internal readonly static int SunWeight = Shader.PropertyToID("_SunWeight");

        internal readonly static int SunDecay = Shader.PropertyToID("_SunDecay");
        internal readonly static int SunDensity = Shader.PropertyToID("_SunDensity");

        internal readonly static int SunColor = Shader.PropertyToID("_SunColor");
        internal readonly static int SunPosition = Shader.PropertyToID("_SunPosition");

        internal readonly static int SunTex = Shader.PropertyToID("_SunTex");
        internal readonly static int SunTexIntermed = Shader.PropertyToID("_SunTexIntermed");

        
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    /// Specifies the input needed by this custom post process. Default is Color only.
    public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Depth;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        m_Material = CoreUtils.CreateEngineMaterial("PRISM/PRISMSunshafts");
    }

    public static void SetSunTransform(Transform sun)
    {
        raysTransform = sun;
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<PRISMSunshafts>();

        _Main = new RenderTargetHandle();
        _Main.Init("_MainTex");

        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.rayIntensity.value > 0;
    }


    protected void SetGodraysShaderValues(Material raysMaterial, Camera m_Camera, Vector3 sunPos)
    {
        raysMaterial.SetFloat(ShaderIDs.SunWeight, m_VolumeComponent.rayIntensity.value);

        raysMaterial.SetFloat(ShaderIDs.SunDecay, m_VolumeComponent.rayDecay.value);

        m_Camera.depthTextureMode// = DepthTextureMode.MotionVectors;
        |= DepthTextureMode.MotionVectors;

        Vector3 v = m_Camera.WorldToViewportPoint(sunPos);
        raysMaterial.SetVector(ShaderIDs.SunPosition, new Vector4(v.x, v.y, v.z, 1f));
        raysMaterial.SetColor(ShaderIDs.SunColor, m_VolumeComponent.sunColor.value);

        if (v.z >= 0.0f)
        {
            //raysMaterial.SetColor(ShaderIDs.SunColor, Color.white);
        }
        else
        {
            raysMaterial.SetColor(ShaderIDs.SunColor, Color.black);
        }

        if(m_VolumeComponent.useUltraQuality.value == true)
        {
            m_Material.EnableKeyword("ULTRA_QUALITY");
            raysMaterial.SetFloat(ShaderIDs.SunDensity, m_VolumeComponent.rayDensity.value);
        } else
        {
            m_Material.DisableKeyword("ULTRA_QUALITY");
            raysMaterial.SetFloat(ShaderIDs.SunDensity, m_VolumeComponent.rayDensity.value);
        }
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Intensity, m_VolumeComponent.rayIntensity.value);
        }

        Vector3 sunPos = m_VolumeComponent.sunTransformPosition.value;
        var lightIndexMain = renderingData.lightData.mainLightIndex;
        var lights = renderingData.lightData.visibleLights;
        if (lightIndexMain >= 0)
        {
            var mainLight = lights.ElementAt(lightIndexMain);
            sunPos = mainLight.light.gameObject.transform.position;
        }

        SetGodraysShaderValues(m_Material, renderingData.cameraData.camera, sunPos);

        RenderTextureDescriptor descriptorOne = GetTempRTDescriptor(renderingData);
        cmd.GetTemporaryRT(_Main.id, descriptorOne);
        cmd.Blit(source, ShaderIDs.Input);

        // Create a temporary target
        RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);

        var intermed = RenderTexture.GetTemporary(descriptor);

        if (m_VolumeComponent.useMobileDownsampling.value == true)
        {
            descriptor.width = (int)(descriptor.width  / 2f);
            descriptor.height = (int)(descriptor.height / 2f);
        } else
        {
            descriptor.width = (int)(descriptor.width / 1.1f);
            descriptor.height = (int)(descriptor.height / 1.1f);
        }
        //cmd.GetTemporaryRT(_intermediate.id, descriptor, FilterMode.Bilinear);
        descriptor.autoGenerateMips = false;
        var bHalf = RenderTexture.GetTemporary(descriptor);
        //var bHalfBlit = RenderTexture.GetTemporary(descriptor);

        cmd.SetGlobalTexture(ShaderIDs.Input, renderingData.cameraData.renderer.cameraColorTarget);
        cmd.Blit(source, intermed, m_Material, 0);

        m_Material.SetTexture(ShaderIDs.SunTexIntermed, intermed);
        cmd.Blit(intermed, bHalf, m_Material, 1);

        //cmd.SetGlobalTexture(ShaderIDs.Input, renderingData.cameraData.renderer.cameraColorTarget);
        cmd.SetGlobalTexture(ShaderIDs.Input, _Main.id);
        cmd.SetGlobalTexture(ShaderIDs.SunTex, bHalf);
        cmd.Blit(_Main.id, destination, m_Material, 2);

        //cmd.ReleaseTemporaryRT(_intermediate.id);
        RenderTexture.ReleaseTemporary(bHalf);
        RenderTexture.ReleaseTemporary(intermed);

        cmd.ReleaseTemporaryRT(_Main.id);
    }

}
