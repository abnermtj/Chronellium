using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering.Universal;
using PRISM.Utils;


[VolumeComponentEditor(typeof(PRISMEffects))]
public class PRISMEffectsEditor : VolumeComponentEditor
{
    public Texture2D editorTex;
    const string editorTextureStringb64 = "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";

    SerializedObject serObj;

    SerializedDataParameter m_Intensity;
    SerializedDataParameter showInSceneView;
    SerializedDataParameter colorTemperature;
    SerializedDataParameter colorTint;
    SerializedDataParameter exposure;
    SerializedDataParameter useColorCorrection;

    SerializedDataParameter shadows;
    SerializedDataParameter midtones;
    SerializedDataParameter highlights;

    SerializedDataParameter colorVibrance;
    SerializedDataParameter contrast;

    SerializedDataParameter lightness;
    SerializedDataParameter labA_GreenAndMagenta;
    SerializedDataParameter labB_YellowAndBlue;

    SerializedDataParameter useBloom;
    SerializedDataParameter useHQBloom;
    SerializedDataParameter increaseBloomClarity;
    SerializedDataParameter useBloomStab;
    SerializedDataParameter bloomStrength;
    SerializedDataParameter bloomThreshold;
    SerializedDataParameter bloomRadius;
    SerializedDataParameter useLensDirt;
    SerializedDataParameter bloomDirtTexture;
    SerializedDataParameter lensDirtExposure;
    SerializedDataParameter bloomSaturation;

    SerializedDataParameter VignetteStrength;

    SerializedDataParameter useLensDistortion;
    SerializedDataParameter useChromaticAberration;
    SerializedDataParameter glassDispersion;
    SerializedDataParameter petzvalDistortionValue;
    SerializedDataParameter barrelDistortion;
    SerializedDataParameter chromaticColorWeights;

    SerializedDataParameter sensorNoise;
    SerializedDataParameter sensorPixelSize;
    SerializedDataParameter useDither;
    SerializedDataParameter useFilmicNoise;
    SerializedDataParameter noiseBlueSaturation;
    SerializedDataParameter noiseGreenSaturation;
    SerializedDataParameter noiseRedSaturation;
    SerializedDataParameter useTVNoise;
    SerializedDataParameter ditherBitDepth;
    SerializedDataParameter noiseTVSpeed;
    SerializedDataParameter noiseTVWidth;
    SerializedDataParameter noiseTVAngle;

    SerializedDataParameter useLut;
    SerializedDataParameter useCubeLUTs;
    SerializedDataParameter twoDLookupTex;
    SerializedDataParameter lutLerpAmount;
    SerializedDataParameter useSecondLut;
    SerializedDataParameter secondaryTwoDLookupTex;
    SerializedDataParameter secondaryLutLerpAmount;

    SerializedDataParameter tonemap;
    SerializedDataParameter gammaOffset;

    SerializedDataParameter useLinearSpace;

    private GUILayoutOption[] knobOptions;

    public static bool useHDRBloomIntensity = true;
    //START LUT VARIABLES==================================

   public string currentEffectsString = "";

    #region old
    /*
    private Texture2D tempClutTex2D;
    private Texture2D tempSecondClutTex2D;

    
    bool showToneParams = false;

    static bool needsToSave = false;
    string extraSaveString = "";
    string extraLoadString = "";
    
    static bool showBloom = true;
    static bool showChromatic = true;
    static bool showLensDistortions = true;
    static bool showNoise = true;
    static bool showDof = true;
    static bool showAdvancedBloom = false;
    //	static bool showAdvancedRays = false;
    static bool showTonemap = true;
    //static bool showColorify = true;
    static bool showFog = true;
    //	static bool showRays = true;
    static bool showExposure = true;
    static bool showLut = true;
    //static bool showNV = true;
    static bool showAO = true;*/

    GUIContent colorContent = new GUIContent("Use Color Correction", "Enables Color Correction effects");
    GUIContent colorTempContent = new GUIContent("   >Color Temperature (Warm>Cool)", "Controls the temperature of the color, used to offset light color for a neutral scene");
    GUIContent colorTintContent = new GUIContent("   >Color Tint (Magenta>Green)", "Controls the tint of the color, used to make fine corrections");
    GUIContent exposureContent = new GUIContent("   >Exposure Compensation", "Pushes the brightness curve towards the right (or left, if negative) of the image, in stops of brightness");
    GUIContent colorVibranceContent = new GUIContent("   >Vibrance", "Increases the saturation of colours in a more realistic way, with a reduced effect on already saturated colours");
    GUIContent contrastContent = new GUIContent("   >Contrast", "Increases the perceptual difference between colours in the scene");

    GUIContent lightnessContent = new GUIContent("   >Lightness", "Performs a smooth brightness adjustment to the entire scene based on a bezier curve. A steeper curve will create more brightness at either shadows or highlights.");
    GUIContent labA_GreenAndMagentaContent = new GUIContent("   >Green & Magenta Chromacity", "Controls the vibrance of green & magenta hues.");
    GUIContent labB_YellowAndBlueContent = new GUIContent("   >Yellow & Blue Chromacity", "Controls the vibrance of yellow & blue hues.");

    GUIContent lensDistortContent = new GUIContent("Use Lens Distortions", "Simulates lens-induced distortions, such as vignetting, barrel/pincushion distortion, chromatic aberration and fringing.");
    GUIContent glassDispersionContent = new GUIContent("   >Lens Fringing", "Simulates Color Fringing as a result of glass-misalignment-induced dispersion of light");
    GUIContent petzvalDispersionContent = new GUIContent("   >Petzval Distortion", "Makes the corners of the frame appear swirly, originally as a result of some carl-zeiss Biotar lens designs");
    GUIContent lensDistortPincushionContent = new GUIContent("   >Barrel Distortion", "Curves straight lines in either the edges or the centre of the frame as a result of a field curve");

    GUIContent useChromabContent = new GUIContent("   >Use Chromatic Aberrations", "Simulates misalignment in color wavelengths towards the edges of the lens, ie. blurry corners");
    GUIContent chromaticIntensityContent = new GUIContent("   >Chromatic Intensity", "Increases the intensity of the chromatic abberation effect");
    GUIContent chromaticTypeContent = new GUIContent("   >Aberration Type", "Vignette aberration applies to the corners of the image, vertical aberration applies to the vertical edges");

    GUIContent chromaticBlurContent = new GUIContent("   >Blur Edges", "Applies a blur to the areas of the screen affected by chromatic aberration. Requires 2 additional blur passes.");
    GUIContent chromaticBlurWidthContent = new GUIContent("     >Edge Blur Amount", "The width of the blur applied. Does not affect performance.");

    GUIContent linearSpaceContent = new GUIContent("Use Linear Space", "Performs colour operations in linear space");


