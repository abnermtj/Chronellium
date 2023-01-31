using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

using UnityEngine.Rendering.Universal;//v1.1.8n
//using UnityEngine.Experimental.Rendering.Universal;
//using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering;
using System.Reflection;

[ExecuteInEditMode]
public class connectSuntoVolumeFogURP : MonoBehaviour
{
    [Header("------------------------------------------------------")]
    [Header("General Volumetric Lighting and Fog Setup")]
    [Header("------------------------------------------------------")]
    [Tooltip("Enable volume lighting and fog, use this instead of turn on-off the script itself.")]
    public bool enableFog = true;
    [Tooltip("Enable HDR mode, use depending on camera HDR usage.")]
    public bool allowHDR = true;// false; //v0.7
    public Transform sun;
    //v1.6
    public Camera reflectCamera;    

    //v1.1.8n - pipeline setup automation
    public bool setupForwardRenderer = false;//read camera renderer and assign forward renderer feature if not exist
    public Material volumeFogMaterial;//defaults to the predefined material which is setup in the sample URP forward renderer feature

    //v1.9.9.7 - Ethereal v1.1.8f
    [Header("------------------------------------------------------")]
    [TextArea]
    [Tooltip("Multiple cameras rendering")]
    public string ExtraCamerasNote = "Add any extra cameras that will render the volumes here. Each must have a separate forward renderer with the image effect" +
        "configured with IsForReflections, plus isForDualcameras checkboxes on and the Extra camera ID corresponding to the camera position in this list.";
    public List<Camera> extraCameras = new List<Camera>();

    //v1.7 - //v1.9.9.8 - Ethereal v1.1.8h
    [Tooltip("Local Spot or Point Lights maximum count (Up to 6), or use < 0 to enable infinite Lights with cutoff at Abs(LightCount) distance")]
    //[Range(1, 6)]
    public int lightCount = 3;

    //v1.5
    [Tooltip("Global volumetric lighting Intensity, zero disables the volume lighting and uses only fog")]
    public float blendVolumeLighting = 0.02f;//0;
    [Tooltip("Volumetric lighting Sampling Steps")]
    public float LightRaySamples = 12;//8;

    //v1.1.8f
    [Header("------------------------------------------------------")]
    [TextArea]
    public string volumeSamplingNote = "Volume sampling X can be set to zero to enable the zero noise mode for spot and point light volumes. Set to" +
        "one to add noise and more even spread of the light and may use any amount between zero and one to blend no noise region with variable noise amount.";
    ////v1.9.9.4 - Ethereal 1.1.7 - Control sampling for no noise option
    [Tooltip("Volume sampling control, (x) noise to no noise ratio, 0 is zero noise, (y) no noise sampling step length, (z,w) noise sampling step lengths (local,directional)")]
    public Vector4 volumeSamplingControl = new Vector4(0.001f,1,1,1);

    [Header("------------------------------------------------------")]
    [Header("Volume Wind Noise power & speed, Fog vs Volume control")]
    [Header("------------------------------------------------------")]

    //v1.9.9.7 - Ethereal v1.1.8f
    [TextArea]
    [Tooltip("Multiple cameras rendering")]
    public string stepControlNote = "The steps control X value can be set to a number other than zero to enable the steady step volume sampling mode," +
        "the number dictates the step distance, use zero to have the step depend on scene depth, this mode may create differences in the volume based" +
        "on the background depth across its length.";
    [Tooltip("Volume Sampling Method (x), Local Lights power (y-z), Wind Noise Strength(w)")]
    public Vector4 stepsControl = new Vector4(0, 0, 1, 1);
    [Tooltip("Volume lighting and Fog power (x-y), Wind Noise Freq (z) & Speed(w)")]
    public Vector4 lightNoiseControl = new Vector4(1f, 0.75f, 1, 1);

    //v1.9.9.3
    [Tooltip("Volume Shadow control (x-unity shadow distance, y,z-shadow atten power & offset, w-)")]
    public Vector4 shadowsControl = new Vector4(500, 1, 1, 0);

    [Header("------------------------------------------------------")]
    [Header("Impostor Lights Array (List of Spot-Point disabled lights)")]
    [Header("------------------------------------------------------")]
    //v1.9.9.1
    [Tooltip("Impostor spot and point Lights list, light component can be disabled and will still produce a volume based on their properties")]
    public List<Light> lightsArray = new List<Light>();
    //FOG
    [Tooltip("Single Impostor Local Volume Light")]
    public Light localLightA;
    public float localLightIntensity;
    public float localLightRadius;
    [Tooltip("Single Impostor Local Fog Light")]
    public Vector4 PointL = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    public Vector4 PointLParams = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

