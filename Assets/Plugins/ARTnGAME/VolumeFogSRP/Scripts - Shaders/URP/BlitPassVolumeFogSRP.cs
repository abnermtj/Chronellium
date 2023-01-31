using System.Collections.Generic;
namespace UnityEngine.Rendering.Universal// LWRP //v1.1.8n
{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    ///
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitPassVolumeFogSRP : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {
        //v0.4  - Unity 2020.1
#if UNITY_2020_2_OR_NEWER
        public BlitVolumeFogSRP.BlitSettings settings;
        UnityEngine.Rendering.Universal.RenderTargetHandle _handle;
        public override void OnCameraSetup(CommandBuffer cmd, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            _handle.Init(settings.textureId);
            destination = (settings.destination == BlitVolumeFogSRP.Target.Color)
                ? UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget
                : _handle;

            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;
        }
#endif

        //SSMS
        #region Public Properties

        public bool enableComposite = false;
        public int downSampleAA = 1;//SSMS
        public bool enableWetnessHaze = false;

        /// Prefilter threshold (gamma-encoded)
        /// Filters out pixels under this level of brightness.
        public float thresholdGamma;
        //{
        //    get { return Mathf.Max(_threshold, 0); }
        //    set { _threshold = value; }
        //}

        /// Prefilter threshold (linearly-encoded)
        /// Filters out pixels under this level of brightness.
        public float thresholdLinear;
        //{
        //    get { return GammaToLinear(thresholdGamma); }
        //    set { _threshold = LinearToGamma(value); }
        //}

        [HideInInspector]
        [SerializeField]
        [Tooltip("Filters out pixels under this level of brightness.")]
        public float _threshold = 0f;

        /// Soft-knee coefficient
        /// Makes transition between under/over-threshold gradual.
        public float softKnee;
        //{
        //    get { return _softKnee; }
        //    set { _softKnee = value; }
        //}

        [HideInInspector]
        [SerializeField, Range(0, 1)]
        [Tooltip("Makes transition between under/over-threshold gradual.")]
        public float _softKnee = 0.5f;

        /// Bloom radius
        /// Changes extent of veiling effects in a screen
        /// resolution-independent fashion.
        public float radius;
        //{
        //    get { return _radius; }
        //    set { _radius = value; }
        //}

        [Header("Scattering")]
        [SerializeField, Range(1, 7)]
        [Tooltip("Changes extent of veiling effects\n" +
                 "in a screen resolution-independent fashion.")]
        public float _radius = 7f;

        /// Blur Weight
        /// Gives more strength to the blur texture during the combiner loop.
        public float blurWeight;
        //{
        //    get { return _blurWeight; }
        //    set { _blurWeight = value; }
        //}

        [SerializeField, Range(0.1f, 100)]
        [Tooltip("Higher number creates a softer look but artifacts are more pronounced.")] // TODO Better description.
        public float _blurWeight = 1f;

        /// Bloom intensity
        /// Blend factor of the result image.
        public float intensity;
        //{
        //    get { return Mathf.Max(_intensity, 0); }
        //    set { _intensity = value; }
        //}

        [SerializeField]
        [Tooltip("Blend factor of the result image.")]
        [Range(0, 1)]
        public float _intensity = 1f;

        /// High quality mode
        /// Controls filter quality and buffer resolution.
        public bool highQuality;
        //{
        //    get { return _highQuality; }
        //    set { _highQuality = value; }
        //}

        [SerializeField]
        [Tooltip("Controls filter quality and buffer resolution.")]
        public bool _highQuality = true;

        /// Anti-flicker filter
        /// Reduces flashing noise with an additional filter.
        [SerializeField]
        [Tooltip("Reduces flashing noise with an additional filter.")]
        public bool _antiFlicker = true;

        public bool antiFlicker;
        //{
        //    get { return _antiFlicker; }
        //    set { _antiFlicker = value; }
        //}

        /// Distribution texture
        [SerializeField]
        [Tooltip("1D gradient. Determines how the effect fades across distance.")]
        public Texture2D _fadeRamp;

        public Texture2D fadeRamp;
        //{
        //    get { return _fadeRamp; }
        //    set { _fadeRamp = value; }
        //}

        /// Blur tint
        [SerializeField]
        [Tooltip("Tints the resulting blur. ")]
        public Color _blurTint = Color.white;

        public Color blurTint;
        //{
        //    get { return _blurTint; }
        //    set { _blurTint = value; }
        //}
        #endregion

        #region Private Members

        //[SerializeField, HideInInspector]
        //Shader _shader;
        //Material _material;

        const int kMaxIterations = 16;
        RenderTexture[] _blurBuffer1 = new RenderTexture[kMaxIterations];
        RenderTexture[] _blurBuffer2 = new RenderTexture[kMaxIterations];

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
        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            //var shader = _shader ? _shader : Shader.Find("Hidden/SSMS");
            //_material = new Material(shader);
            //_material.hideFlags = HideFlags.DontSave;
            // SMSS
            if (fadeRamp == null)
            {
                //_fadeRamp = Resources.Load("Textures/nonLinear2", typeof(Texture2D)) as Texture2D;
            };
        }
        //void OnDisable()
        //{
        //DestroyImmediate(_material);
        //}
        // [ImageEffectOpaque]
        //void renderSSMS(RenderTexture source, RenderTexture destination, Material _material)
        public void renderSSMS(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {
            Material _material = blitMaterial;
            //CAMERA 
            // Calculate vectors towards frustum corners.
            Camera camera = Camera.main;
            if (isForReflections && reflectCamera != null)
            {
                // camera = reflectionc UnityEngine.Rendering.Universal.RenderingData.ca
                // ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData
                camera = reflectCamera;
            }
            if (isForReflections && isForDualCameras) //v1.9.9.7 - Ethereal v1.1.8f
            {
                //if list has members, choose 0 for 1st etc
                if (extraCameras.Count > 0 && extraCameraID >= 0 && extraCameraID < extraCameras.Count)
                {
                    camera = extraCameras[extraCameraID];
                }
            }
            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            //RENDER FINAL EFFECT
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            //var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            var format = allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9 //v LWRP //v0.7
            // Debug.Log(renderingData.cameraData.camera.allowHDR);
            //RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(context.width, context.height, 0, format);
            RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format);
            RenderTexture.active = tmpBuffer1;
            GL.ClearWithSkybox(false, camera);
            ////context.command.BlitFullscreenTriangle(context.source, tmpBuffer1);
            //Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
            Blit(cmd, source, tmpBuffer1);

            ////TEST 1
            //Blit(cmd, tmpBuffer1, source);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //return;

            var useRGBM = Application.isMobilePlatform;

            // source texture size
            var tw = rtW;// source.width;
            var th = rtH;// source.height;

            // halve the texture size for the low quality mode
            if (!_highQuality)
            {
                tw /= 2;
                th /= 2;
            }

            // blur buffer format
            var rtFormat = useRGBM ?
                RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

            // determine the iteration count
            var logh = Mathf.Log(th, 2) + _radius - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

            // update the shader properties
            var lthresh = thresholdLinear;
            _material.SetFloat("_Threshold", lthresh);

            var knee = lthresh * _softKnee + 1e-5f;
            var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
            _material.SetVector("_Curve", curve);

            var pfo = !_highQuality && _antiFlicker;
            _material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

            _material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
            _material.SetFloat("_Intensity", _intensity);

            _material.SetTexture("_FadeTex", _fadeRamp);
            _material.SetFloat("_BlurWeight", _blurWeight);
            _material.SetFloat("_Radius", _radius);
            _material.SetColor("_BlurTint", _blurTint);

            // prefilter pass
            var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
            int offset = 10;
            var pass = _antiFlicker ? 1 + offset : 0 + offset;

            ////TEST 1
            //Blit(cmd, tmpBuffer1, source, _material, radialBlurIterations);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //return;

            //_material.SetTexture("_FogTex", tmpBuffer1);// ource);//v0.1
            //Graphics.Blit(source, prefiltered, _material, pass);
            Graphics.Blit(tmpBuffer1, prefiltered, _material, pass); //v0.1

            ////TEST 2
            //Blit(cmd, prefiltered, source);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //RenderTexture.ReleaseTemporary(prefiltered);
            //RenderTexture.ReleaseTemporary(tmpBuffer1);
            //return;

            // construct a mip pyramid
            var last = prefiltered;
            for (var level = 0; level < iterations; level++)
            {
                _blurBuffer1[level] = RenderTexture.GetTemporary(
                    last.width / 2, last.height / 2, 0, rtFormat
                );

                pass = (level == 0) ? (_antiFlicker ? 3 + offset : 2 + offset) : 4 + offset;
                Graphics.Blit(last, _blurBuffer1[level], _material, pass);

                last = _blurBuffer1[level];
            }

            ////TEST 3
            //Blit(cmd, last, source);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //RenderTexture.ReleaseTemporary(prefiltered);
            //RenderTexture.ReleaseTemporary(tmpBuffer1);
            //return;

            // upsample and combine loop
            for (var level = iterations - 2; level >= 0; level--)
            {
                var basetex = _blurBuffer1[level];
                _material.SetTexture("_BaseTexA", basetex); //USE another, otherwise BUGS

                _blurBuffer2[level] = RenderTexture.GetTemporary(
                    basetex.width, basetex.height, 0, rtFormat
                );

                pass = _highQuality ? 6 + offset : 5 + offset;
                Graphics.Blit(last, _blurBuffer2[level], _material, pass);
                last = _blurBuffer2[level];
            }

            ////TEST 4
            //Blit(cmd, last, source);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //RenderTexture.ReleaseTemporary(prefiltered);
            //RenderTexture.ReleaseTemporary(tmpBuffer1);
            //return;

            ////TEST 1
            //Blit(cmd, tmpBuffer1, source);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
            //// release the temporary buffers
            //for (var i = 0; i < kMaxIterations; i++)
            //{
            //    if (_blurBuffer1[i] != null)
            //        RenderTexture.ReleaseTemporary(_blurBuffer1[i]);

            //    if (_blurBuffer2[i] != null)
            //        RenderTexture.ReleaseTemporary(_blurBuffer2[i]);

            //    _blurBuffer1[i] = null;
            //    _blurBuffer2[i] = null;
            //}
            //RenderTexture.ReleaseTemporary(prefiltered);
            //RenderTexture.ReleaseTemporary(tmpBuffer1);
            //return;

            // finish process
            _material.SetTexture("_BaseTexA", tmpBuffer1);// ource);//v0.1
            pass = _highQuality ? 8 + offset : 7 + offset;

            //v0.1
            //Graphics.Blit(last, destination, _material, pass);
            // _material.SetTexture("_FogTex", last);// ource);//v0.1
            _material.SetTexture("_MainTexA", last);// ource);//v0.1
            Blit(cmd, last, source, _material, pass);
            context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);