    //    [InspectorName("Color Temperature (Warm>Cool)"), Tooltip("Controls the temperature of the color, used to offset light color for a neutral scene")]
    GUIContent bloomContent = new GUIContent("Use Bloom", "Simulates the light fringing & bleeding artifacts of a camera lens");
    GUIContent bloomHDRSliderContent = new GUIContent("   >Use HDR Bloom Intensity Slider", "Corrects the slider for use with HDR Bloom");
    GUIContent bloomTypeContent = new GUIContent("   >Bloom Type", "The method used to create the bloom texture. HDR bloom is recommended for any HDR scene, simple bloom is marginally cheaper and works better with LDR/stylised scenes.");
    GUIContent bloomTextureSizeContent = new GUIContent("   >Bloom Downsample", "Increasing this value offers more performance, and a larger bloom area, at the cost of temporal stability");
    GUIContent bloomThreshTypeContent = new GUIContent("   >Bloom Threshold Method", "Gradual: Better for 'full-screen' bloom, Highlights: great for only blooming very bright areas, Curve: Like gradual, but more expensive and more control");
    public GUIContent bloomBlurPassesContent = new GUIContent("   >Bloom Blur Passes", "# of blur passes to apply to the bloom texture. More blur passes are more expensive, particularly if you are using a large base bloom texture");
    GUIContent bloomIntensityContent = new GUIContent("   >Bloom Intensity", "Increases the intensity of the bloom effect");
    //GUIContent bloomExposureContent = new GUIContent("   >Bloom Exposure", "Adjusts the amount of exposure applied to the bloom texture");
    GUIContent bloomThresholdContent = new GUIContent("   >Bloom Threshold", "(OPTIONAL) - The bloom threshold may be desirable in scenes where you only want the brightest pixels to contribute to bloom");
    GUIContent bloomKneeContent = new GUIContent("   >Bloom Saturation", "Knee of the quadratic curve");
    GUIContent bloomHQContent = new GUIContent("   >Use HQ Bloom", "Uses a high quality bloom effect");
    GUIContent bloomClarityContent = new GUIContent("     >Use Highlight-Weighted Threshold", "Increases clarity of bloom, decreases spread");
    GUIContent bloomRadContent = new GUIContent("   >Bloom Spread", "Radius of the blur filter used when blooming. Warning: Setting too high may cause cache misses (lower performance) - always profile before and after you set this");
    GUIContent bloomStabilityContent = new GUIContent("   >Use Stablility Buffer", "Uses the previous frames bloom texture to reduce 'firefly' artifacts");
    GUIContent bloomLensDirtContent = new GUIContent("   >Use Lens Dirt", "Applies the lens dirt texture to brighter areas of the image");

    GUIContent bloomLensDirtTextureContent = new GUIContent("     >Lens Dirt Texture", "Requires alpha channel");
    GUIContent bloomLensDirtIntensityContent = new GUIContent("     >Lens Dirt Intensity", "Increases the intensity of the lens dirt texture");
    GUIContent bloomDebugTextureContent = new GUIContent("   >Show Bloom Texture", "Shows you the bloom texture that is generated by the bloom pre-pass. Useful for debugging");
    GUIContent bloomBlurScreenContent = new GUIContent("     >Blur screen with bloom tex", "(Does not work with DoF) - Blurs the screen using the generated bloom texture, which has already undergone blur passes");
    GUIContent bloomUseScreenAddContent = new GUIContent("   >Use Screen Blend", "A smoother way of blending the bloom texture to the main texture, which also retains more detail. Only available in LDR.");

    GUIContent bloomAdvancedContent = new GUIContent("   Show Advanced Values", "");

    GUIContent bloomFlaresContent = new GUIContent("   >Use Lens Flares", "Applies a screen-space lens flare to the image using a modified bloom texture");
    GUIContent bloomFlareIntensityContent = new GUIContent("     >Flare Intensity", "Intensity of the lens flares in the final image");
    GUIContent bloomFlareChromContent = new GUIContent("     >Flare Chrom. Aberration", "Intensity of the chromatic aberration/distortion in the flare");
    GUIContent bloomFlareGhostNumContent = new GUIContent("     >Flare # of Ghosts", "Number of elements in the flare");
    GUIContent bloomFlareGhostSpaceContent = new GUIContent("     >Flare Ghost Spacing", "Distance of each element of the flare away from the last one");
    GUIContent bloomFlareHaloContent = new GUIContent("     >Flare Halo Width", "Width of the halo of the flare");
    //GUIContent bloomFlareColorTexContent = new GUIContent ("     >Flare Color Texture", "Color of lens flare across the screen");
    GUIContent bloomStarburstColorTexContent = new GUIContent("     >Flare Starburst Texture", "Starburst texture to add a diffraction pattern and color onto the flare");
    GUIContent bloomFlareDirtMultContent = new GUIContent("     >Flare Dirt Multiplier", "Scales the intensity of the lens dirt effect that is added to the flares texture.");

    GUIContent bloomUseUIBlurContent = new GUIContent("   >Use Bloom tex for UI blur", "Stores the blurred full-screen texture generated by the bloom effect for use with the PRISM/UI Blur shader. That shader will fallback to a lower performance GrabPass method if this is not checked.");
    GUIContent bloomUIBlurPassNumberContent = new GUIContent("     >Blur pass to grab from", "The bloom blur pass number that we will grab the full-screen blur texture from, the earlier the pass, the less blur. No performance impact.");



    GUIContent dofContent = new GUIContent("Use Depth of Field", "Simulates camera lens defocus & bokeh artifacts");
    GUIContent dofRadiusContent = new GUIContent("   >DoF Radius", "Increases maximum blur radius and bokeh size. No impact on performance");
    GUIContent dofSampleContent = new GUIContent("   >DoF Sample Count", "Increases size of blur & bokeh, large impact on performance. Low is suitable in most cases");
    GUIContent dofBokehFactorContent = new GUIContent("   >DoF Bokeh Factor", "Increases propensity to generate Bokeh");
    GUIContent dofPointContent = new GUIContent("   >DoF Focus Point", "Distance at which to focus the camera");
    GUIContent dofNearPointContent = new GUIContent("     >DoF Near Focus Point", "Distance at which to focus the camera for near blur");
    GUIContent dofDistanceContent = new GUIContent("   >DoF Focus Range", "DoF blur plane distance until it reaches maximum blur");
    GUIContent dofNearblurContent = new GUIContent("   >Use Near Blur", "When ticked, the Depth of Field will also blur pixels near the camera and outside of focal range");
    GUIContent dofNearDistanceContent = new GUIContent("     >DoF Near Focus Range", "DoF close blur plane distance until it reaches maximum blur");
    public GUIContent dofMedianContent = new GUIContent("   >Use Median Filter", "Shares a median filter (for stability) with other effects - disable this on mobile");
    GUIContent dofBlurSkyboxContent = new GUIContent("   >Blur Skybox", "If enabled, the DoF effect will blur the skybox");
    GUIContent dofStabContent = new GUIContent("     >Use DoF Stablility Buffer", "If enabled, passes the screen through a median filter to generate a more stable texture for Bokeh (if ticked, this texture is also used for more stable Godrays and Bloom)");
    GUIContent dofDebugContent = new GUIContent("     >Visualise Focus", "Shows you the focus of the camera, where white is fully focused, and black is fully blurred. Useful for debugging");
    public GUIContent dofAdvancedContent = new GUIContent("   Show Advanced Values", "Show advanced variables of the DoF effect");
    GUIContent dofDownsampleContent = new GUIContent("     >DoF Downsample", "Increasing this value offers more performance, and a larger blur area, at the cost of temporal stability");

    GUIContent vignetteContent = new GUIContent("Use Vignette", "Darkens the corners of the image");
    GUIContent vignetteStrengthContent = new GUIContent("   >Vignette Strength", "Darkens the corners of the image");
    //GUIContent advVignetteContent = new GUIContent("   >Show Advanced Values", "Exposes the vignette start and end variables of the vignette, for a more controlled effect");

    GUIContent noiseContent = new GUIContent("Use Filmic Noise", "Adds semi-procedural filmic noise across the image");
    GUIContent noiseIntensityContent = new GUIContent("   >Noise Intensity", "Increases the intensity of the noise effect");
    GUIContent noiseSizeContent = new GUIContent("   >Sensor Pixel Size", "Increases the size of the pixels in the noise");
    GUIContent noiseRedSaturationContent = new GUIContent("     >Noise Red Channel", "Value of Red in the noise pixels");
    GUIContent noiseGreenSaturationContent = new GUIContent("     >Noise Green Channel", "Value of Green in the noise pixels");
    GUIContent noiseBlueSaturationContent = new GUIContent("     >Noise Blue Channel", "Value of Blue in the noise pixels");
    GUIContent useDitherContent = new GUIContent("Use Dither", "Adds a dither effect to the image");
    GUIContent ditherBitDepthContent = new GUIContent("   >Dither Bit Depth", "Dithers the image to only use a limited bit depth");
    GUIContent noiseTVContent = new GUIContent("   >Use TV Noise Animation", "Simulates a TV style noise/scanline effect");
    GUIContent noiseTVSpeedContent = new GUIContent("     >TV Noise Speed", "Speed of animation");
    GUIContent noiseTVWidthContent = new GUIContent("     >TV Noise Width", "Width of effect");
    GUIContent noiseTVAngleContent = new GUIContent("     >TV Noise Vertical/Horizontal", "Changes whether noise pulse animates across the screen or top to bottom");