    [Header("------------------------------------------------------")]
    [Header("Light volumes Controls (By color and power)")]
    [Header("------------------------------------------------------")]
    //v1.1.8p
    [TextArea(6, 9)]
    //[TextArea]
    [Tooltip("Volumes Intensity Control")]
    public string volumesIntensityControlsNote = "The main controls of the volumes intensity are the LightControlA.x for the directional light" +
        " and the LightControlA.y for the Spot and Point lights. Note that LightControlA.x is a multiplier of intensity, and the LightControlA.y " +
        "is an intensity divider, thus the higher the value, the lower the local lights intensity, so use a high value if the scene if very bright. " +
        "Ideally use a value of one as starting point to regulate the intensity lower or higher by moving above or below one.";
    //v1.9.9
    [Tooltip("Sun volume power (x), Global Local lights Power (y), Local Light A-B Power (z-w)")]
    public Vector4 lightControlA = new Vector4(0.92f, 0.3f, 1, 1);
    [Tooltip("Local Light C_D-E-F Powers")]
    public Vector4 lightControlB = new Vector4(1, 1, 1, 1);
    public bool controlByColor = false;
    [Tooltip("Local light to activate, put any of R-G-B component at 128")]
    public Light lightA;
    [Tooltip("Local light to activate (Disabled - to be used in next Ethereal update")]
    public Light lightB;//grab colors of the two lights to apply volume to


    [Header("------------------------------------------------------")]
    [Header("Volumetric Heigth Fog parameters")]
    [Header("------------------------------------------------------")]

    //FOG URP /////////////
    //FOG URP /////////////
    //FOG URP /////////////
    //public float blend =  0.5f;
    [Tooltip("Global tint of the volume fog and lighting effect")]
    public Color _FogColor = Color.white / 1.7f;
    //fog params
    [Tooltip("Select 2D (0) or 3D noise (1)")]
    public int noise3D = 1;
    [Tooltip("Select 2D noise texture")]
    public Texture2D noiseTexture;
    [Tooltip("Fog start distance from camera, use higher value to push fog to the background or lower to bring towards the camera")]
    public float _startDistance = 250;//10000;//30f;
    [Tooltip("Select Fog maximum height in the scene")]
    public float _fogHeight = 10;//0.75f;
    [Tooltip("Select Fog density")]
    public float _fogDensity = 0.01f;//1f;
    [HideInInspector]
    public float _cameraRoll = 0.0f;
    [HideInInspector]
    public Vector4 _cameraDiff = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    [HideInInspector]
    public float _cameraTiltSign = 1;
    [Tooltip("Select Fog height based density")]
    public float heightDensity = 0.2f;//1;
    [Tooltip("Fog noise height based density")]
    public float noiseDensity = 0.2f;//1;
    [Tooltip("Fog noise scale, use a low enough value so noise pattern is not visible (e.g. 0.1 to 10)")]
    public float noiseScale = 0.1f;//1;
    [Tooltip("Fog noise distance to camera based density")]
    public float noiseThickness = 0.1f;//1;
    [Tooltip("Fog noise speed, use to emulate wind in x,y,z directions")]
    public Vector3 noiseSpeed = new Vector4(1f, 1f, 1f);
    [HideInInspector]
    public float startDistance = 1;

    [Header("------------------------------------------------------")]
    [Header("Fog Occlusion by scene Objects control")]
    [Header("------------------------------------------------------")]
    [Tooltip("Fog occlusion, use to allow bleeding of bright back fog above frontal scene objects, higher number occludes light more.")]
    public float occlusionDrop = 1f;
    [Tooltip("Fog occlusion exponent, control the falloff of frontal objects fog overlap, if enabled by Occlusion Drop variable.")]
    public float occlusionExp = 1f;



