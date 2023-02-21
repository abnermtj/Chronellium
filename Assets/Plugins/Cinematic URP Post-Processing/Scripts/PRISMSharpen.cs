using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using System;
using System.Linq;


// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Cinematic URP Post Processing/CUPP - PRISM Sharpen")]
public class PRISMSharpen : VolumeComponent
{
    [Tooltip("Intensity of the effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0, 0, 1);

    [Tooltip("Use a second sharpen pass")]
    public BoolParameter useMultiPassSharpen = new BoolParameter(false);// (0, 0, 1);

    [Tooltip("Make the first sharpen pass depth-aware")]
    public BoolParameter useDepthAwareSharpen = new BoolParameter(false);// (0, 0, 1);
}

namespace PRISM.Utils
{
    static class GaussianKernel
    {
        public static double Correlation(double[] array1, double[] array2)
        {
            double[] array_xy = new double[array1.Length];
            double[] array_xp2 = new double[array1.Length];
            double[] array_yp2 = new double[array1.Length];
            for (int i = 0; i < array1.Length; i++)
                array_xy[i] = array1[i] * array2[i];
            for (int i = 0; i < array1.Length; i++)
                array_xp2[i] = Math.Pow(array1[i], 2.0);
            for (int i = 0; i < array1.Length; i++)
                array_yp2[i] = Math.Pow(array2[i], 2.0);
            double sum_x = 0;
            double sum_y = 0;
            foreach (double n in array1)
                sum_x += n;
            foreach (double n in array2)
                sum_y += n;
            double sum_xy = 0;
            foreach (double n in array_xy)
                sum_xy += n;
            double sum_xpow2 = 0;
            foreach (double n in array_xp2)
                sum_xpow2 += n;
            double sum_ypow2 = 0;
            foreach (double n in array_yp2)
                sum_ypow2 += n;
            double Ex2 = Math.Pow(sum_x, 2.00);
            double Ey2 = Math.Pow(sum_y, 2.00);

            return (array1.Length * sum_xy - sum_x * sum_y) /
                   Math.Sqrt((array1.Length * sum_xpow2 - Ex2) * (array1.Length * sum_ypow2 - Ey2));
        }

        static readonly Dictionary<int, double> idealStandardDeviation = new Dictionary<int, double>
        {
            { 5, 7.013915463849E-01 },
            { 7, 8.09171316279E-01 },
            { 9, 9.0372907227E-01 },
            { 11, 9.890249035E-01 },
            { 13, 1.067359295 },
            { 15, 1.1402108 },
            { 17, 1.2086 }
        };

        public static double IdealStandardDeviation(int taps)
        {
            if (!idealStandardDeviation.ContainsKey(taps))
                throw new ArgumentException("No ideal standard deviation for this tap count : " + taps, "taps");

            return idealStandardDeviation[taps];
        }

        public static float[] GetWeights(int taps)
        {
            return GetWeights(taps, IdealStandardDeviation(taps));
        }
        public static float[] GetWeights(int taps, double standardDeviation)
        {
            if (taps < 1 || taps % 2 == 0)
                throw new ArgumentException("Invalid tap count : " + taps, "taps");

            return GetWeightsInternal(taps, standardDeviation).Select(x => (float)x).ToArray();
        }

        public static void GetFastWeights(int taps, out float[] weights, out float[] offsets)
        {
            GetFastWeights(taps, IdealStandardDeviation(taps), out weights, out offsets);
        }
        public static void GetFastWeights(int taps, double standardDeviation, out float[] weights, out float[] offsets)
        {
            if (taps < 1 || taps % 2 == 0)
                throw new ArgumentException("Invalid tap count : " + taps, "taps");

            double[] tempWeights = GetWeightsInternal(taps, standardDeviation);

            bool hasEndTap = tempWeights.Length % 2 == 0;
            int joinedTaps = (tempWeights.Length - 1) / 2,
                totalTaps = joinedTaps + 1 + (hasEndTap ? 1 : 0);

            offsets = new float[totalTaps];
            weights = new float[totalTaps];

            weights[0] = (float)tempWeights[0];
            offsets[0] = 0;

            for (int i = 0; i < joinedTaps; i++)
            {
                double sum = tempWeights[i * 2 + 1] + tempWeights[i * 2 + 2];
                weights[i + 1] = (float)sum;
                offsets[i + 1] = (float)(0.5 - tempWeights[i * 2 + 1] / sum);
            }

            if (hasEndTap)
                weights[weights.Length - 1] = (float)tempWeights[tempWeights.Length - 1];
        }

        static double[] GetWeightsInternal(int taps, double standardDeviation)
        {
            int halfTaps = (taps - 1) / 2 + 1;

            var weights = new double[halfTaps];
            for (int i = 0; i < halfTaps; i++)
                weights[i] = Gaussian(i, standardDeviation);

            double augmentationFactor = 1 + LostLight(taps, standardDeviation);
            for (int i = 0; i < halfTaps; i++)
                weights[i] *= augmentationFactor;

            return weights;
        }

        public static double LostLight(int taps, double standardDeviation)
        {
            int halfTaps = (taps - 1) / 2 + 1;

            double sum = Gaussian(0, standardDeviation);
            for (int i = 1; i < halfTaps; i++)
                sum += Gaussian(i, standardDeviation) * 2;

            return -1 + 1 / sum;
        }

        public static double BoxLikeness(int taps, double standardDeviation)
        {
            int halfTaps = (taps - 1) / 2 + 1;

            var weights = new double[halfTaps];
            for (int i = 0; i < halfTaps; i++)
                weights[i] = Gaussian(i, standardDeviation);

            const double RelativeEpsilon = 1E-14;
            double invTaps = 1.0 / taps;
            IEnumerable<double> boxWeights = Enumerable.Repeat(invTaps + RelativeEpsilon, 1)
                .Concat(Enumerable.Repeat(invTaps - RelativeEpsilon / (halfTaps - 1), halfTaps - 1));

            double correlation = 1 - Correlation(weights, boxWeights.ToArray());// weights.Correlation(boxWeights);

            const double minAlpha = -1.08975982634075E-03, minA = 1.56809355440769E-02, minB = -3.46280631112407E-02, minC = 6.75067153072151E-02;
            const double maxAlpha = 1.78780869266509E-03, maxA = -3.68630036049787E-02, maxB = 2.59171745554970E-01, maxC = -2.49537606767719E-01;

            double minCorrected = correlation - (minAlpha * Math.Pow(halfTaps, 3) + minA * Math.Pow(halfTaps, 2) + minB * halfTaps + minC);
            double maxCorrected = minCorrected / (maxAlpha * Math.Pow(halfTaps, 3) + maxA * Math.Pow(halfTaps, 2) + maxB * halfTaps + maxC);

            return Math.Min(Math.Max(maxCorrected, 0), 1);
        }

        static double Gaussian(int distance, double standardDeviation)
        {
            return 1 / (Math.Sqrt(2 * Math.PI) * standardDeviation) *
                Math.Exp(-Math.Pow(distance, 2) / (2 * Math.Pow(standardDeviation, 2)));
        }
    }
}



// Define the renderer for the custom post processing effect
[CustomPostProcess("CUPP - PrismSharpen", CustomPostProcessInjectionPoint.BeforePostProcess)]
public class PRISMSharpen_URP : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private PRISMSharpen m_VolumeComponent;

    // The postprocessing material
    private Material m_Material;


    private RenderTargetHandle _Main = default;
    private RenderTargetHandle _intermediate = default;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Intensity = Shader.PropertyToID("_Intensity");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    /// Specifies the input needed by this custom post process. Default is Color only.
    public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Depth;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        m_Material = CoreUtils.CreateEngineMaterial("PRISM/PRISMFullscreenSharpenPass");
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<PRISMSharpen>();

        _Main = new RenderTargetHandle();
        _Main.Init("_MainTex");

        _intermediate = new RenderTargetHandle();
        _intermediate.Init("_IntermediateTex");

        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.intensity.value > 0;
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Intensity, m_VolumeComponent.intensity.value);
        }

        RenderTextureDescriptor descriptorOne = GetTempRTDescriptor(renderingData);
        cmd.GetTemporaryRT(_Main.id, descriptorOne);
        cmd.Blit(source, ShaderIDs.Input);

        // set source texture
        cmd.SetGlobalTexture(ShaderIDs.Input, source);
        
        if (m_VolumeComponent.useDepthAwareSharpen.value == true)
        {
            m_Material.EnableKeyword("MEDIAN_SHARPEN");
            m_Material.EnableKeyword("DEPTH_SHARPEN");
        } else
        {
            m_Material.DisableKeyword("DEPTH_SHARPEN");
            m_Material.DisableKeyword("MEDIAN_SHARPEN");
        }


        if(m_VolumeComponent.useMultiPassSharpen.value == true)
        {
            // Create a temporary target
            RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);
            cmd.GetTemporaryRT(_intermediate.id, descriptor, FilterMode.Bilinear);


            m_Material.EnableKeyword("MEDIAN_SHARPEN");
            cmd.Blit(source, _intermediate.Identifier(), m_Material, 0);


            m_Material.DisableKeyword("MEDIAN_SHARPEN");
            cmd.SetGlobalTexture(ShaderIDs.Input, _intermediate.id);
            //cmd.Blit(_intermediate.Identifier(), renderingData.cameraData.renderer.cameraColorTarget, m_Material, 0);
            cmd.Blit(_intermediate.Identifier(), destination, m_Material, 0);

            cmd.ReleaseTemporaryRT(_intermediate.id);
        } else
        {

            cmd.SetGlobalTexture(ShaderIDs.Input, _Main.id);
            cmd.Blit(_Main.id, destination, m_Material);
            //RTHandle sourceHandle = RTHandle.Alloc();
            //Blitter.BlitCameraTexture(cmd, renderingData.cameraData.targetTexture.colorBuffer.)
            //CoreUtils.DrawFullScreen(cmd, m_Material, renderingData.cameraData.renderer.cameraColorTarget);
        }

        cmd.ReleaseTemporaryRT(_Main.id);
    }

}