    GUIContent sharpenContent = new GUIContent("Use Sharpen", "Applys an edge-detecting sharpen filter");
    GUIContent sharpenIntensityContent = new GUIContent("   >Sharpen Intensity", "Increases the intensity of the Sharpen (default: 0.4)");

    GUIContent brightContent = new GUIContent("Use Brightness", "Performs a nice brightness amplification to some parts of the image");
    GUIContent brightIntensityContent = new GUIContent("   >Brightness Intensity", "Increases the intensity of the effect");
    GUIContent brightCutoffContent = new GUIContent("   >Brightness Cutoff", "The cutoff point of the brightness curve");
    GUIContent brightSoftnessContent = new GUIContent("   >Lightness Slope", "The slope of the brightness curve");

    GUIContent tonemapContent = new GUIContent("Use Tonemap", "Transforms color from Linear into Gamma color space");
    GUIContent tonemapTypeContent = new GUIContent("   >Tonemap Type", "Filmic = Advanced filmic tonemap, only really affects HDR colors. RomB = similar, but with very nice deep colors");
    GUIContent tonemapParamsContent = new GUIContent("   >Tonemap Parameters", "Depending on what type of tonemapping you are using, these change different points in the tonemapping algorithm. In most cases, you shouldn't need to change them");
    GUIContent tonemapSecondaryParamsContent = new GUIContent("   >Secondary Tonemap Params", "Depending on what type of tonemapping you are using, these change different points in the tonemapping algorithm. In most cases, you shouldn't need to change them");
    //	GUIContent tonemapShowParamsContent = new GUIContent("   >Show Raw Values", "Depending on what type of tonemapping you are using, these change different points in the tonemapping algorithm. In most cases, you shouldn't need to change them");

    GUIContent gammaCorrectionContent = new GUIContent("Use Gamma Correction", "Applies gamma correction to the final color of the image. Some monitors have slightly different base gamma values, so the gamma value should normally be user-set");
    GUIContent gammaValueContent = new GUIContent("   >Gamma Value", "Some monitors have slightly different base gamma values, so the gamma value should normally be user-set");

    GUIContent exposureAdaptContent = new GUIContent("Use Exposure Adaptation", "Simulates the ambient light response as human eyes adjust when going from a bright environment into a dark environment or vice versa");
    GUIContent exposureSpeedContent = new GUIContent("   >Exposure Speed", "The normalized delta-speed at which the 'eye' texture adapts towards the new value");
    //GUIContent exposureMultContent = new GUIContent("   >Exposure Multiplier", "Increases the intensity of the effect");
    GUIContent exposureOffsetContent = new GUIContent("   >Exposure Offset", "Changes the 'default' exposure value of the 'eye' by applying an offset");
    GUIContent exposureMinContent = new GUIContent("   >Minimum Exposure", "The minimum value that exposure will be clamped to (not an actual camera exposure value)");
    GUIContent exposureMatContent = new GUIContent("   >Maximum Exposure", "The maximum value that exposure will be clamped to (not an actual camera exposure value)");
    GUIContent exposureDebugContent = new GUIContent("   >View adaptation texture", "Render the exposure texture to half the screen, to visualise how fast it changes");

    GUIContent lutContent = new GUIContent("Use Color Correction LUT", "Replaces the standard colour palette with a new palette based on a lookup texture");
    GUIContent lutInfoContent = new GUIContent("   >Lookup Texture", "Texture to base the 3D Lookup texture on");
    GUIContent lutSecondaryInfoContent = new GUIContent("   >Use Secondary LUT", "Allows a secondary LUT texture to be used");

    GUIContent fogContent = new GUIContent("Use Fog", "Simulates a fog effect, which increases in intensity over distance, occluding distant objects");
    //GUIContent fogIntensityContent = new GUIContent("   >Fog Intensity", "Maximum thickness of the fog effect");
    GUIContent fogColorContent = new GUIContent("   >Fog Start Color", "Color of the fog closest to the camera");
    //GUIContent fogStartPointContent = new GUIContent ("   >Fog Start Point", "Distance away from the camera at which fog starts to occur");
    //GUIContent fogEndPointContent = new GUIContent ("   >Fog End Point", "Distance away from the camera at which the end color begins to blend in");
    GUIContent fogTypeContent = new GUIContent("   >Fog Type", "The method used to fade the fog in. ");
    public GUIContent fogNoiseContent = new GUIContent("   >Fog Noise Intensity", "Strength of the 'wind'/noise in the fog");
    GUIContent fogDistanceContent = new GUIContent("   >Fog Distance", "Range in units that the fog takes to reach maximum intensity");
    GUIContent fogAffectSkyboxContent = new GUIContent("   >Apply to Skybox", "Applies the fog regardless of depth");

    GUIContent aoContent = new GUIContent("Use Ambient Obscurance", "Applies a screen-space ambient obscurance effect which approximates basic global illumination");
    GUIContent aoIntensityContent = new GUIContent("   >AO Intensity", "Darkness of the AO effect");
    GUIContent aoBlurPassesContent = new GUIContent("   >AO Blur Passes", "Number of bilateral blur passes performed on the AO texture. Moderate performance impact.");
    //GUIContent aoBlurFilterDistanceContent = new GUIContent("   >AO Blur Filter Distance", "Scales the distance of the AO blur effect, resulting in a larger/smaller blur");
    GUIContent aoDownsampleContent = new GUIContent("   >Use AO Downsample", "Perform all AO calculations at half resolution, saving performance.");
    GUIContent aoRadiusContent = new GUIContent("   >AO Radius", "The radius that the AO effect samples around geometry for occlusion");
    GUIContent aoDepthTypeContent = new GUIContent("   >AO Depth Method", "What texture the AO should grab Depth from (Ideally: Gbuffer (if using deferred), then Depth, then Depthnormals)");
    public GUIContent aoBiasContent = new GUIContent("     >AO Bias", "Increases the distance required between geometry to cause AO. Useful for fixing artifacts.");
    public GUIContent aoContrastContent = new GUIContent("     >AO Contrast", "Determines the contrast of the occlusion. Changing this will result in AO either underoccluding or overoccluding, but may be desired in some nonrealistic (eg lowpoly) scenes");
    public GUIContent aoLightingContent = new GUIContent("     >AO Lighting Contribution", "Weights the AO based on the base lightness of the pixel, so that brighter pixels (like those lit by direct light) don't get AO applied");
    public GUIContent aoDebugContent = new GUIContent("   >View AO Texture", "Debug view of the AO-only texture");

    public GUIContent aoUseCutoffContent = new GUIContent("     >Use AO Cutoff", "Fade the AO effect out over distance");
    public GUIContent aoAdvancedContent = new GUIContent("   Show Advanced Values", "Show advanced variables of the AO effect");
    public GUIContent aoCutoffStartContent = new GUIContent("       >AO Cutoff Start", "Start fading the AO out at this distance from the camera (Units)");
    public GUIContent aoCutoffRangeContent = new GUIContent("       >AO Cutoff Range", "Fade the AO effect out over this range (Units)");

    public GUIContent aoBlurTypeContent = new GUIContent("     >AO Blur type", "Changes the type of Bilateral blur the AO uses. Fast = ~15% faster per pass than Wide, but Wide performs a 'blurrier' blur");
    public GUIContent aoSampleQualityContent = new GUIContent("   >AO Sample Amount", "Takes more samples per pixel as increases. Can have a large impact on performance, particularly when not downsampling");