    [Header("------------------------------------------------------")]
    [Header("Volumetric Fog Atmospheric Scattering controls")]
    [Header("------------------------------------------------------")]
    [Tooltip("Fog scattering luminance control, smaller values allow more light")]
    public float luminance = 5;//1;
    [Tooltip("Fog scattering luminance falloff, smaller values allow more light")]
    public float lumFac = 0.5f;//1;
    [Tooltip("Fog light scattering factor, higher number will give an overall brighter image and increase light around the sun")]
    public float ScatterFac = 1;
    [Tooltip("Fog scattering turbidity factor, smaller values make more moody look, use -0.1 to 1000 range")]
    public float turbidity = 1;
    [Tooltip("Fog scattering turbidity falloff, smaller values make more moody look, use 0.000001 to 0.001 range")]
    public float TurbFac = 7;
    [Tooltip("Fog light scattering horizon control, higher number spreads the sun light more on the horizon around the sun, use 0 to 500 range")]
    public float HorizFac = 2;//1;
    [Tooltip("Fog scattering reileigh property, use 1 to 1000000 range")]
    public float reileigh = 1;
    [Tooltip("Fog scattering mie property, use 0.01 to 1 range")]
    public float mieCoefficient = 0.00001f;
    [Tooltip("Fog scattering mie directional focus, higher number focuses scatter light around the sun, use 0.01 to 1 range")]
    public float mieDirectionalG = 0.7f;//1;
    [Tooltip("Fog scattering bias, adjust overall scattering blightness falloff, use 0 to 10 range")]
    public float bias = 2.15f;//1;
    [Tooltip("Fog scattering contrast, adjust overall scattering contrast, use 0.1 to 100 range")]
    public float contrast = 1;
    [Tooltip("Fog scattering tint color, use to fine tune scattering color absorbion per channel")]
    public Color TintColor = new Color(1, 1, 1, 1);
    [Tooltip("Fog scattering tint parameters, use to fine tune scattering color absorbion per channel, use range 0 to 1000000")]
    public Vector3 TintColorK = new Vector3(0, 0, 0);
    [Tooltip("Fog scattering tint parameters, use to fine tune scattering color absorbion per channel, use range -1000 to 1000")]
    public Vector3 TintColorL = new Vector3(0, 0, 0);
    [HideInInspector]
    public Vector4 Sun = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

    [Header("------------------------------------------------------")]
    [Header("Volumetric Fog Sky Blending control")]
    [Header("------------------------------------------------------")]
    [Tooltip("Fog scattering applied to sky on and off. Turn on to make sky receive fog.")]
    public bool FogSky = true;
    [Tooltip("Fog scattering applied to sky amount, if enabled by the Fog Sky variable.")]
    public float ClearSkyFac = 0.5f;//1f;


    [Header("------------------------------------------------------")]
    [Header("Screen Space Sun Shafts module")]
    [Header("------------------------------------------------------")]

    //v1.9.9.2
    [Tooltip("Enable screen space sun shafts extra effect")]
    public bool enableSunShafts = false;//simple screen space sun shafts
    [Tooltip("Enable screen space sun shafts radial effect")]
    public bool _useRadialDistance = false;
    [HideInInspector]
    public bool _fadeToSkybox = true;
    //END FOG URP //////////////////
    //END FOG URP //////////////////
    //END FOG URP //////////////////

    //SUN SHAFTS
    [Tooltip("Screen space sun shafts blend mode, additive allows more bright look.")]
    public BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
    //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
    [Tooltip("Screen space sun shafts blur iterations count. More iteration make effect look smoother. Use range 1 to 3")]
    public int radialBlurIterations = 2;
    [Tooltip("Screen space sun shafts color")]
    public Color sunColor = Color.white;
    [Tooltip("Screen space sun shafts threshold color, above which sun shafts appear based on sky color brightness")]
    public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
    [Tooltip("Screen space sun shafts blur radius, adjust to get shafts extend towards camera and push back towards the sky, use 0 to 5 range")]
    public float sunShaftBlurRadius = 2.5f;
    [Tooltip("Screen space sun shafts intensity, use 0.1 to 50 range")]
    public float sunShaftIntensity = 1.15f;
    [Tooltip("Screen space sun shafts max blur radius, adjust to get a smoother or more edgy effect, use 0 to 5 range")]
    public float maxRadius = 0.75f;
    [Tooltip("Use Depth Texture based occlusion, Depth must be enabled on camera")]
    public bool useDepthTexture = true;
    //PostProcessProfile postProfile;