            // release the temporary buffers
            for (var i = 0; i < kMaxIterations; i++)
            {
                if (_blurBuffer1[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer1[i]);

                if (_blurBuffer2[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer2[i]);

                _blurBuffer1[i] = null;
                _blurBuffer2[i] = null;
            }
            RenderTexture.ReleaseTemporary(prefiltered);
            RenderTexture.ReleaseTemporary(tmpBuffer1);
        }
        #endregion
        //END SSMS

        //v0.6
        public int downSample = 1;
        public float depthDilation = 1;
        public bool enabledTemporalAA = false;
        public float TemporalResponse = 1;
        public float TemporalGain = 1;

        //v0.6a
        public bool enableBlendMode = false;
        public float controlBackAlphaPower = 1;
        public float controlCloudAlphaPower = 0.001f;
        public Vector4 controlCloudEdgeA = new Vector4(1, 1, 1, 1);

        //v1.9.9.7 - Ethereal v1.1.8f
        public List<Camera> extraCameras = new List<Camera>();
        public int extraCameraID = 0; //assign 0 for reflection camera, 1 to N for choosing from the extra cameras list

        //v1.9.9.6 - Ethereal v1.1.8e
        public bool isForDualCameras = false;

        //v1.9.9.5 - Ethereal v1.1.8
        //Add visible lights count - renderingData.cullResults.visibleLights.Length

        ////v1.9.9.4 - Ethereal 1.1.7 - Control sampling for no noise option
        //[Tooltip("Volume sampling control, noise to no noise ratio, 0 is zero noise (x), no noise sampling step length (y) & noise sampling step lengths (z,w)")]
        public Vector4 volumeSamplingControl = new Vector4(1, 1, 1, 1);

        //v1.9.9.3
        //[Tooltip("Volume Shadow control (x-unity shadow distance, y,z-shadow atten power & offset, w-)")]
        public Vector4 shadowsControl = new Vector4(500, 1, 1, 0);

        //v1.9.9.2
        public bool enableSunShafts = false;//simple screen space sun shafts

        //v1.9.9.1
        public List<Light> lightsArray = new List<Light>();

        //v1.9.9
        public Vector4 lightControlA = new Vector4(1, 1, 1, 1);
        public Vector4 lightControlB = new Vector4(1, 1, 1, 1);
        public bool controlByColor = false;
        public Light lightA;
        public Light lightB;//grab colors of the two lights to apply volume to

        //v1.7
        public int lightCount = 3;

        //v1.6
        public bool isForReflections = false;
        public Camera reflectCamera;

        public float blendVolumeLighting = 0;
        public float LightRaySamples = 8;
        public Vector4 stepsControl = new Vector4(0, 0, 1, 1);
        public Vector4 lightNoiseControl = new Vector4(0.6f, 0.75f, 1, 1);  //v1.5

        //FOG URP /////////////
        //FOG URP /////////////
        //FOG URP /////////////
        //public float blend =  0.5f;
        public Color _FogColor = Color.white / 2;
        //fog params
        public Texture2D noiseTexture;
        public float _startDistance = 30f;
        public float _fogHeight = 0.75f;
        public float _fogDensity = 1f;
        public float _cameraRoll = 0.0f;
        public Vector4 _cameraDiff = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public float _cameraTiltSign = 1;
        public float heightDensity = 1;
        public float noiseDensity = 1;
        public float noiseScale = 1;
        public float noiseThickness = 1;
        public Vector3 noiseSpeed = new Vector4(1f, 1f, 1f);
        public float occlusionDrop = 1f;
        public float occlusionExp = 1f;
        public int noise3D = 1;
        public float startDistance = 1;
        public float luminance = 1;
        public float lumFac = 1;
        public float ScatterFac = 1;
        public float TurbFac = 1;
        public float HorizFac = 1;
        public float turbidity = 1;
        public float reileigh = 1;
        public float mieCoefficient = 1;
        public float mieDirectionalG = 1;
        public float bias = 1;
        public float contrast = 1;
        public Color TintColor = new Color(1, 1, 1, 1);
        public Vector3 TintColorK = new Vector3(0, 0, 0);
        public Vector3 TintColorL = new Vector3(0, 0, 0);
        public Vector4 Sun = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public bool FogSky = true;
        public float ClearSkyFac = 1f;
        public Vector4 PointL = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        public Vector4 PointLParams = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        bool _useRadialDistance = false;
        bool _fadeToSkybox = true;

        bool allowHDR = true;// false; //v0.7
        //END FOG URP //////////////////
        //END FOG URP //////////////////
        //END FOG URP //////////////////


        //SUN SHAFTS         
        public BlitVolumeFogSRP.BlitSettings.SunShaftsResolution resolution = BlitVolumeFogSRP.BlitSettings.SunShaftsResolution.Normal;
        public BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode screenBlendMode = BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen;
        public Vector3 sunTransform = new Vector3(0f, 0f, 0f); // Transform sunTransform;
        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        public float blend = 0.5f;
                
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }
        public bool inheritFromController = true;