    /*GUIContent raysContent = new GUIContent ("Use Godrays", "Simulates fake light scattering (Crepuscular Rays) when looking at a ray caster");
	GUIContent raysWeightContent = new GUIContent ("   >Rays Weight", "Weight of the Rays effect. Around 0.5-0.6 is usually a good value");
	GUIContent raysColorContent = new GUIContent ("   >Rays Color", "Main Color of the rays cast");
	GUIContent raysThresholdContent = new GUIContent ("   >Rays Threshold", "Colors darker than this will be excluded from the blurring pass of the rays effect");
	GUIContent raysCasterContent = new GUIContent ("   >Ray Caster", "The transform that the rays should come from (Usuall a directional light)");
	GUIContent raysDebugContent = new GUIContent ("   >Show Rays Texture", "Renders a debug view of the rays texture");
	public GUIContent raysAdvancedContent = new GUIContent ("   Show Advanced Values", "Show advanced variables of the godrays effect");*/
    public GUIContent rayDecayContent = new GUIContent("       >Ray Decay", "The amount at which the rays intensity decrease as they get further away from the sun");
    public GUIContent rayExposureContent = new GUIContent("       >Ray Exposure", "The exposure of the sun while grabbing the rays texture - helps with blending (default 0.2)");
    public GUIContent rayDensityContent = new GUIContent("       >Ray Density", "How close the samples of each ray are taken together");
    public GUIContent rayDownsampleContent = new GUIContent("       >Ray Downsample", "Quarter is recommended for most games, but for slightly sharper rays, use half");

    GUIContent colorifyContent = new GUIContent("Use Color Enhance", "Enhances the colors of pixels");
    GUIContent colorifyAmountContent = new GUIContent("   >Color Intensity", "Amount of color enhancement to apply (default 0.4)");
    int lastframecount = 0;
    /*
    static bool hasDisplayedTonemapWarning = false;
    static bool hasDisplacedHDRWarning = false;
    static bool hasDisplayedGBufferWarning = false;*/
    #endregion

    public override void OnEnable()
    {
        //LerpVolumeWeight.v = this;
        serObj = new SerializedObject(target);
        
        var prism = new PropertyFetcher<PRISMEffects>(serializedObject);


        knobOptions = new GUILayoutOption[]
        {
                GUILayout.Height(48),
                GUILayout.MaxWidth(50),
            //add more layout options
        };


        //var prismset = new PropertyModification
        m_Intensity = Unpack(prism.Find(x => x.intensity));
        showInSceneView = Unpack(prism.Find(x => x.showInSceneView));
        colorTemperature = Unpack(prism.Find(x => x.colorTemperature));
        colorTint = Unpack(prism.Find(x => x.colorTint));
        exposure = Unpack(prism.Find(x => x.exposure));
        useColorCorrection = Unpack(prism.Find(x => x.useColorCorrection));
        colorVibrance = Unpack(prism.Find(x => x.colorVibrance));
        contrast = Unpack(prism.Find(x => x.contrast));
        lightness = Unpack(prism.Find(x => x.lightness));
        labA_GreenAndMagenta = Unpack(prism.Find(x => x.labA_GreenAndMagenta));
        labB_YellowAndBlue = Unpack(prism.Find(x => x.labB_YellowAndBlue));
        VignetteStrength = Unpack(prism.Find(x => x.vignetteStrength));
        useLensDistortion = Unpack(prism.Find(x => x.useLensDistortion));

        shadows = Unpack(prism.Find(x => x.shadows));
        midtones = Unpack(prism.Find(x => x.midtones));
        highlights = Unpack(prism.Find(x => x.highlights));

        useChromaticAberration = Unpack(prism.Find(x => x.useChromaticAberration));
        glassDispersion = Unpack(prism.Find(x => x.glassDispersion));
        petzvalDistortionValue = Unpack(prism.Find(x => x.petzvalDistortionValue));
        chromaticColorWeights = Unpack(prism.Find(x => x.chromaticColorWeights));
        barrelDistortion = Unpack(prism.Find(x => x.barrelDistortion));

        useFilmicNoise = Unpack(prism.Find(x => x.useFilmicNoise));
        sensorNoise = Unpack(prism.Find(x => x.sensorNoise));
        sensorPixelSize = Unpack(prism.Find(x => x.sensorPixelSize));
        useDither = Unpack(prism.Find(x => x.useDither));
        ditherBitDepth = Unpack(prism.Find(x => x.ditherBitDepth));
        noiseBlueSaturation = Unpack(prism.Find(x => x.noiseBlueSaturation));
        noiseGreenSaturation = Unpack(prism.Find(x => x.noiseGreenSaturation));
        noiseRedSaturation = Unpack(prism.Find(x => x.noiseRedSaturation));
        useTVNoise = Unpack(prism.Find(x => x.useTVNoise));
        noiseTVSpeed = Unpack(prism.Find(x => x.noiseTVSpeed));
        noiseTVWidth = Unpack(prism.Find(x => x.noiseTVWidth));
        noiseTVAngle = Unpack(prism.Find(x => x.noiseTVAngle));


        useLut = Unpack(prism.Find(x => x.useLut));
        useCubeLUTs = Unpack(prism.Find(x => x.useCubeLUTs));
        twoDLookupTex = Unpack(prism.Find(x => x.twoDLookupTex));
        lutLerpAmount = Unpack(prism.Find(x => x.lutLerpAmount));
        useSecondLut = Unpack(prism.Find(x => x.useSecondLut));
        secondaryTwoDLookupTex = Unpack(prism.Find(x => x.secondaryTwoDLookupTex));
        secondaryLutLerpAmount = Unpack(prism.Find(x => x.secondaryLutLerpAmount));

        tonemap = Unpack(prism.Find(x => x.tonemap));
        gammaOffset = Unpack(prism.Find(x => x.gammaOffset));

        useBloom = Unpack(prism.Find(x => x.useBloom));
        bloomStrength = Unpack(prism.Find(x => x.bloomStrength));
        bloomThreshold = Unpack(prism.Find(x => x.bloomThreshold));
        bloomRadius = Unpack(prism.Find(x => x.bloomRadius));
        useLensDirt = Unpack(prism.Find(x => x.useLensDirt));
        bloomDirtTexture = Unpack(prism.Find(x => x.bloomDirtTexture));
        lensDirtExposure = Unpack(prism.Find(x => x.lensDirtExposure));
        increaseBloomClarity = Unpack(prism.Find(x => x.increaseBloomClarity));

        useLinearSpace = Unpack(prism.Find(x => x.useLinearSpace));

        useHQBloom = Unpack(prism.Find(x => x.useHighQualityBloom));
        bloomSaturation = Unpack(prism.Find(x => x.bloomSaturation));
        useBloomStab = Unpack(prism.Find(x => x.useBloomStability));

    }

    public static float SnapTo(float a, float snap)
    {
        return Mathf.Round(a / snap) * snap;
    }