    //SSMS
    [Header("------------------------------------------------------")]
    [Header("Advanced Settings - Anti Aliasing")]
    [Header("------------------------------------------------------")]
    //v0.6    
    [Tooltip("Enable Temporal AA mode")]
    public bool enabledTemporalAA = false;
    [Tooltip("Temporal AA respose, lower give less noise but increase blur when camera moves")]
    public float TemporalResponse = 1;
    [Tooltip("Temporal AA Gain, lower give less noise but increase blur when camera moves")]
    public float TemporalGain = 1;
    [Tooltip("Use downsampling for better performance")]
    public int downSample = 1;
    [Tooltip("Dilate Depth at object egdes for best visuals in downsampling")]
    public float depthDilation = 1;
    //v0.6a
    public bool enableBlendMode = false;
    public float controlBackAlphaPower = 1;
    public float controlCloudAlphaPower = 0.001f;
    public Vector4 controlCloudEdgeA = new Vector4(1, 1, 1, 1);

    //SSMS
    #region Public Properties

    public bool enableComposite = false;
    public int downSampleAA = 1;//SSMS
    public bool enableWetnessHaze = false;

    float GammaToLinear(float x)
    {
#if UNITY_5_3_OR_NEWER
        return Mathf.GammaToLinearSpace(x);
#else
            if (x <= 0.04045f)
                return x / 12.92f;
            else
                return Mathf.Pow((x + 0.055f) / 1.055f, 2.4f);
#endif
    }
    /// Prefilter threshold (gamma-encoded)
    /// Filters out pixels under this level of brightness.
    public float thresholdGamma
    {
        get { return Mathf.Max(_threshold, 0); }
        set { _threshold = value; }
    }
    float LinearToGamma(float x)
    {
#if UNITY_5_3_OR_NEWER
        return Mathf.LinearToGammaSpace(x);
#else
            if (x <= 0.0031308f)
                return 12.92f * x;
            else
                return 1.055f * Mathf.Pow(x, 1 / 2.4f) - 0.055f;
#endif
    }

    /// Prefilter threshold (linearly-encoded)
    /// Filters out pixels under this level of brightness.
    public float thresholdLinear
    {
        get { return GammaToLinear(thresholdGamma); }
        set { _threshold = LinearToGamma(value); }
    }

    [HideInInspector]
    [SerializeField]
    [Tooltip("Filters out pixels under this level of brightness.")]
    public float _threshold = 0f;

    /// Soft-knee coefficient
    /// Makes transition between under/over-threshold gradual.
    public float softKnee
    {
        get { return _softKnee; }
        set { _softKnee = value; }
    }

    [HideInInspector]
    [SerializeField, Range(0, 1)]
    [Tooltip("Makes transition between under/over-threshold gradual.")]
    public float _softKnee = 0.5f;

    /// Bloom radius
    /// Changes extent of veiling effects in a screen
    /// resolution-independent fashion.
    public float radius
    {
        get { return _radius; }
        set { _radius = value; }
    }

    [Header("Scattering")]
    [SerializeField, Range(1, 7)]
    [Tooltip("Changes extent of veiling effects\n" +
             "in a screen resolution-independent fashion.")]

    public float _radius = 7f;

    /// Blur Weight
    /// Gives more strength to the blur texture during the combiner loop.
    public float blurWeight
    {
        get { return _blurWeight; }
        set { _blurWeight = value; }
    }

    [SerializeField, Range(0.1f, 100)]
    [Tooltip("Higher number creates a softer look but artifacts are more pronounced.")] // TODO Better description.
    public float _blurWeight = 1f;

    /// Bloom intensity
    /// Blend factor of the result image.
    public float intensity
    {
        get { return Mathf.Max(_intensity, 0); }
        set { _intensity = value; }
    }

    [SerializeField]
    [Tooltip("Blend factor of the result image.")]
    [Range(0, 1)]
    public float _intensity = 1f;

    /// High quality mode
    /// Controls filter quality and buffer resolution.
    public bool highQuality
    {
        get { return _highQuality; }
        set { _highQuality = value; }
    }

    [SerializeField]
    [Tooltip("Controls filter quality and buffer resolution.")]
    public bool _highQuality = true;

    /// Anti-flicker filter
    /// Reduces flashing noise with an additional filter.
    [SerializeField]
    [Tooltip("Reduces flashing noise with an additional filter.")]
    public bool _antiFlicker = true;

    public bool antiFlicker
    {
        get { return _antiFlicker; }
        set { _antiFlicker = value; }
    }