        public bool enableFog = true;

        public Material blitMaterial = null;
        //public Material blitMaterialFOG = null;
        public int blitShaderPassIndex = 0;
        public FilterMode filterMode { get; set; }

        private RenderTargetIdentifier source { get; set; }
        private UnityEngine.Rendering.Universal.RenderTargetHandle destination { get; set; }

        UnityEngine.Rendering.Universal.RenderTargetHandle m_TemporaryColorTexture;
        string m_ProfilerTag;


        //SUN SHAFTS
        RenderTexture lrColorB;
        UnityEngine.Rendering.Universal.RenderTargetHandle lrDepthBuffer;

        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitPassVolumeFogSRP(UnityEngine.Rendering.Universal.RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag, BlitVolumeFogSRP.BlitSettings settings)
        {
            this.enableFog = settings.enableFog;

            this.inheritFromController = settings.inheritFromController;
            this.renderPassEvent = renderPassEvent;
            this.blitMaterial = blitMaterial;
            this.blitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");

            //SUN SHAFTS
            this.resolution = settings.resolution;
            this.screenBlendMode = settings.screenBlendMode;
            this.sunTransform = settings.sunTransform;
            this.radialBlurIterations = settings.radialBlurIterations;
            this.sunColor = settings.sunColor;
            this.sunThreshold = settings.sunThreshold;
            this.sunShaftBlurRadius = settings.sunShaftBlurRadius;
            this.sunShaftIntensity = settings.sunShaftIntensity;
            this.maxRadius = settings.maxRadius;
            this.useDepthTexture = settings.useDepthTexture;
            this.blend = settings.blend;

            ////// VOLUME FOG URP /////////////////
            //FOG URP /////////////
            //FOG URP /////////////
            //FOG URP /////////////
            //this.blend =  0.5f;
            this._FogColor = settings._FogColor;
            //fog params
            this.noiseTexture = settings.noiseTexture;
            this._startDistance = settings._startDistance;
            this._fogHeight = settings._fogHeight;
            this._fogDensity = settings._fogDensity;
            this._cameraRoll = settings._cameraRoll;
            this._cameraDiff = settings._cameraDiff;
            this._cameraTiltSign = settings._cameraTiltSign;
            this.heightDensity = settings.heightDensity;
            this.noiseDensity = settings.noiseDensity;
            this.noiseScale = settings.noiseScale;
            this.noiseThickness = settings.noiseThickness;
            this.noiseSpeed = settings.noiseSpeed;
            this.occlusionDrop = settings.occlusionDrop;
            this.occlusionExp = settings.occlusionExp;
            this.noise3D = settings.noise3D;
            this.startDistance = settings.startDistance;
            this.luminance = settings.luminance;
            this.lumFac = settings.lumFac;
            this.ScatterFac = settings.ScatterFac;
            this.TurbFac = settings.TurbFac;
            this.HorizFac = settings.HorizFac;
            this.turbidity = settings.turbidity;
            this.reileigh = settings.reileigh;
            this.mieCoefficient = settings.mieCoefficient;
            this.mieDirectionalG = settings.mieDirectionalG;
            this.bias = settings.bias;
            this.contrast = settings.contrast;
            this.TintColor = settings.TintColor;
            this.TintColorK = settings.TintColorK;
            this.TintColorL = settings.TintColorL;
            this.Sun = settings.Sun;
            this.FogSky = settings.FogSky;
            this.ClearSkyFac = settings.ClearSkyFac;
            this.PointL = settings.PointL;
            this.PointLParams = settings.PointLParams;
            this._useRadialDistance = settings._useRadialDistance;
            this._fadeToSkybox = settings._fadeToSkybox;
            //END FOG URP //////////////////
            //END FOG URP //////////////////
            //END FOG URP //////////////////
            ////// END VOLUME FOG URP /////////////////
            this.blendVolumeLighting = settings.blendVolumeLighting;
            //this.LightRaySamples = settings.LightRaySamples;
            this.isForReflections = settings.isForReflections;

            //v1.9.9.6 - Ethereal v1.1.8e
            this.isForDualCameras = settings.isForDualCameras;

            //v1.9.9.7 - Ethereal v1.1.8f
            this.extraCameraID = settings.extraCameraID;
        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Target</param>
        public void Setup(RenderTargetIdentifier source, UnityEngine.Rendering.Universal.RenderTargetHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }
        
        connectSuntoVolumeFogURP connector;

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            //grab settings if script on scene camera
            if (connector == null)
            {
                connector = renderingData.cameraData.camera.GetComponent<connectSuntoVolumeFogURP>();
                if (connector == null && Camera.main != null)
                {
                    connector = Camera.main.GetComponent<connectSuntoVolumeFogURP>();
                }
            }
            //Debug.Log(Camera.main.GetComponent<connectSuntoVolumeFogURP>().sun.transform.position);
            if (inheritFromController && connector != null)
            {
                this.enableFog = connector.enableFog;

                this.sunTransform = new Vector3(connector.Sun.x, connector.Sun.y, connector.Sun.z);// connector.sun.transform.position;
                this.screenBlendMode = connector.screenBlendMode;
                //public Vector3 sunTransform = new Vector3(0f, 0f, 0f); 
                this.radialBlurIterations = connector.radialBlurIterations;
                this.sunColor = connector.sunColor;
                this.sunThreshold = connector.sunThreshold;
                this.sunShaftBlurRadius = connector.sunShaftBlurRadius;
                this.sunShaftIntensity = connector.sunShaftIntensity;
                this.maxRadius = connector.maxRadius;
                this.useDepthTexture = connector.useDepthTexture;

                ////// VOLUME FOG URP /////////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //FOG URP /////////////
                //this.blend =  0.5f;
                this._FogColor = connector._FogColor;
                //fog params
                this.noiseTexture = connector.noiseTexture;
                this._startDistance = connector._startDistance;
                this._fogHeight = connector._fogHeight;
                this._fogDensity = connector._fogDensity;
                this._cameraRoll = connector._cameraRoll;
                this._cameraDiff = connector._cameraDiff;
                this._cameraTiltSign = connector._cameraTiltSign;
                this.heightDensity = connector.heightDensity;
                this.noiseDensity = connector.noiseDensity;
                this.noiseScale = connector.noiseScale;
                this.noiseThickness = connector.noiseThickness;
                this.noiseSpeed = connector.noiseSpeed;
                this.occlusionDrop = connector.occlusionDrop;
                this.occlusionExp = connector.occlusionExp;
                this.noise3D = connector.noise3D;
                this.startDistance = connector.startDistance;
                this.luminance = connector.luminance;
                this.lumFac = connector.lumFac;
                this.ScatterFac = connector.ScatterFac;
                this.TurbFac = connector.TurbFac;
                this.HorizFac = connector.HorizFac;
                this.turbidity = connector.turbidity;
                this.reileigh = connector.reileigh;
                this.mieCoefficient = connector.mieCoefficient;
                this.mieDirectionalG = connector.mieDirectionalG;
                this.bias = connector.bias;
                this.contrast = connector.contrast;
                this.TintColor = connector.TintColor;
                this.TintColorK = connector.TintColorK;
                this.TintColorL = connector.TintColorL;
                this.Sun = connector.Sun;
                this.FogSky = connector.FogSky;
                this.ClearSkyFac = connector.ClearSkyFac;
                this.PointL = connector.PointL;
                this.PointLParams = connector.PointLParams;
                this._useRadialDistance = connector._useRadialDistance;
                this._fadeToSkybox = connector._fadeToSkybox;

                this.allowHDR = connector.allowHDR;
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                //END FOG URP //////////////////
                ////// END VOLUME FOG URP /////////////////

                this.blendVolumeLighting = connector.blendVolumeLighting;
                this.LightRaySamples = connector.LightRaySamples;
                this.stepsControl = connector.stepsControl;
                this.lightNoiseControl = connector.lightNoiseControl;

                //v1.6
                this.reflectCamera = connector.reflectCamera;

                //v1.7
                this.lightCount = connector.lightCount;

                //v1.9.9
                this.lightControlA = connector.lightControlA;
                this.lightControlB = connector.lightControlB;
                this.controlByColor = connector.controlByColor;
                this.lightA = connector.lightA;
                this.lightB = connector.lightB;

                //v1.9.9
                this.lightsArray = connector.lightsArray;

                //v1.9.9.2
                this.enableSunShafts = connector.enableSunShafts;

                //v1.9.9.3
                this.shadowsControl = connector.shadowsControl;

                //v1.9.9.4
                this.volumeSamplingControl = connector.volumeSamplingControl;

                //v1.9.9.7 - Ethereal v1.1.8f
                this.extraCameras = connector.extraCameras;

                //v0.6
                this.downSample = connector.downSample;
                this.depthDilation = connector.depthDilation;
                this.enabledTemporalAA = connector.enabledTemporalAA;
                this.TemporalResponse = connector.TemporalResponse;
                this.TemporalGain = connector.TemporalGain;

                //v0.6a
                this.enableBlendMode = connector.enableBlendMode;
                this.controlBackAlphaPower = connector.controlBackAlphaPower;
                this.controlCloudAlphaPower = connector.controlCloudAlphaPower;
                this.controlCloudEdgeA = connector.controlCloudEdgeA;

                //SSMS
                enableComposite = connector.enableComposite;
                downSampleAA = connector.downSampleAA;
                enableWetnessHaze = connector.enableWetnessHaze;
                //SSMS
                thresholdGamma = connector._threshold;
                thresholdLinear = connector._threshold;
                _threshold = connector._threshold;
                softKnee = connector._softKnee;
                _softKnee = connector._softKnee;
                radius = connector._radius;
                _radius = connector._radius;
                blurWeight = connector._blurWeight;
                _blurWeight = connector._blurWeight;
                intensity = connector.intensity;
                _intensity = connector.intensity;
                highQuality = connector._highQuality;
                _highQuality = connector._highQuality;
                _antiFlicker = connector._antiFlicker;
                antiFlicker = connector._antiFlicker;
                _fadeRamp = connector._fadeRamp;
                fadeRamp = connector._fadeRamp;
                _blurTint = connector._blurTint;
                blurTint = connector._blurTint;
            }

            //if still null, disable effect
            bool connectorFound = true;
            if (connector == null)
            {
                connectorFound = false;
            }

            if (enableFog && connectorFound)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;

                // Can't read and write to same color target, create a temp render target to blit. 
                if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
                {
                    cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                    //RenderShafts(context, renderingData, cmd, opaqueDesc);

                    RenderFog(context, renderingData, cmd, opaqueDesc);

                    //v1.9.9.2
                    //if (enableSunShafts)
                    //{
                    //    if (connector.sun != null)
                    //    {
                    //        this.sunTransform = new Vector3(connector.sun.position.x, connector.sun.position.y, connector.sun.position.z);
                    //        RenderShafts(context, renderingData, cmd, opaqueDesc);
                    //    }
                    //    else
                    //    {
                    //        CommandBufferPool.Release(cmd);//release fog here v1.9.9.2
                    //    }
                    //}
                    //else
                    //{
                    //    CommandBufferPool.Release(cmd);//release fog here v1.9.9.2
                    //}

                    //v1.9.9.2
                    if (enableSunShafts && connector.sun != null)
                    {
                        this.sunTransform = new Vector3(connector.sun.position.x, connector.sun.position.y, connector.sun.position.z);
                        RenderShafts(context, renderingData, cmd, opaqueDesc);
                    }
                    if (enableWetnessHaze)
                    {
                        renderSSMS(context, renderingData, cmd, opaqueDesc);
                    }
                    //if ((enableSunShafts && connector.sun != null) || enableWetnessHaze)//else
                    {
                        CommandBufferPool.Release(cmd);//release fog here v1.9.9.2
                    }

                }
            }
            else
            {
                //v1.9.9.2 - if no fog
                if (enableSunShafts && connectorFound && connector.sun != null)
                {
                    CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

                    RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                    opaqueDesc.depthBufferBits = 0;

                    // Can't read and write to same color target, create a temp render target to blit. 
                    if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
                    {
                        cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);                            
                        this.sunTransform = new Vector3(connector.sun.position.x, connector.sun.position.y, connector.sun.position.z);
                        RenderShafts(context, renderingData, cmd, opaqueDesc);                         
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destination == UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);            
                cmd.ReleaseTemporaryRT(lrDepthBuffer.id);               
            }
        }    

        //SUN SHAFTS
        public void RenderShafts(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        {           
            opaqueDesc.depthBufferBits = 0;          
            Material sheetSHAFTS = blitMaterial;
           
            sheetSHAFTS.SetFloat("_Blend", blend);
            
            Camera camera = Camera.main;

            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            // we actually need to check this every frame
            if (useDepthTexture)
            {               
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }           

            Vector3 v = Vector3.one * 0.5f;
            if (sunTransform != Vector3.zero) {
                v = camera.WorldToViewportPoint(sunTransform);
            }
            else {
                v = new Vector3(0.5f, 0.5f, 0.0f);
            }
            
            //v0.1
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;

            cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));
            sheetSHAFTS.SetVector("_SunThreshold", sunThreshold);

            if (!useDepthTexture)
            {               
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, camera);
             
                sheetSHAFTS.SetTexture("_Skybox", tmpBuffer);               
                Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 3);

                RenderTexture.ReleaseTemporary(tmpBuffer);
            }
            else
            {               
                Blit(cmd, source, lrDepthBuffer.Identifier(), sheetSHAFTS, 2);
            }            

            Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
           
            radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);
            float ofs = sunShaftBlurRadius * (1.0f / 768.0f);

            sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            sheetSHAFTS.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));

            for (int it2 = 0; it2 < radialBlurIterations; it2++)
            {
                // each iteration takes 2 * 6 samples
                // we update _BlurRadius each time to cheaply get a very smooth look
                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0); 
                Blit(cmd, lrDepthBuffer.Identifier(), lrColorB, sheetSHAFTS, 1);
                cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;             
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f)); 
                cmd.GetTemporaryRT(lrDepthBuffer.id, opaqueDesc, filterMode);                
                Blit(cmd, lrColorB, lrDepthBuffer.Identifier(), sheetSHAFTS, 1);
                RenderTexture.ReleaseTemporary(lrColorB);  
                ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;              
                sheetSHAFTS.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }
            
            // put together:
            if (v.z >= 0.0f)
            {              
                sheetSHAFTS.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
            }
            else
            {
                sheetSHAFTS.SetVector("_SunColor", Vector4.zero); // no backprojection !
            }
          
            cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Identifier());          
            Blit(cmd, m_TemporaryColorTexture.Identifier(), source, sheetSHAFTS, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
           
            cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);           

            context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
           
            RenderTexture.ReleaseTemporary(lrColorB);
        }


        /////////////////////// VOLUME FOG SRP /////////////////////////////////////
        public void RenderFog(ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData, CommandBuffer cmd, RenderTextureDescriptor opaqueDesc)
        //public override void Render(PostProcessRenderContext context)
        {
            //var _material = context.propertySheets.Get(Shader.Find("Hidden/InverseProjectVFogLWRP"));
            Material _material = blitMaterial;

            //v1.9.9.5 - Ethereal v1.1.8
            //Add visible lights count - renderingData.cullResults.visibleLights.Length
            _material.SetInt("_visibleLightsCount",renderingData.cullResults.visibleLights.Length);

            _material.SetFloat("_DistanceOffset", _startDistance);
            _material.SetFloat("_Height", _fogHeight); //v0.1                                                                      
            _material.SetFloat("_cameraRoll", _cameraRoll);
            _material.SetVector("_cameraDiff", _cameraDiff);
            _material.SetFloat("_cameraTiltSign", _cameraTiltSign);

            var mode = RenderSettings.fogMode;
            if (mode == FogMode.Linear)
            {
                var start = RenderSettings.fogStartDistance;//RenderSettings.RenderfogStartDistance;
                var end = RenderSettings.fogEndDistance;
                var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
                _material.SetFloat("_LinearGrad", -invDiff);
                _material.SetFloat("_LinearOffs", end * invDiff);
                _material.DisableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else if (mode == FogMode.Exponential)
            {
                const float coeff = 1.4426950408f; // 1/ln(2)
                var density = RenderSettings.fogDensity;// RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.EnableKeyword("FOG_EXP");
                _material.DisableKeyword("FOG_EXP2");
            }
            else // FogMode.ExponentialSquared
            {
                const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
                var density = RenderSettings.fogDensity;//RenderfogDensity;
                _material.SetFloat("_Density", coeff * density * _fogDensity);
                _material.DisableKeyword("FOG_EXP");
                _material.EnableKeyword("FOG_EXP2");
            }
            if (_useRadialDistance)
                _material.EnableKeyword("RADIAL_DIST");
            else
                _material.DisableKeyword("RADIAL_DIST");

            if (_fadeToSkybox)
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);//v0.1            
            }
            else
            {
                _material.DisableKeyword("USE_SKYBOX");
                _material.SetColor("_FogColor", _FogColor);// RenderfogColor);
            }

            //v0.1 - v1.9.9.2
            //if (noiseTexture == null)
            //{
            //    noiseTexture = new Texture2D(1280, 720);
            //}
            if (_material != null && noiseTexture != null)
            {
                //if (noiseTexture == null)
                //{
                //    noiseTexture = new Texture2D(1280, 720);
                //}
                _material.SetTexture("_NoiseTex", noiseTexture);
            }

            // Calculate vectors towards frustum corners.
            Camera camera = Camera.main;

            if (isForReflections && reflectCamera != null)
            {
                // camera = reflectionc UnityEngine.Rendering.Universal.RenderingData.ca
                // ScriptableRenderContext context, UnityEngine.Rendering.Universal.RenderingData renderingData
                camera = reflectCamera;
            }

            if (isForReflections && isForDualCameras) //v1.9.9.7 - Ethereal v1.1.8f
            {
                //if list has members, choose 0 for 1st etc
                if (extraCameras.Count > 0 && extraCameraID >= 0 && extraCameraID < extraCameras.Count)
                {
                    camera = extraCameras[extraCameraID];
                }
            }

            //v1.7.1 - Solve editor flickering
            if (Camera.current != null)
            {
                camera = Camera.current;
            }

            var cam = camera;// GetComponent<Camera>();
            var camtr = cam.transform;


            ////////// SCATTER
            var camPos = camtr.position;
            float FdotC = camPos.y - _fogHeight;
            float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);


            ///// ffrustrum
            //float camNear = cam.nearClipPlane;
            //float camFar = cam.farClipPlane;
            //float camFov = cam.fieldOfView;
            //float camAspect = cam.aspect;

            //Matrix4x4 frustumCorners = Matrix4x4.identity;

            //float fovWHalf = camFov * 0.5f;

            //Vector3 toRight = camtr.right * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
            //Vector3 toTop = camtr.up * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            //Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
            //float camScale = topLeft.magnitude * camFar / camNear;

            //topLeft.Normalize();
            //topLeft *= camScale;

            //Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
            //topRight.Normalize();
            //topRight *= camScale;

            //Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
            //bottomRight.Normalize();
            //bottomRight *= camScale;

            //Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
            //bottomLeft.Normalize();
            //bottomLeft *= camScale;

            //frustumCorners.SetRow(0, topLeft);
            //frustumCorners.SetRow(1, topRight);
            //frustumCorners.SetRow(2, bottomRight);
            //frustumCorners.SetRow(3, bottomLeft);

            //_material.SetMatrix("_FrustumCornersWS", frustumCorners);

            _material.SetVector("_CameraWS", camPos);
            _material.SetFloat("blendVolumeLighting", blendVolumeLighting);//v0.2 - SHADOWS
            _material.SetFloat("_RaySamples", LightRaySamples);
            _material.SetVector("_stepsControl", stepsControl);
            _material.SetVector("lightNoiseControl", lightNoiseControl);

            //Debug.Log("_HeightParams="+ new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));

            _material.SetVector("_HeightParams", new Vector4(_fogHeight, FdotC, paramK, heightDensity * 0.5f));
            _material.SetVector("_DistanceParams", new Vector4(-Mathf.Max(startDistance, 0.0f), 0, 0, 0));
            _material.SetFloat("_NoiseDensity", noiseDensity);
            _material.SetFloat("_NoiseScale", noiseScale);
            _material.SetFloat("_NoiseThickness", noiseThickness);
            _material.SetVector("_NoiseSpeed", noiseSpeed);
            _material.SetFloat("_OcclusionDrop", occlusionDrop);
            _material.SetFloat("_OcclusionExp", occlusionExp);
            _material.SetInt("noise3D", noise3D);
            //SM v1.7
            _material.SetFloat("luminance", luminance);
            _material.SetFloat("lumFac", lumFac);
            _material.SetFloat("Multiplier1", ScatterFac);
            _material.SetFloat("Multiplier2", TurbFac);
            _material.SetFloat("Multiplier3", HorizFac);
            _material.SetFloat("turbidity", turbidity);
            _material.SetFloat("reileigh", reileigh);
            _material.SetFloat("mieCoefficient", mieCoefficient);
            _material.SetFloat("mieDirectionalG", mieDirectionalG);
            _material.SetFloat("bias", bias);
            _material.SetFloat("contrast", contrast);

            //v1.7.1 - Solve editor flickering
            Vector3 sunDir = Sun;// connector.sun.transform.forward;
            if ((Camera.current != null || isForDualCameras) && connector.sun != null) //v1.9.9.2  //v1.9.9.6 - Ethereal v1.1.8e
            {
                sunDir = connector.sun.transform.forward;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
            }

            _material.SetVector("v3LightDir", sunDir);// Sun);//.forward); //v1.7.1
            _material.SetVector("_TintColor", new Vector4(TintColor.r, TintColor.g, TintColor.b, 1));//68, 155, 345
            _material.SetVector("_TintColorK", new Vector4(TintColorK.x, TintColorK.y, TintColorK.z, 1));
            _material.SetVector("_TintColorL", new Vector4(TintColorL.x, TintColorL.y, TintColorL.z, 1));

            //v1.6 - reflections
            if (isForReflections && !isForDualCameras) //v1.9.9.6 - Ethereal v1.1.8e
            {
                _material.SetFloat("_invertX", 1);
            }
            else
            {
                _material.SetFloat("_invertX", 0);
            }

            //v1.7
            _material.SetInt("lightCount", lightCount);

            //v1.9.9
            _material.SetVector("lightControlA", lightControlA);
            _material.SetVector("lightControlB", lightControlB);
            if (lightA)
            {
                _material.SetVector("lightAcolor", new Vector3(lightA.color.r, lightA.color.g, lightA.color.b));
                _material.SetFloat("lightAIntensity", lightA.intensity); 
            }
            if (lightB)
            {
                _material.SetVector("lightBcolor", new Vector3(lightB.color.r, lightB.color.g, lightB.color.b));
                _material.SetFloat("lightBIntensity", lightA.intensity);
            }
            if (controlByColor)
            {
                _material.SetInt("controlByColor", 1);
            }
            else
            {
                _material.SetInt("controlByColor", 0);
            }

            //v1.9.9.3
            _material.SetVector("shadowsControl", shadowsControl);

            //v1.9.9.4
            _material.SetVector("volumeSamplingControl", volumeSamplingControl);

            //v1.9.9.1
            // Debug.Log(_material.HasProperty("lightsArrayLength"));
            //Debug.Log(_material.HasProperty("controlByColor"));
            if (_material.HasProperty("lightsArrayLength") && lightsArray.Count > 0) //check for other shader versions
            {
                //pass array
                _material.SetVectorArray("_LightsArrayPos", new Vector4[32]);
                _material.SetVectorArray("_LightsArrayDir", new Vector4[32]);
                int countLights = lightsArray.Count;
                if(countLights > 32)
                {
                    countLights = 32;
                }
                _material.SetInt("lightsArrayLength", countLights);
                //Debug.Log(countLights);
                // material.SetFloatArray("_Points", new float[10]);
                //float[] array = new float[] { 1, 2, 3, 4 };
                Vector4[] posArray = new Vector4[countLights];
                Vector4[] dirArray = new Vector4[countLights];
                Vector4[] colArray = new Vector4[countLights];
                for (int i=0;i< countLights; i++)
                {
                    //posArray[i].x = lightsArray(0).
                    posArray[i].x = lightsArray[i].transform.position.x;
                    posArray[i].y = lightsArray[i].transform.position.y;
                    posArray[i].z = lightsArray[i].transform.position.z;
                    posArray[i].w = lightsArray[i].intensity;
                    //Debug.Log(posArray[i].w);
                    colArray[i].x = lightsArray[i].color.r;
                    colArray[i].y = lightsArray[i].color.g;
                    colArray[i].z = lightsArray[i].color.b;

                    //check if point light
                    if (lightsArray[i].type == LightType.Point)
                    {
                        dirArray[i].x = 0;
                        dirArray[i].y = 0;
                        dirArray[i].z = 0;
                    }
                    else
                    {
                        dirArray[i].x = lightsArray[i].transform.forward.x;
                        dirArray[i].y = lightsArray[i].transform.forward.y;
                        dirArray[i].z = lightsArray[i].transform.forward.z;
                    }
                    dirArray[i].w = lightsArray[i].range;
                }
                _material.SetVectorArray("_LightsArrayPos", posArray);
                _material.SetVectorArray("_LightsArrayDir", dirArray);
                _material.SetVectorArray("_LightsArrayColor", colArray);
                //material.SetFloatArray(array);
            }
            else
            {
                _material.SetInt("lightsArrayLength", 0);
            }


            float Foggy = 0;
            if (FogSky) //ClearSkyFac
            {
                Foggy = 1;
            }
            _material.SetFloat("FogSky", Foggy);
            _material.SetFloat("ClearSkyFac", ClearSkyFac);
            //////// END SCATTER

            //LOCAL LIGHT
            _material.SetVector("localLightPos", new Vector4(PointL.x, PointL.y, PointL.z, PointL.w));//68, 155, 345
            _material.SetVector("localLightColor", new Vector4(PointLParams.x, PointLParams.y, PointLParams.z, PointLParams.w));//68, 155, 345
                                                                                                                                //END LOCAL LIGHT

            //v0.6
            _material.SetFloat("depthDilation", depthDilation);
            _material.SetFloat("_TemporalResponse", TemporalResponse);
            _material.SetFloat("_TemporalGain", TemporalGain);

            //RENDER FINAL EFFECT
            int rtW = opaqueDesc.width;
            int rtH = opaqueDesc.height;
            //var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
            var format = allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9 //v LWRP //v0.7

           // Debug.Log(renderingData.cameraData.camera.allowHDR);

            //RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(context.width, context.height, 0, format);
            RenderTexture tmpBuffer1 = RenderTexture.GetTemporary(rtW, rtH, 0, format);
            RenderTexture.active = tmpBuffer1;

            GL.ClearWithSkybox(false, camera);
            ////context.command.BlitFullscreenTriangle(context.source, tmpBuffer1);

            //Blit(cmd, source, m_TemporaryColorTexture.Identifier()); //KEEP BACKGROUND
            Blit(cmd, source, tmpBuffer1); //KEEP BACKGROUND
                                           // cmd.SetGlobalTexture("_ColorBuffer", lrDepthBuffer.Identifier());
                                           // Blit(cmd, m_TemporaryColorTexture.Identifier(), source, _material, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);

            if (!enableComposite)
            {
                _material.SetTexture("_MainTex", tmpBuffer1);

                //WORLD RECONSTRUCT        
                Matrix4x4 camToWorld = camera.cameraToWorldMatrix;// context.camera.cameraToWorldMatrix;
                                                                  //Debug.Log(camToWorld);
                _material.SetMatrix("_InverseView", camToWorld);


                //v0.6
                //int downSample = 2;
                RenderTexture tmpBuffer2 = RenderTexture.GetTemporary(rtW / downSample, rtH / downSample, 0, format);

                /////context.command.BlitFullscreenTriangle(context.source, context.destination, _material, 0);
                //Blit(cmd, m_TemporaryColorTexture.Identifier(), source, _material, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
                //Blit(cmd, tmpBuffer1, source, _material, 6);//v0.6
                Blit(cmd, tmpBuffer1, tmpBuffer2, _material, 6);

                //v0.6
                // if (previousFrameTexture != null) { previousFrameTexture.Release(); }
                // if (previousDepthTexture != null) { previousDepthTexture.Release(); }
                if (previousFrameTexture == null)
                {
                    previousFrameTexture = new RenderTexture((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, format);// RenderTextureFormat.DefaultHDR);//v0.7
                    previousFrameTexture.filterMode = FilterMode.Point;
                    previousFrameTexture.Create();
                }
                if (previousDepthTexture == null)
                {
                    previousDepthTexture = new RenderTexture((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, RenderTextureFormat.RFloat);
                    previousDepthTexture.filterMode = FilterMode.Point;
                    previousDepthTexture.Create();
                }
                RenderTexture tmpBuffer3 = RenderTexture.GetTemporary((int)(rtW / ((float)downSample)), (int)(rtH / ((float)downSample)), 0, format);
                //bool temporalAntiAliasing = true;
                if (enabledTemporalAA && Time.fixedTime > 0.05f) //if (temporalAntiAliasing)
                {
                    //Debug.Log("AA Enabled");
                    var worldToCameraMatrix = Camera.main.worldToCameraMatrix;
                    var projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
                    _material.SetMatrix("_InverseProjectionMatrix", projectionMatrix.inverse);
                    viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
                    _material.SetMatrix("_InverseViewProjectionMatrix", viewProjectionMatrix.inverse);
                    _material.SetMatrix("_LastFrameViewProjectionMatrix", lastFrameViewProjectionMatrix);
                    _material.SetMatrix("_LastFrameInverseViewProjectionMatrix", lastFrameInverseViewProjectionMatrix);
                    _material.SetTexture("_ColorBuffer", tmpBuffer2);//CloudMaterial.SetTexture("_CloudTex", rtClouds);
                    _material.SetTexture("_PreviousColor", previousFrameTexture);
                    _material.SetTexture("_PreviousDepth", previousDepthTexture);
                    cmd.SetRenderTarget(tmpBuffer3);
                    if (mesh == null)
                    {
                        Awake();
                    }
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 7);// (int)RenderPass.TemporalReproj);
                                                                                                     //cmd.blit(context.source, context.destination, _material, 0);
                    cmd.Blit(tmpBuffer3, previousFrameTexture);
                    cmd.Blit(tmpBuffer3, tmpBuffer2);

                    cmd.SetRenderTarget(previousDepthTexture);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 8);//(int)RenderPass.GetDepth);

                    //DEBUG TAA
                    //cmd.Blit(previousDepthTexture, rtClouds);
                    //Blit(cmd, previousDepthTexture, source); //Blit(cmd, rtClouds, source);
                    //context.ExecuteCommandBuffer(cmd);
                    //CommandBufferPool.Release(cmd);
                    //RenderTexture.ReleaseTemporary(rtClouds);
                    //RenderTexture.ReleaseTemporary(tmpBuffer1);
                    //return;
                    //END DEBUG TAA
                }
                //Blit(cmd, tmpBuffer2, source);
                //v0.6a
                RenderTexture tmpBuffer4 = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                if (enableBlendMode)
                {
                    //v0.6a
                    _material.SetFloat("controlBackAlphaPower", controlBackAlphaPower);
                    _material.SetFloat("controlCloudAlphaPower", controlCloudAlphaPower);
                    _material.SetVector("controlCloudEdgeA", controlCloudEdgeA);
                    //cmd.SetRenderTarget(source);
                    //RenderTexture.active = tmpBuffer1;

                    _material.SetTexture("_ColorBuffer", tmpBuffer2);
                    _material.SetTexture("_ManTex", tmpBuffer1);
                    //_material.SetTexture("_ColorBuffer", tmpBuffer3);
                    //Blit(cmd, tmpBuffer1, source, _material, 9);
                    cmd.SetRenderTarget(tmpBuffer4);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 9);
                    Blit(cmd, tmpBuffer4, source);
                }
                else
                {
                    Blit(cmd, tmpBuffer2, source);
                }

                RenderTexture.ReleaseTemporary(tmpBuffer1); RenderTexture.ReleaseTemporary(tmpBuffer2);
                //END RENDER FINAL EFFECT


                ////RELEASE TEMPORARY TEXTURES AND COMMAND BUFFER
                //cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
                //cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
                context.ExecuteCommandBuffer(cmd);
                //CommandBufferPool.Release(cmd); //DO NOT release fog here because sun shafts may be active v1.9.9.2

                //v0.6
                RenderTexture.ReleaseTemporary(tmpBuffer3); RenderTexture.ReleaseTemporary(tmpBuffer4);
                lastFrameViewProjectionMatrix = viewProjectionMatrix;
                lastFrameInverseViewProjectionMatrix = viewProjectionMatrix.inverse;
            }
            else
            {
                //SSMS
                //_material.SetTexture("_MainTex", tmpBuffer1); //SSMS

                //WORLD RECONSTRUCT    
                Matrix4x4 camToWorld = camera.cameraToWorldMatrix;// context.camera.cameraToWorldMatrix;
                //Debug.Log(camToWorld);
                _material.SetMatrix("_InverseView", camToWorld);

                //bool enableComposite = true;
                int dimsW = rtW;
                int dimsH = rtH;
                if (enableComposite)
                {
                    //
                }
                else
                {
                    _material.SetTexture("_MainTex", tmpBuffer1);//SSMS
                    dimsW = rtW / downSample;
                    dimsH = rtH / downSample;
                }
                //v0.6
                //int downSample = 2;
                RenderTexture tmpBuffer2 = RenderTexture.GetTemporary(dimsW, dimsH, 0, format);//SSMS
                RenderTexture tmpBuffer3 = RenderTexture.GetTemporary((int)(rtW / ((float)downSampleAA)), (int)(rtH / ((float)downSampleAA)), 0, format);

                /////context.command.BlitFullscreenTriangle(context.source, context.destination, _material, 0);
                //Blit(cmd, m_TemporaryColorTexture.Identifier(), source, _material, (screenBlendMode == BlitVolumeFogSRP.BlitSettings.ShaftsScreenBlendMode.Screen) ? 0 : 4);
                //Blit(cmd, tmpBuffer1, source, _material, 6);//v0.6

                RenderTexture tmpBuffer2a = RenderTexture.GetTemporary(rtW / downSample, rtH / downSample, 0, format);
                if (enableComposite)
                {
                    Blit(cmd, tmpBuffer2a, tmpBuffer2a, _material, 6);//SSMS

                    //v0.6 - TEMPORAL
                    // if (previousFrameTexture != null) { previousFrameTexture.Release(); }
                    // if (previousDepthTexture != null) { previousDepthTexture.Release(); }
                    if (previousFrameTexture == null)
                    {
                        previousFrameTexture = new RenderTexture((int)(rtW / ((float)downSampleAA)), (int)(rtH / ((float)downSampleAA)), 0, format);// RenderTextureFormat.DefaultHDR);//v0.7
                        previousFrameTexture.filterMode = FilterMode.Point;
                        previousFrameTexture.Create();
                    }
                    if (previousDepthTexture == null)
                    {
                        previousDepthTexture = new RenderTexture((int)(rtW / ((float)downSampleAA)), (int)(rtH / ((float)downSampleAA)), 0, RenderTextureFormat.RFloat);
                        previousDepthTexture.filterMode = FilterMode.Point;
                        previousDepthTexture.Create();
                    }
                    //RenderTexture tmpBuffer3 = RenderTexture.GetTemporary((int)(rtW / ((float)downSampleAA)), (int)(rtH / ((float)downSampleAA)), 0, format);
                    //bool temporalAntiAliasing = true;
                    if (enabledTemporalAA && Time.fixedTime > 0.05f) //if (temporalAntiAliasing)
                    {
                        //Debug.Log("AA Enabled");
                        var worldToCameraMatrix = Camera.main.worldToCameraMatrix;
                        var projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
                        _material.SetMatrix("_InverseProjectionMatrix", projectionMatrix.inverse);
                        viewProjectionMatrix = projectionMatrix * worldToCameraMatrix;
                        _material.SetMatrix("_InverseViewProjectionMatrix", viewProjectionMatrix.inverse);
                        _material.SetMatrix("_LastFrameViewProjectionMatrix", lastFrameViewProjectionMatrix);
                        _material.SetMatrix("_LastFrameInverseViewProjectionMatrix", lastFrameInverseViewProjectionMatrix);
                        _material.SetTexture("_ColorBuffer", tmpBuffer2a);//CloudMaterial.SetTexture("_CloudTex", rtClouds);
                        _material.SetTexture("_PreviousColor", previousFrameTexture);
                        _material.SetTexture("_PreviousDepth", previousDepthTexture);
                        cmd.SetRenderTarget(tmpBuffer3);
                        if (mesh == null)
                        {
                            Awake();
                        }
                        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 7);// (int)RenderPass.TemporalReproj);
                                                                                                         //cmd.blit(context.source, context.destination, _material, 0);
                        cmd.Blit(tmpBuffer3, previousFrameTexture);
                        cmd.Blit(tmpBuffer3, tmpBuffer2a);

                        cmd.SetRenderTarget(previousDepthTexture);
                        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 8);//(int)RenderPass.GetDepth);
                    }

                    //SSMS           
                    Shader.SetGlobalTexture("_FogTex", tmpBuffer2a);
                    //_material.SetTexture("_MainTex", tmpBuffer1);//SSMS
                    _material.SetTexture("_BaseTex", tmpBuffer1);//SSMS
                    Blit(cmd, tmpBuffer1, tmpBuffer2, _material, 19);//SSMS
                }
                else
                {
                    Blit(cmd, tmpBuffer1, source, _material, 6);//v0.6
                }

                //MOVED AA to VOLUME LIGHTING BUFFER ABOVE

                //Blit(cmd, tmpBuffer2, source);
                //v0.6a
                RenderTexture tmpBuffer4 = RenderTexture.GetTemporary(rtW, rtH, 0, format);
                if (enableBlendMode)
                {
                    //v0.6a
                    _material.SetFloat("controlBackAlphaPower", controlBackAlphaPower);
                    _material.SetFloat("controlCloudAlphaPower", controlCloudAlphaPower);
                    _material.SetVector("controlCloudEdgeA", controlCloudEdgeA);
                    //cmd.SetRenderTarget(source);
                    //RenderTexture.active = tmpBuffer1;
                    _material.SetTexture("_ColorBuffer", tmpBuffer2);
                    _material.SetTexture("_ManTex", tmpBuffer1);
                    //_material.SetTexture("_ColorBuffer", tmpBuffer3);
                    //Blit(cmd, tmpBuffer1, source, _material, 9);
                    cmd.SetRenderTarget(tmpBuffer4);
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 9);
                    Blit(cmd, tmpBuffer4, source);
                }
                else
                {
                    Blit(cmd, tmpBuffer2, source);
                }

                RenderTexture.ReleaseTemporary(tmpBuffer1); RenderTexture.ReleaseTemporary(tmpBuffer2);
                //END RENDER FINAL EFFECT

                ////RELEASE TEMPORARY TEXTURES AND COMMAND BUFFER
                //cmd.ReleaseTemporaryRT(lrDepthBuffer.id);
                //cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
                context.ExecuteCommandBuffer(cmd);
                //CommandBufferPool.Release(cmd); //DO NOT release fog here because sun shafts may be active v1.9.9.2

                //v0.6
                RenderTexture.ReleaseTemporary(tmpBuffer3); RenderTexture.ReleaseTemporary(tmpBuffer4); RenderTexture.ReleaseTemporary(tmpBuffer2a);
                lastFrameViewProjectionMatrix = viewProjectionMatrix;
                lastFrameInverseViewProjectionMatrix = viewProjectionMatrix.inverse;
            }
        }

        //v0.6
        Matrix4x4 lastFrameViewProjectionMatrix;
        Matrix4x4 viewProjectionMatrix;
        Matrix4x4 lastFrameInverseViewProjectionMatrix;
        void OnDestroy()
        {
            if (previousFrameTexture != null)
            {
                previousFrameTexture.Release();
                previousFrameTexture = null;
            }

            if (previousDepthTexture != null)
            {
                previousDepthTexture.Release();
                previousDepthTexture = null;
            }
        }
        RenderTexture previousFrameTexture;
        RenderTexture previousDepthTexture;
        Mesh mesh;
        void Awake()
        {
            mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, -1, 1)
            };
            mesh.uv = new Vector2[]
            {
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1)
            };
            mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
        }
        /////////////////////// END VOLUME FOG SRP ///////////////////////////////// 

    }
}