    public override void OnInspectorGUI()
    {
        m_Intensity.overrideState.boolValue = true;
        showInSceneView.overrideState.boolValue = true;
        colorTemperature.overrideState.boolValue = true;
        colorTint.overrideState.boolValue = true;
        exposure.overrideState.boolValue = true;
        useColorCorrection.overrideState.boolValue = true;
        colorVibrance.overrideState.boolValue = true;
        contrast.overrideState.boolValue = true;
        lightness.overrideState.boolValue = true;
        labA_GreenAndMagenta.overrideState.boolValue = true;
        labB_YellowAndBlue.overrideState.boolValue = true;
        VignetteStrength.overrideState.boolValue = true;
        useLensDistortion.overrideState.boolValue = true;

        useChromaticAberration.overrideState.boolValue = true;
        glassDispersion.overrideState.boolValue = true;
        petzvalDistortionValue.overrideState.boolValue = true;
        chromaticColorWeights.overrideState.boolValue = true;
        barrelDistortion.overrideState.boolValue = true;

        sensorNoise.overrideState.boolValue = true;
        sensorPixelSize.overrideState.boolValue = true;
        ditherBitDepth.overrideState.boolValue = true;
        useDither.overrideState.boolValue = true;

        useLut.overrideState.boolValue = true;
        twoDLookupTex.overrideState.boolValue = true;
        lutLerpAmount.overrideState.boolValue = true;
        useSecondLut.overrideState.boolValue = true;
        secondaryTwoDLookupTex.overrideState.boolValue = true;
        secondaryLutLerpAmount.overrideState.boolValue = true;
        target.SetAllOverridesTo(true);
        //PropertyField(m_Intensity);
        //PropertyField(useColorCorrection);
        //PropertyField(colorTemperature);

        //GUILayout.Label(editorTex);
        byte[] b64_bytes = System.Convert.FromBase64String(editorTextureStringb64);

        editorTex = new Texture2D(562, 32);
        editorTex.LoadImage(b64_bytes);
        GUILayout.Label(editorTex);

        EditorGUILayout.BeginHorizontal();
        bool shouldUsePRISM = m_Intensity.value.floatValue == 1f ? true : false;
        shouldUsePRISM = EditorGUILayout.Toggle("Use Cinematic URP Post-Processing - PRISM", shouldUsePRISM);
        if(shouldUsePRISM == true)
        {
            m_Intensity.value.floatValue = 1f;
        } else
        {
            m_Intensity.value.floatValue = 0f;
        }
        EditorGUILayout.EndHorizontal();


        showInSceneView.value.boolValue = EditorGUILayout.Toggle("Show in scene view", showInSceneView.value.boolValue);

        //EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Current effects: ");
        //EditorGUILayout.LabelField(new GUIContent(currentEffectsString, currentEffectsString));

        EditorGUILayout.BeginHorizontal();
       //useLinearSpace.value.boolValue = EditorGUILayout.Toggle(linearSpaceContent, useLinearSpace.value.boolValue);
        //Debug.LogError(useColorCorrection.overrideState.type.ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        useColorCorrection.value.boolValue = EditorGUILayout.Toggle(colorContent, useColorCorrection.value.boolValue);
        //Debug.LogError(useColorCorrection.overrideState.type.ToString());
        EditorGUILayout.EndHorizontal();


        if (useColorCorrection.value.boolValue == true)
        {
            EditorGUI.BeginChangeCheck();
            float colorTemp = EditorGUILayout.Slider(colorTempContent, colorTemperature.value.floatValue, 1000f, 20000f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Temperature");
                colorTemperature.value.floatValue = colorTemp;
            }

            //DrawOverrideCheckbox(colorTint);

            EditorGUI.BeginChangeCheck();
            float colTint = EditorGUILayout.Slider(colorTintContent, colorTint.value.floatValue, -1f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Tint");
                colorTint.value.floatValue = colTint;
            }

            EditorGUI.BeginChangeCheck();
            float colorExp = EditorGUILayout.Slider(exposureContent, exposure.value.floatValue, -5f, 5f);
            colorExp = SnapTo(colorExp, 0.5f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Exposure");
                exposure.value.floatValue = colorExp;
            }

            EditorGUI.BeginChangeCheck();
            float colorVib = EditorGUILayout.Slider(colorVibranceContent, colorVibrance.value.floatValue, -0.2f, 0.2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Vibrance");
                colorVibrance.value.floatValue = colorVib;
            }

            EditorGUI.BeginChangeCheck();
            float colorContrast = EditorGUILayout.Slider(contrastContent, contrast.value.floatValue, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Contrast");
                contrast.value.floatValue = colorContrast;
            }

            EditorGUI.BeginChangeCheck();
            float lab_a = EditorGUILayout.Slider(labA_GreenAndMagentaContent, labA_GreenAndMagenta.value.floatValue, -0.5f, 0.5f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Lab A");
                labA_GreenAndMagenta.value.floatValue = lab_a;
            }

            EditorGUI.BeginChangeCheck();
            float lab_b = EditorGUILayout.Slider(labB_YellowAndBlueContent, labB_YellowAndBlue.value.floatValue, -0.5f, 0.5f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Lab B");
                labB_YellowAndBlue.value.floatValue = lab_b;
            }

            EditorGUI.BeginChangeCheck();
            float light = EditorGUILayout.Slider(lightnessContent, lightness.value.vector2Value.y, 0f, 2f);
            float slope = EditorGUILayout.Slider(brightSoftnessContent, lightness.value.vector2Value.x, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Lightness");
                lightness.value.vector2Value = new Vector2(slope, light);
            }

            Vector2 knobSize = new Vector2(32f, 32f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   >Shadows (R,G,B, Lift)");

            EditorGUI.BeginChangeCheck();
            float shadowsR = shadows.value.vector4Value.x;
            Color redShadowCol = Color.Lerp(Color.black, Color.red, shadowsR);
            shadowsR = EditorGUILayout.Knob(knobSize, shadowsR, 0f, 1f, "", Color.black, redShadowCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shadows Red");
                shadows.value.vector4Value = new Vector4(shadowsR, shadows.value.vector4Value.y, shadows.value.vector4Value.z, shadows.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float shadowsG = shadows.value.vector4Value.y;
            Color greenShadowCol = Color.Lerp(Color.black, Color.green, shadowsG);
            shadowsG = EditorGUILayout.Knob(knobSize, shadowsG, 0f, 1f, "", Color.black, greenShadowCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shadows Green");
                shadows.value.vector4Value = new Vector4(shadows.value.vector4Value.x, shadowsG, shadows.value.vector4Value.z, shadows.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float shadowsB = shadows.value.vector4Value.z;
            Color blueShadowCol = Color.Lerp(Color.black, Color.blue, shadowsB);
            shadowsB = EditorGUILayout.Knob(knobSize, shadowsB, 0f, 1f, "", Color.black, blueShadowCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shadows Blue");
                shadows.value.vector4Value = new Vector4(shadows.value.vector4Value.x, shadows.value.vector4Value.y, shadowsB, shadows.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float shadowsW = shadows.value.vector4Value.w;
            Color whiteShadowCol = new Color(shadowsR, shadowsG, shadowsB);// Color.Lerp(Color.black, Color.white, shadowsW);
            shadowsW = EditorGUILayout.Knob(knobSize, shadowsW, -0.1f, 0.1f, "", Color.black, whiteShadowCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Shadows White");
                shadows.value.vector4Value = new Vector4(shadows.value.vector4Value.x, shadows.value.vector4Value.y, shadows.value.vector4Value.z, shadowsW);
            }
            EditorGUILayout.EndHorizontal();

            // MIDS 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   >Midtones (R,G,B, Lift)");

            EditorGUI.BeginChangeCheck();
            float midtonesR = midtones.value.vector4Value.x;
            Color redMidCol = Color.Lerp(Color.black, Color.red, midtonesR);
            midtonesR = EditorGUILayout.Knob(knobSize, midtonesR, 0f, 1f, "", Color.black, redMidCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "midtones Red");
                midtones.value.vector4Value = new Vector4(midtonesR, midtones.value.vector4Value.y, midtones.value.vector4Value.z, midtones.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float midtonesG = midtones.value.vector4Value.y;
            Color greenMidCol = Color.Lerp(Color.black, Color.green, midtonesG);
            midtonesG = EditorGUILayout.Knob(knobSize, midtonesG, 0f, 1f, "", Color.black, greenMidCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "midtones Green");
                midtones.value.vector4Value = new Vector4(midtones.value.vector4Value.x, midtonesG, midtones.value.vector4Value.z, midtones.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float midtonesB = midtones.value.vector4Value.z;
            Color blueMidCol = Color.Lerp(Color.black, Color.blue, midtonesB);
            midtonesB = EditorGUILayout.Knob(knobSize, midtonesB, 0f, 1f, "", Color.black, blueMidCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "midtones Blue");
                midtones.value.vector4Value = new Vector4(midtones.value.vector4Value.x, midtones.value.vector4Value.y, midtonesB, midtones.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float midtonesW = midtones.value.vector4Value.w;
            //Color whiteMidCol = Color.Lerp(Color.black, Color.white, midtonesW);
            Color whiteMidCol = new Color(midtonesR, midtonesG, midtonesB);// Color.Lerp(Color.black, Color.white, midtonesW);
            midtonesW = EditorGUILayout.Knob(knobSize, midtonesW, -0.1f, 0.1f, "", Color.black, whiteMidCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "midtones White");
                midtones.value.vector4Value = new Vector4(midtones.value.vector4Value.x, midtones.value.vector4Value.y, midtones.value.vector4Value.z, midtonesW);
            }
            EditorGUILayout.EndHorizontal();


            // HIGHLIGHTS 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("   >Highlights (R,G,B, Lift)");

            EditorGUI.BeginChangeCheck();
            float highlightsR = highlights.value.vector4Value.x;
            Color redHighCol = Color.Lerp(Color.black, Color.red, highlightsR);
            highlightsR = EditorGUILayout.Knob(knobSize, highlightsR, 0f, 1f, "", Color.black, redHighCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "highlights Red");
                highlights.value.vector4Value = new Vector4(highlightsR, highlights.value.vector4Value.y, highlights.value.vector4Value.z, highlights.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float highlightsG = highlights.value.vector4Value.y;
            Color greenHighCol = Color.Lerp(Color.black, Color.green, highlightsG);
            highlightsG = EditorGUILayout.Knob(knobSize, highlightsG, 0f, 1f, "", Color.black, greenHighCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "highlights Green");
                highlights.value.vector4Value = new Vector4(highlights.value.vector4Value.x, highlightsG, highlights.value.vector4Value.z, highlights.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float highlightsB = highlights.value.vector4Value.z;
            Color blueHighCol = Color.Lerp(Color.black, Color.blue, highlightsB);
            highlightsB = EditorGUILayout.Knob(knobSize, highlightsB, 0f, 1f, "", Color.black, blueHighCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "highlights Blue");
                highlights.value.vector4Value = new Vector4(highlights.value.vector4Value.x, highlights.value.vector4Value.y, highlightsB, highlights.value.vector4Value.w);
            }

            EditorGUI.BeginChangeCheck();
            float highlightsW = highlights.value.vector4Value.w;
            //Color whiteMidCol = Color.Lerp(Color.black, Color.white, highlightsW);
            Color whiteHighCol = new Color(highlightsR, highlightsG, highlightsB);// Color.Lerp(Color.black, Color.white, highlightsW);
            highlightsW = EditorGUILayout.Knob(knobSize, highlightsW, -0.1f, 0.1f, "", Color.black, whiteHighCol, true, knobOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "highlights White");
                highlights.value.vector4Value = new Vector4(highlights.value.vector4Value.x, highlights.value.vector4Value.y, highlights.value.vector4Value.z, highlightsW);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        useBloom.value.boolValue = EditorGUILayout.Toggle(bloomContent, useBloom.value.boolValue);
        //Debug.LogError(useColorCorrection.overrideState.type.ToString());
        EditorGUILayout.EndHorizontal();

        if(useBloom.value.boolValue == true)
        {

            EditorGUILayout.BeginHorizontal();
            useHDRBloomIntensity = EditorGUILayout.Toggle(bloomHDRSliderContent, useHDRBloomIntensity);
            EditorGUILayout.EndHorizontal();

            float maxBloom = 5f;
            if (useHDRBloomIntensity) maxBloom = 0.2f;
            
            EditorGUI.BeginChangeCheck();
            float bloomStr = EditorGUILayout.Slider(bloomIntensityContent, bloomStrength.value.floatValue, 0f, maxBloom);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Bloom Strength");
                bloomStrength.value.floatValue = bloomStr;
            }

            EditorGUI.BeginChangeCheck();
            float bloomThresh = EditorGUILayout.Slider(bloomThresholdContent, bloomThreshold.value.floatValue, 0f, 3f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Bloom Threshold");
                bloomThreshold.value.floatValue = bloomThresh;
            }

            EditorGUILayout.BeginHorizontal();
            increaseBloomClarity.value.boolValue = EditorGUILayout.Toggle(bloomClarityContent, increaseBloomClarity.value.boolValue);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            float bloomRad = (float)EditorGUILayout.Slider(bloomRadContent, bloomRadius.value.floatValue, 2f, 4f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Bloom Radius");
                bloomRadius.value.floatValue = bloomRad;
            }

            EditorGUILayout.BeginHorizontal();
            useHQBloom.value.boolValue = EditorGUILayout.Toggle(bloomHQContent, useHQBloom.value.boolValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            useBloomStab.value.boolValue = EditorGUILayout.Toggle(bloomStabilityContent, useBloomStab.value.boolValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            useLensDirt.value.boolValue = EditorGUILayout.Toggle(bloomLensDirtContent, useLensDirt.value.boolValue);
            EditorGUILayout.EndHorizontal();

            if(useLensDirt.value.boolValue == true)
            {
                EditorGUI.BeginChangeCheck();
                float bloomdirtXp = EditorGUILayout.Slider(bloomLensDirtIntensityContent, lensDirtExposure.value.floatValue, 0f, 20f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Bloom dirt xp");
                    lensDirtExposure.value.floatValue = bloomdirtXp;
                }

                PropertyField(bloomDirtTexture);
            }
        }


        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        useLensDistortion.value.boolValue = EditorGUILayout.Toggle(lensDistortContent, useLensDistortion.value.boolValue);
        EditorGUILayout.EndHorizontal();
        if (useLensDistortion.value.boolValue == true)
        {
            EditorGUI.BeginChangeCheck();
            float vigStr = EditorGUILayout.Slider(vignetteStrengthContent, VignetteStrength.value.floatValue, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Vignette Strength");
                VignetteStrength.value.floatValue = vigStr;
            }

            EditorGUI.BeginChangeCheck();
            float fringe = EditorGUILayout.Slider(glassDispersionContent, glassDispersion.value.floatValue, 0f, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Glass Dispersion");
                glassDispersion.value.floatValue = fringe;
            }

            EditorGUI.BeginChangeCheck();
           float distort = EditorGUILayout.Slider(lensDistortPincushionContent, barrelDistortion.value.floatValue, -1f, 0.25f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Lens distortion");
                barrelDistortion.value.floatValue = distort;
            }

            EditorGUI.BeginChangeCheck();
            float petzVal = EditorGUILayout.Slider(petzvalDispersionContent, petzvalDistortionValue.value.floatValue, -.5f, 0.2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "PetzvalDistortion");
                petzvalDistortionValue.value.floatValue = petzVal;
            }

            Vector2 knobSize = new Vector2(32f,32f);

            EditorGUILayout.BeginHorizontal();
            useChromaticAberration.value.boolValue = EditorGUILayout.Toggle(useChromabContent, useChromaticAberration.value.boolValue);
            EditorGUILayout.EndHorizontal();

            if(useChromaticAberration.value.boolValue == true)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   >Chromatic Aberrations          ");

                EditorGUI.BeginChangeCheck();
                float chromWeightR = chromaticColorWeights.value.vector3Value.x;
                chromWeightR = EditorGUILayout.Knob(knobSize, chromWeightR, 0f, 0.2f, "", Color.black, Color.red, true, knobOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Chromatic Color Weight r");
                    chromaticColorWeights.value.vector3Value = new Vector3(chromWeightR, chromaticColorWeights.value.vector3Value.y, chromaticColorWeights.value.vector3Value.z);
                }

                EditorGUI.BeginChangeCheck();
                float chromWeightG = chromaticColorWeights.value.vector3Value.y;
                chromWeightG = EditorGUILayout.Knob(knobSize, chromWeightG, 0f, 0.2f, "", Color.black, Color.green, true, knobOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Chromatic Color Weight g");
                    chromaticColorWeights.value.vector3Value = new Vector3(chromaticColorWeights.value.vector3Value.x, chromWeightG, chromaticColorWeights.value.vector3Value.z);
                }
                EditorGUILayout.EndHorizontal();
            }

        }


        //NOISE

        EditorGUILayout.BeginHorizontal();
        useFilmicNoise.value.boolValue = EditorGUILayout.Toggle(noiseContent, useFilmicNoise.value.boolValue);
        EditorGUILayout.EndHorizontal();

        if(useFilmicNoise.value.boolValue == true)
        {
            EditorGUI.BeginChangeCheck();
            int noiseVal = EditorGUILayout.IntSlider(noiseIntensityContent, (int)sensorNoise.value.intValue, 0,15);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Sensor Noise");
                sensorNoise.value.intValue = noiseVal;
            }

            EditorGUI.BeginChangeCheck();
            float noiseRed = EditorGUILayout.Slider(noiseRedSaturationContent, noiseRedSaturation.value.floatValue, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Noise red");
                noiseRedSaturation.value.floatValue = noiseRed;
            }

            EditorGUI.BeginChangeCheck();
            float noiseGreen = EditorGUILayout.Slider(noiseGreenSaturationContent, noiseGreenSaturation.value.floatValue, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Noise G");
                noiseGreenSaturation.value.floatValue = noiseGreen;
            }

            EditorGUI.BeginChangeCheck();
            float noiseBlue = EditorGUILayout.Slider(noiseBlueSaturationContent, noiseBlueSaturation.value.floatValue, 0f, 2f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Noise b");
                noiseBlueSaturation.value.floatValue = noiseBlue;
            }

            EditorGUI.BeginChangeCheck();
            float pixelsize = EditorGUILayout.Slider(noiseSizeContent, sensorPixelSize.value.floatValue, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Sensor Pixel size");
                sensorPixelSize.value.floatValue = pixelsize;
            }

            EditorGUILayout.BeginHorizontal();
            useTVNoise.value.boolValue = EditorGUILayout.Toggle(noiseTVContent, useTVNoise.value.boolValue);
            EditorGUILayout.EndHorizontal();

            if(useTVNoise.value.boolValue)
            {

                EditorGUI.BeginChangeCheck();
                float anoiseTVSpeed = EditorGUILayout.Slider(noiseTVSpeedContent, noiseTVSpeed.value.floatValue, -10f, 10f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Noise TV");
                    noiseTVSpeed.value.floatValue = anoiseTVSpeed;
                }

                EditorGUI.BeginChangeCheck();
                float anoiseTVWidth = EditorGUILayout.Slider(noiseTVWidthContent, noiseTVWidth.value.floatValue, 0.0001f, 0.2f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Noise TV");
                    noiseTVWidth.value.floatValue = anoiseTVWidth;
                }

                EditorGUI.BeginChangeCheck();
                float anoiseTVAngle = (float)EditorGUILayout.IntSlider(noiseTVAngleContent, (int)noiseTVAngle.value.floatValue, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Noise TV");
                    noiseTVAngle.value.floatValue = anoiseTVAngle;
                }
            }

        }

        EditorGUILayout.BeginHorizontal();
        useDither.value.boolValue = EditorGUILayout.Toggle(useDitherContent, useDither.value.boolValue);
        EditorGUILayout.EndHorizontal();

        if (useDither.value.boolValue == true)
        {
            EditorGUI.BeginChangeCheck();
            int ditherDepth = EditorGUILayout.IntSlider(ditherBitDepthContent, (int)ditherBitDepth.value.intValue, 2, 10);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Dither bit depth");
                ditherBitDepth.value.intValue = ditherDepth;
            }
        }

        //Lut
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        bool useLt = EditorGUILayout.Toggle(lutContent, useLut.value.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Lut");
            useLut.value.boolValue = useLt;
        }

        EditorGUILayout.EndHorizontal();

        //if (useLut.value.boolValue)
        //showLut = EditorGUILayout.Foldout(showLut, " >Show LUT Values");

        if (useLut.value.boolValue == true)
        {
            //Enable disable prism. Maybe not?
            //PropertyField(useLut);
            EditorGUI.BeginChangeCheck();

            PropertyField(twoDLookupTex);

            if (EditorGUI.EndChangeCheck())
            {
                Debug.LogWarning("Converting LUT");
            }

            if (twoDLookupTex.value.objectReferenceValue != null)
                if (GUILayout.Button("Cycle to next LUT"))
            {
                Texture2D tex = twoDLookupTex.value.objectReferenceValue as Texture2D;
                string path = AssetDatabase.GetAssetPath(tex);
                var allPaths = AssetDatabase.GetAllAssetPaths();

                List<string> cuppLUTPaths = new List<string>();
                foreach(var v in allPaths)
                {
                    if (v.Contains("CUPP_LUT"))
                    {
                        cuppLUTPaths.Add(v);
                    }
                }

                int upTo = 0;
                for(int i = 0; i < cuppLUTPaths.Count; i++)
                {
                    if(cuppLUTPaths[i].Contains(tex.name))
                    {
                        upTo = i+1;
                    }
                }

                if(upTo == cuppLUTPaths.Count)
                {
                    upTo = 0;
                }

                var newTex = (Texture2D)AssetDatabase.LoadAssetAtPath(cuppLUTPaths[upTo], typeof(Texture2D));
                twoDLookupTex.value.objectReferenceValue = newTex;
                Debug.Log("Cycled through " + cuppLUTPaths.Count + " CUPP_LUTs and reached: " + newTex.name);
            }


            PropertyField(lutLerpAmount);
            PropertyField(useSecondLut);
            PropertyField(secondaryTwoDLookupTex);

            if(secondaryTwoDLookupTex.value.objectReferenceValue != null)
            if (GUILayout.Button("Cycle to next secondary LUT"))
            {
                Texture2D tex = secondaryTwoDLookupTex.value.objectReferenceValue as Texture2D;
                string path = AssetDatabase.GetAssetPath(tex);
                var allPaths = AssetDatabase.GetAllAssetPaths();

                List<string> cuppLUTPaths = new List<string>();
                foreach (var v in allPaths)
                {
                    if (v.Contains("CUPP_LUT"))
                    {
                        cuppLUTPaths.Add(v);
                        //Debug.Log("Added " + v);
                    }
                }

                int upTo = 0;
                for (int i = 0; i < cuppLUTPaths.Count; i++)
                {
                    if (cuppLUTPaths[i].Contains(tex.name))
                    {
                        upTo = i + 1;
                    }
                }

                if (upTo == cuppLUTPaths.Count)
                {
                    upTo = 0;
                }

                var newTex = (Texture2D)AssetDatabase.LoadAssetAtPath(cuppLUTPaths[upTo], typeof(Texture2D));
                secondaryTwoDLookupTex.value.objectReferenceValue = newTex;
                Debug.LogWarning("Cycled through " + cuppLUTPaths.Count + " CUPP_LUTs and reached: " + newTex.name);
            }

            PropertyField(secondaryLutLerpAmount);
        }

        //Debug.LogError(twoDLookupTex.value.objectReferenceValue);
        // PropertyField(twoDLookupTex);



        #region old
        /*
        if (useLut.value.boolValue && showLut)
        {
            Rect r;
            Texture2D t;

            EditorGUI.BeginChangeCheck();

            tempClutTex2D = EditorGUILayout.ObjectField(lutInfoContent, tempClutTex2D, typeof(Texture2D), false) as Texture2D;
            if (tempClutTex2D == null)
            {
                t = twoDLookupTex.value.objectReferenceValue as Texture2D;
                if (t)
                    tempClutTex2D = t;
            }

            Texture2D tex = tempClutTex2D;

            if (EditorGUI.EndChangeCheck())
            {
                Debug.LogWarning("Converting");
                prism.Convert(tex);
            }

            if (prism.twoDLookupTex)
                if (tex && prism.basedOnTempTex != prism.twoDLookupTex.name)
                {
                    EditorGUILayout.Separator();
                    if (!prism.ValidDimensions(tex))
                    {
                        EditorGUILayout.HelpBox("Invalid texture dimensions!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
                    }
                    else if (!prism.enabled)
                    {

                    }
                    else if (GUILayout.Button("Convert and Apply LUT!"))
                    {
                        string path = AssetDatabase.GetAssetPath(tex);
                        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                        bool doImport = textureImporter.isReadable == false;
                        if (textureImporter.mipmapEnabled == true)
                        {
                            doImport = true;
                        }
                        //if (textureImporter.textureFormat != TextureImporterFormat.AutomaticTruecolor) {
                        //	doImport = true;
                        //}

                        if (doImport)
                        {
                            textureImporter.isReadable = true;
                            textureImporter.mipmapEnabled = false;
                            //textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }

                        prism.Convert(tex);
                    }
                }

            if (prism.threeDLookupTex != null && prism.twoDLookupTex != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(150f);
                if (GUILayout.Button("Flip LUT (fixes Amplify Color LUTs)"))
                {
                    prism.twoDLookupTex = FlipTexture(prism.twoDLookupTex);
                    prism.Reset();
                    OnEnable();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                float lutI = EditorGUILayout.Slider("   >LUT Intensity", prism.lutLerpAmount, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Lut intense");
                    prism.lutLerpAmount = lutI;
                }

                if (prism.basedOnTempTex == "")
                {
                    string pth = prism.twoDLookupTex.name;
                    prism.basedOnTempTex = pth;
                }

                EditorGUILayout.LabelField("   >Using: " + prism.basedOnTempTex);
                t = prism.twoDLookupTex;
                if (t)
                {
                    r = GUILayoutUtility.GetLastRect();
                    r = GUILayoutUtility.GetRect(r.width, 20);
                    r.x += r.width * 0.05f / 2.0f;
                    r.width *= 0.95f;
                    GUI.DrawTexture(r, t);
                    GUILayoutUtility.GetRect(r.width, 4);
                }
            }

            EditorGUI.BeginChangeCheck();
            bool secc = EditorGUILayout.Toggle(lutSecondaryInfoContent, prism.useSecondLut);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Use 2sec");
                prism.useSecondLut = secc;
            }

            if (prism.useSecondLut)
            {
                Rect r2;
                Texture2D t2;

                EditorGUI.BeginChangeCheck();

                tempSecondClutTex2D = EditorGUILayout.ObjectField(lutInfoContent, tempSecondClutTex2D, typeof(Texture2D), false) as Texture2D;
                if (tempSecondClutTex2D == null)
                {
                    t2 = prism.secondaryTwoDLookupTex;
                    if (t2)
                        tempSecondClutTex2D = t2;
                }

                Texture2D tex2 = tempSecondClutTex2D;

                if (EditorGUI.EndChangeCheck())
                {
                    Debug.LogWarning("Converting 2nd LUT");
                    prism.Convert(tex2, true);
                    prism.Reset();
                }

                if (tex2 && prism.secondaryTwoDLookupTex != null && prism.secondaryBasedOnTempTex != prism.secondaryTwoDLookupTex.name)
                {
                    EditorGUILayout.Separator();
                    if (!prism.ValidDimensions(tex2))
                    {
                        EditorGUILayout.HelpBox("Invalid texture dimensions for LUT 2!\nPick another texture or adjust dimension to e.g. 256x16.", MessageType.Warning);
                    }
                    else if (prism.enabled == false)
                    {

                    }
                    else if (GUILayout.Button("Convert and Apply LUT 2!"))
                    {
                        string path = AssetDatabase.GetAssetPath(tex2);
                        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                        bool doImport = textureImporter.isReadable == false;
                        if (textureImporter.mipmapEnabled == true)
                        {
                            doImport = true;
                        }
                        //if (textureImporter.textureFormat != TextureImporterFormat.AutomaticTruecolor) {
                        //	doImport = true;
                        //}

                        if (doImport)
                        {
                            textureImporter.isReadable = true;
                            textureImporter.mipmapEnabled = false;
                            //textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        }

                        prism.Convert(tex2, true);
                    }
                }

                if (prism.secondaryThreeDLookupTex != null && prism.secondaryTwoDLookupTex != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(150f);
                    if (GUILayout.Button("Flip LUT (fixes Amplify Color LUTs)"))
                    {
                        prism.twoDLookupTex = FlipTexture(prism.secondaryTwoDLookupTex);
                        prism.Reset();
                        OnEnable();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginChangeCheck();
                    float lutI2 = EditorGUILayout.Slider("   >Second LUT Intensity", prism.secondaryLutLerpAmount, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Lut intense");
                        prism.secondaryLutLerpAmount = lutI2;
                    }

                    if (prism.secondaryBasedOnTempTex == "")
                    {
                        string pth2 = prism.secondaryTwoDLookupTex.name;
                        prism.secondaryBasedOnTempTex = pth2;
                    }

                    EditorGUILayout.LabelField("   >Using: " + prism.secondaryBasedOnTempTex);
                    t2 = prism.secondaryTwoDLookupTex;
                    if (t2)
                    {
                        r2 = GUILayoutUtility.GetLastRect();
                        r2 = GUILayoutUtility.GetRect(r2.width, 20);
                        r2.x += r2.width * 0.05f / 2.0f;
                        r2.width *= 0.95f;
                        GUI.DrawTexture(r2, t2);
                        GUILayoutUtility.GetRect(r2.width, 4);
                    }
                }
            }
        }
        */
        #endregion


        EditorGUI.BeginChangeCheck();
        bool tmap = EditorGUILayout.Toggle(tonemapContent, tonemap.value.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Tonemap");
            tonemap.value.boolValue = tmap;
        }

        if (tmap)
        {
            EditorGUI.BeginChangeCheck();
            float gOffset = EditorGUILayout.Slider(gammaValueContent, gammaOffset.value.floatValue, -0.4f, 0.4f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Sensor Pixel size");
                gammaOffset.value.floatValue = gOffset;
            }
        }

    serializedObject.ApplyModifiedProperties();

    }

    public override int GetHashCode()
    {
        CallCycleTex();
        return base.GetHashCode();
    }

    public void CallCycleTex()
    {


        bool testmode = false;

        testmode = (Application.isPlaying && Time.frameCount > lastframecount);

        if (testmode)
        {
            //lastframecount += Time.frameCount;
            lastframecount += 10;
        }

        if(testmode)
        {

            Texture2D tex = twoDLookupTex.value.objectReferenceValue as Texture2D;
            //Debug.Log(tex.name);
            string path = AssetDatabase.GetAssetPath(tex);
            var allPaths = AssetDatabase.GetAllAssetPaths();

            List<string> cuppLUTPaths = new List<string>();
            foreach (var v in allPaths)
            {
                if (v.Contains("CUPP_LUT"))
                {
                    cuppLUTPaths.Add(v);
                    //Debug.Log("Added " + v);
                }
            }

            int upTo = 0;
            //Debug.Log("CyclcuppLUTPaths.Count);
            for (int i = 0; i < cuppLUTPaths.Count; i++)
            {
                if (cuppLUTPaths[i].Contains(tex.name))
                {
                    //Debug.Log("Got up to: " + upTo);
                    upTo = i + 1;
                }
            }

            if (upTo == cuppLUTPaths.Count)
            {
                upTo = 0;
            }

            //TextureImporter textureImporter = AssetImporter.GetAtPath(cuppLUTPaths[upTo]) as TextureImporter;
            //bool doImport = textureImporter.isReadable == false;
            var newTex = (Texture2D)AssetDatabase.LoadAssetAtPath(cuppLUTPaths[upTo], typeof(Texture2D));
            twoDLookupTex.value.objectReferenceValue = newTex;
            Debug.Log("Cycled through " + cuppLUTPaths.Count + " CUPP_LUTs and reached: " + newTex.name);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