    /// Distribution texture
    [SerializeField]
    [Tooltip("1D gradient. Determines how the effect fades across distance.")]
    public Texture2D _fadeRamp;

    public Texture2D fadeRamp
    {
        get { return _fadeRamp; }
        set { _fadeRamp = value; }
    }

    /// Blur tint
    [SerializeField]
    [Tooltip("Tints the resulting blur. ")]
    public Color _blurTint = Color.white;

    public Color blurTint
    {
        get { return _blurTint; }
        set { _blurTint = value; }
    }

    #endregion
    //END SSMS

    // Start is called before the first frame update
    void Start()
    {
        //postProfile = GetComponent<PostProcessVolume>().profile;
        //v1.6
        //if (reflectCamera == null)
        //{
        //    PlanarReflectionsSM_LWRP reflectScript = GetComponent<PlanarReflectionsSM_LWRP>();
        //    if (reflectScript != null && reflectScript.outReflectionCamera != null)
        //    {
        //        reflectCamera = reflectScript.outReflectionCamera;
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {

        //v1.1.8n - pipeline setup automation
        if (setupForwardRenderer)
        {
#if (UNITY_EDITOR)
            //https://forum.unity.com/threads/access-renderer-feature-settings-at-runtime.770918/
            //RenderPipeline pipeURP = RenderPipelineManager.currentPipeline;
            UniversalRenderPipelineAsset pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
            //ScriptableRenderer rendererURP = pipeline.GetRenderer(0);
            // rendererURP.supportedRenderingFeatures
            //pipeline.GetRenderer(0);
            //ScriptableRendererData renderDATA = pipeline.GetRenderer(0).fe
            //ScriptableRendererData renderDATA = pipeline.LoadBuiltinRendererData(RendererType.Custom);
            

            //m_DefaultRendererIndex
            FieldInfo propertyInfoA = pipeline.GetType().GetField("m_DefaultRendererIndex", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            int rendererDefaultIndex = ((int)propertyInfoA?.GetValue(pipeline));
            Debug.Log("Default renderer ID = " + rendererDefaultIndex);

            FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);//REFLECTION
            ScriptableRendererData renderDATA = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[rendererDefaultIndex];

            //BlitVolumeFogSRP volumeFOGFeature = (BlitVolumeFogSRP)renderDATA.rendererFeatures[0];
            List<ScriptableRendererFeature> features = renderDATA.rendererFeatures;
            bool foundFogFeature = false;
            for (int i = 0; i < features.Count; i++)
            {
                //if find, all good, if not set it up
                if (features[i].GetType() == typeof(BlitVolumeFogSRP)) //if (features[i].name == "NewBlitVolumeFogSRP")
                {
                    foundFogFeature = true;
                }
                //Debug.Log(features[i].name);
            }
            if (foundFogFeature)
            {
                Debug.Log("The Ethereal volumetric fog forward renderer feature is already added in the Default renderer in the URP pipeline asset.");
            }
            else
            {
                //SET IT UP
                if (volumeFogMaterial != null)
                {
                    BlitVolumeFogSRP volumeFOGFeature = ScriptableObject.CreateInstance<BlitVolumeFogSRP>(); //new BlitVolumeFogSRP();
                    volumeFOGFeature.settings.blitMaterial = volumeFogMaterial;
                    volumeFOGFeature.name = "NewBlitVolumeFogSRP";
                    ScriptableRendererFeature BlitVolumeFogSRPfeature = volumeFOGFeature as ScriptableRendererFeature;
                    BlitVolumeFogSRPfeature.Create();
                    renderDATA.rendererFeatures.Add(BlitVolumeFogSRPfeature);
                    renderDATA.SetDirty();
                    Debug.Log("The Ethereal volumetric fog forward renderer feature is now added in the Default renderer in the URP pipeline asset.");
                }
                else
                {
                    Debug.Log("The Ethereal volumetric fog material is not assigned, please assign the 'VolumeFogSRP_FORWARD_URP' material in the 'VolumeFogMaterial' slot in the " +
                        "connect script and then enable the 'Setup Forward Renderer' checkbox to setup the fog.");
                }
            }
            //FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            //_scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
            // renderObjects = (RenderObjects)_scriptableRendererData.rendererFeatures[0];
            //renderObjects.settings.stencilSettings.stencilCompareFunction = CompareFunction.Equal;

            //ForwardRendererData rendererData;
            //var blurFeature = rendererData.rendererFeatures.OfType<fog>().FirstOrDefault();
            //if (blurFeature == null) return;
            //blurFeature.settings.BlurAmount = value;
            //rendererData.SetDirty();

            //UniversalRenderPipelineAsset currentURP = scriptab
            //GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.fe
            //Camera.main.renderingPath = RenderingPath.
#endif
            setupForwardRenderer = false;
        }


        if(sun != null)
        {
            UpdateFOG();
        }

        //v1.6
        //if(reflectCamera == null)
        //{
        //    PlanarReflectionsSM_LWRP reflectScript = GetComponent<PlanarReflectionsSM_LWRP>();
        //    if(reflectScript != null && reflectScript.outReflectionCamera != null)
        //    {
        //        reflectCamera = reflectScript.outReflectionCamera;
        //    }
        //}
    }


    //FOG

    // Update is called once per frame
    //float _cameraRoll;
   // Vector4 _cameraDiff;
   // Vector4 PointLParams;
   // Vector4 Sun;
   // Vector4 PointL;
   // int _cameraTiltSign;
    void UpdateFOG()
    {
        var volFog = this; //The custom forward renderer will read variables from this script

        //var volFog = postProfile.GetSetting<VolumeFogSM_SRP>();
        if (volFog != null)
        {
            if (localLightA != null)
            {

                //volFog.sunTransform.value = sun.transform.position;
            }
            Camera cam = Camera.main;//Camera cam = Camera.current; //v1.7.1 - Solve editor flickering
            if (cam == null)
            {
                cam = Camera.main;
            }
            volFog._cameraRoll = cam.transform.eulerAngles.z;

            volFog._cameraDiff = cam.transform.eulerAngles;// - prevRot;

            if (cam.transform.eulerAngles.y > 360)
            {
                volFog._cameraDiff.y = cam.transform.eulerAngles.y % 360;
            }
            if (cam.transform.eulerAngles.y > 180)
            {
                volFog._cameraDiff.y = -(360 - volFog._cameraDiff.y);
            }

            //slipt in 90 degs, 90 to 180 mapped to 90 to zero
            //volFog._cameraDiff.value.w = 1;
            if (volFog._cameraDiff.y > 90 && volFog._cameraDiff.y < 180)
            {
                volFog._cameraDiff.y = 180 - volFog._cameraDiff.y;
                volFog._cameraDiff.w = -1;
                //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w ,- 1, Time.deltaTime * 20);
            }
            else if (volFog._cameraDiff.y < -90 && volFog._cameraDiff.y > -180)
            {
                volFog._cameraDiff.y = -180 - volFog._cameraDiff.y;
                volFog._cameraDiff.w = -1;
                //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, -1, Time.deltaTime * 20);
                //Debug.Log("dde");
            }
            else
            {
                //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, 1, Time.deltaTime * 20);
                volFog._cameraDiff.w = 1;
            }

            //vertical fix
            if (cam.transform.eulerAngles.x > 360)
            {
                volFog._cameraDiff.x = cam.transform.eulerAngles.x % 360;
            }
            if (cam.transform.eulerAngles.x > 180)
            {
                volFog._cameraDiff.x = 360 - volFog._cameraDiff.x;
            }
            //Debug.Log(cam.transform.eulerAngles.x);
            if (cam.transform.eulerAngles.x > 0 && cam.transform.eulerAngles.x < 180)
            {
                volFog._cameraTiltSign = 1;
            }
            else
            {
                // Debug.Log(cam.transform.eulerAngles.x);
                volFog._cameraTiltSign = -1;
            }
            if (sun != null)
            {
                Vector3 sunDir = sun.transform.forward;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
                // volFog.Sun.value = -new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                volFog.Sun = new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                //volFog.sun.position = new Vector3(sunDir.x, sunDir.y, sunDir.z);////// vector 4 to vector3
            }
            else
            {
                volFog.Sun = new Vector4(15, 0, 1, 1);
            }
            if (localLightA != null)
            {
                volFog.PointL = new Vector4(localLightA.transform.position.x, localLightA.transform.position.y, localLightA.transform.position.z, localLightIntensity);
                volFog.PointLParams = new Vector4(localLightA.color.r, localLightA.color.g, localLightA.color.b, localLightRadius);
            }
            //Debug.Log(volFog._cameraDiff.value);
            //prevRot = cam.transform.eulerAngles;
        }
    }

    //END FOG

}
