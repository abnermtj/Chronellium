namespace PlanarReflections4 {

    /*
    * PIDI - Planar Reflections™ 4 - Copyright© 2017-2021
    * PIDI - Planar Reflections is a trademark and copyrighted property of Jorge Pinal Negrete.

    * You cannot sell, redistribute, share nor make public this code, modified or not, in part nor in whole, through any
    * means on any platform except with the purpose of contacting the developers to request support and only when taking
    * all pertinent measures to avoid its release to the public and / or any unrelated third parties.
    * Modifications are allowed only for internal use within the limits of your Unity based projects and cannot be shared,
    * published, redistributed nor made available to any third parties unrelated to Irreverent Software by any means.
    *
    * For more information, contact us at support@irreverent-software.com
    *
    */

    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
    using UnityEngine.Rendering.PostProcessing;
#endif

    [System.Serializable]
    public class PlanarReflectionSettings {

        public float nearClipPlane = 0.03f;

        public float farClipPlane = 1000;

        public LayerMask reflectLayers = 1;

        [Range( 0, 1 )] public float customLODBias = 1.0f;

        public int maxLODLevel;

        public bool renderShadows = true;

        public bool usePostFX = true;

        public LayerMask postFXVolumeMask;

        public bool accurateMatrix = true;

        public string camerasTag;

        public bool screenBasedResolution = true;

        public Vector2 explicitResolution = new Vector2( 512, 512 );

        [Range( 0.1f, 2.0f )] public float outputResolutionMultiplier = 1.0f;

        [Range( 0, 60 )] public int reflectionFramerate = 0;

        public bool useMipMaps = true;

        public bool useAntialiasing = false;

        public bool clearToColor = false;

        public Color backgroundColor = Color.blue;

        public bool renderDepth = false;

        public bool renderFog;

        public int fogRendererIndex = 1;

        public bool forceHDR = true;

        public bool updateOnCastOnly = true;

        public bool framerateByDistance = true;

        public float framerateThreshold = 20;

        public Material customSkybox;

#if UPDATE_PLANAR3

        #region LEGACY_API
        /// <summary>
        /// This is a legacy method and will be deprecated soon. Please use nearClipPlane instead.
        /// <br>Controls the near clip plane value of the virtual camera that renders the reflection. When using accurate matrices it is ignored and the reflective surface's plane is used instead.</br>
        /// </summary>
        public float nearClipDistance { get { return nearClipPlane; } set { nearClipPlane = value; } }

        public float farClipDistance { get { return farClipPlane; } set { farClipPlane = value; } }

        public bool customShadowDistance { get { return renderShadows; } }

        public float shadowDistance { get { return 50f; } }

        public int targetFramerate { get { return reflectionFramerate; } set { reflectionFramerate = value; } }

        public bool useDepth { get { return renderDepth; } set { renderDepth = value; } }

        public string CamerasTag { get { return camerasTag; } set { camerasTag = value; } }

        public bool trackCamerasWithTag { get { return !string.IsNullOrEmpty( camerasTag ); } }

        public PlanarReflections3.ReflectionClipMode reflectionClipMode {
            get { return accurateMatrix ? PlanarReflections3.ReflectionClipMode.AccurateClipping : PlanarReflections3.ReflectionClipMode.SimpleApproximation; }
            set { accurateMatrix = value == PlanarReflections3.ReflectionClipMode.AccurateClipping; }
        }

        public PlanarReflections3.ResolutionMode resolutionMode {
            get { return screenBasedResolution ? PlanarReflections3.ResolutionMode.ScreenBased : PlanarReflections3.ResolutionMode.ExplicitValue; }
            set { screenBasedResolution = value == PlanarReflections3.ResolutionMode.ScreenBased; }
        }

        public bool useCustomClearFlags { get { return true; } }

        public int clearFlags { get { return clearToColor ? 1 : 0; } set { clearToColor = value == 1; } }

        public bool forceFloatOutput { get { return forceHDR; } set { forceHDR = value; } }

        #endregion
#endif

#if UPDATE_PLANAR3

        public static implicit operator PlanarReflectionSettings( PlanarReflections3.ReflectionSettings source ) {

            var newSettings = new PlanarReflectionSettings();

            newSettings.nearClipPlane = source.nearClipDistance;
            newSettings.farClipPlane = source.farClipDistance;
            newSettings.reflectLayers = source.reflectLayers;
            newSettings.renderShadows = !source.customShadowDistance || source.shadowDistance > 0.1f;
            newSettings.reflectionFramerate = source.targetFramerate;
            newSettings.usePostFX = source.usePostFX;
            newSettings.explicitResolution = source.explicitResolution;
            newSettings.renderDepth = source.useDepth;
            newSettings.useMipMaps = source.useMipMaps;
            newSettings.useAntialiasing = source.useAntialiasing;
            newSettings.clearToColor = source.useCustomClearFlags && source.clearFlags == 1;
            newSettings.camerasTag = source.trackCamerasWithTag ? source.CamerasTag : "";
            newSettings.outputResolutionMultiplier = source.resolutionDownscale;
            newSettings.forceHDR = source.forceFloatOutput;
            newSettings.backgroundColor = source.backgroundColor;
            newSettings.accurateMatrix = source.reflectionClipMode == PlanarReflections3.ReflectionClipMode.AccurateClipping;
            newSettings.screenBasedResolution = source.resolutionMode == PlanarReflections3.ResolutionMode.ScreenBased;

            return newSettings;

        }

#endif


    }

    [System.Serializable]
    public class ReflectionData {
        protected Camera _reflectionCam;
        public Camera ReflectionCamera { get { return _reflectionCam; } }

        public UniversalAdditionalCameraData UniversalData;

        public RenderTexture _reflectionTex;

        public RenderTexture _reflectionDepth;
        
        public RenderTexture _reflectionFog;

        public Vector2Int screenRes;

        public int recursionLevel = 0;

        public float timer;

        public void ForceSetCamera( Camera cam ) {
            _reflectionCam = cam;
            UniversalData = _reflectionCam.GetUniversalAdditionalCameraData();
        }

        public ReflectionData( Camera cam, PlanarReflectionSettings settings ) {
            _reflectionCam = cam;
            _reflectionTex = RenderTexture.GetTemporary( 1, 1 );
            _reflectionDepth = RenderTexture.GetTemporary( 1, 1 );
            _reflectionFog = RenderTexture.GetTemporary( 1, 1 );
            screenRes = new Vector2Int( Screen.width, Screen.height );

            UniversalData = _reflectionCam.GetUniversalAdditionalCameraData();

            RegenerateTextures( settings );

        }

        public void RegenerateTextures( PlanarReflectionSettings settings ) {

            RenderTexture.ReleaseTemporary( _reflectionTex );
            RenderTexture.ReleaseTemporary( _reflectionDepth );
            RenderTexture.ReleaseTemporary( _reflectionFog );

            var rd = new RenderTextureDescriptor( Mathf.RoundToInt( settings.outputResolutionMultiplier * ( settings.screenBasedResolution ? Screen.width : settings.explicitResolution.x ) ), Mathf.RoundToInt( settings.outputResolutionMultiplier * ( settings.screenBasedResolution ? Screen.height : settings.explicitResolution.y ) ) );
            rd.useMipMap = settings.useMipMaps;
            rd.msaaSamples = 1;
            rd.colorFormat = settings.forceHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            rd.depthBufferBits = 16;
            rd.autoGenerateMips = true;
            rd.volumeDepth = 1;
            rd.vrUsage = VRTextureUsage.None;
            rd.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            rd.mipCount = 6;
            _reflectionTex = RenderTexture.GetTemporary( rd );
            _reflectionTex.filterMode = FilterMode.Bilinear;
            if ( settings.renderDepth ) {
                rd.colorFormat = RenderTextureFormat.Depth;
                _reflectionDepth = RenderTexture.GetTemporary( rd );
                _reflectionDepth.filterMode = FilterMode.Bilinear;
                _reflectionDepth.name = "_REFDEPTHP4";
            }
            else {
                RenderTexture.ReleaseTemporary( _reflectionDepth );
            }
            
            if ( settings.renderFog ) {
                rd.colorFormat = RenderTextureFormat.Default;
                _reflectionFog = RenderTexture.GetTemporary( rd );
                _reflectionFog.filterMode = FilterMode.Bilinear;
                _reflectionFog.name = "_REFFOGP4";
            }
            else {
                RenderTexture.ReleaseTemporary( _reflectionFog );
            }
            screenRes = new Vector2Int( Screen.width, Screen.height );
            _reflectionTex.name = "_REFTEXP4";

        }

    }

    [HelpURL( "https://irreverent-software.com/docs/pidi-planar-reflections-4/getting-started/installation/" )]
    [ExecuteAlways]
    public class PlanarReflectionRenderer : MonoBehaviour {

#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessResources internalPostFXResources;
#endif


#if UNITY_EDITOR

        public Mesh defaultReflectorMesh;

        public Material defaultReflectorMaterial;

        public bool showPreviewReflector;

        public string Version { get { return "4.1.0"; } }

#endif

        [SerializeField] protected PlanarReflectionSettings _settings = new PlanarReflectionSettings();

        public PlanarReflectionSettings Settings { get { return _settings; } }

        public RenderTexture externalReflectionTex;

        public RenderTexture externalReflectionDepth;

        protected Dictionary<Camera, ReflectionData> _reflectionData = new Dictionary<Camera, ReflectionData>();

        private List<Camera> _reflectionCameras = new List<Camera>( new Camera[1] );


        private Camera[] updateCams = new Camera[0];

#if UNITY_EDITOR

        protected ReflectionData _sceneReflection;

        private MaterialPropertyBlock _sceneReflectorMatBlock;

#endif


        public Texture GetReflection( Camera cam ) {

            if ( cam.cameraType == CameraType.SceneView ) {
#if UNITY_EDITOR
                if ( externalReflectionTex ) {
                    return _sceneReflection != null && _sceneReflection._reflectionTex != null ? (Texture)externalReflectionTex : Texture2D.blackTexture;
                }
                return _sceneReflection != null && _sceneReflection._reflectionTex != null ? (Texture)_sceneReflection._reflectionTex : Texture2D.blackTexture;
#else
                return Texture2D.blackTexture;
#endif
            }
            else {

                if ( externalReflectionTex ) {
                    return _reflectionData.ContainsKey( cam ) ? (Texture)externalReflectionTex : Texture2D.blackTexture;
                }

                if ( _reflectionData.ContainsKey( cam ) ) {
                    return _reflectionData[cam]._reflectionTex;
                }
                else {
                    return Texture2D.blackTexture;
                }
            }


        }


        public Texture GetReflectionDepth( Camera cam ) {

            if ( cam.cameraType == CameraType.SceneView ) {
#if UNITY_EDITOR
                return _sceneReflection != null && _sceneReflection._reflectionDepth != null ? (Texture)_sceneReflection._reflectionDepth : Texture2D.whiteTexture;
#else
                return Texture2D.blackTexture;
#endif
            }
            else {
                if ( _reflectionData.ContainsKey( cam ) ) {
                    return _reflectionData[cam]._reflectionDepth;
                }
                else {
                    return Texture2D.blackTexture;
                }
            }


        }

        
        public Texture GetReflectionFog( Camera cam ) {

            if ( cam.cameraType == CameraType.SceneView ) {
#if UNITY_EDITOR
                return _sceneReflection != null && _sceneReflection._reflectionFog != null ? (Texture)_sceneReflection._reflectionFog : Texture2D.blackTexture;
#else
                return Texture2D.blackTexture;
#endif
            }
            else {
                if ( _reflectionData.ContainsKey( cam ) ) {
                    return _reflectionData[cam]._reflectionFog;
                }
                else {
                    return Texture2D.blackTexture;
                }
            }


        }



#if UNITY_EDITOR
        [UnityEditor.MenuItem( "GameObject/Effects/Planar Reflections 4/Create Reflections Renderer", priority = -99 )]
        public static void CreateReflectionsRendererObject() {

            var reflector = new GameObject( "Reflection Renderer", typeof( PlanarReflectionRenderer ) );
            reflector.transform.position = Vector3.zero;
            reflector.transform.rotation = Quaternion.identity;

        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "_URPReflectionCamera" ) ) {
#if !UNITY_2018_3_OR_NEWER
                    DestroyImmediate( cam.targetTexture );
#else
                    RenderTexture.ReleaseTemporary( cam.targetTexture );
#endif
                    cam.targetTexture = null;
                    DestroyImmediate( cam.gameObject );
                }
            }
        }

#endif




        public void OnEnable() {

            _reflectionCameras = new List<Camera>( new Camera[1] );

#if UNITY_EDITOR


#if UNITY_2018_1_OR_NEWER
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
#endif


            UnityEditor.Undo.undoRedoPerformed += ApplySettings;

            if ( UnityEditor.AssetDatabase.FindAssets( "Planar4Logo_Gizmos" ).Length < 1 ) {

                var sceneIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>( UnityEditor.AssetDatabase.GUIDToAssetPath( UnityEditor.AssetDatabase.FindAssets( "l: Pidi_PlanarGizmos" )[0] ) );


                if ( !UnityEditor.AssetDatabase.IsValidFolder( "Assets/Gizmos" ) )
                    UnityEditor.AssetDatabase.CreateFolder( "Assets", "Gizmos" );
                var t = new Texture2D( sceneIcon.width, sceneIcon.height );
                t.SetPixels( sceneIcon.GetPixels() );
                System.IO.File.WriteAllBytes( Application.dataPath + "/Gizmos/Planar4Logo_Gizmos.png", t.EncodeToPNG() );
                UnityEditor.AssetDatabase.Refresh();
                var importer = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath( "Assets/Gizmos/Planar4Logo_Gizmos.png" );
                importer.isReadable = true;
                importer.textureType = UnityEditor.TextureImporterType.GUI;
                importer.SaveAndReimport();
                UnityEditor.AssetDatabase.Refresh();
            }

#endif


#if UPDATE_PLANAR3
            if ( GetComponent<PlanarReflections3.PlanarReflectionsRenderer>() ) {
                _settings = GetComponent<PlanarReflections3.PlanarReflectionsRenderer>().Settings;
            }
#endif


            //Camera.onPreCull += RenderReflection;

            RenderPipelineManager.beginCameraRendering -= RenderURPReflection;
            RenderPipelineManager.beginCameraRendering += RenderURPReflection;


        }

        public void OnDisable() {

#if UNITY_EDITOR
            UnityEditor.Undo.undoRedoPerformed -= ApplySettings;
#endif


            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= RenderURPReflection;


            foreach ( KeyValuePair<Camera, ReflectionData> pair in _reflectionData ) {
                RenderTexture.ReleaseTemporary( pair.Value._reflectionTex );
                RenderTexture.ReleaseTemporary( pair.Value._reflectionDepth );
                RenderTexture.ReleaseTemporary( pair.Value._reflectionFog );
            }

            var cams = _reflectionCameras.ToArray();

            for ( int i = 0; i < cams.Length; i++ ) {
                if ( cams[i] ) {
                    DestroyImmediate( cams[i].gameObject );
                }
            }

            _reflectionCameras.Clear();
            _reflectionCameras.Add( null );
            _reflectionData.Clear();
#if UNITY_EDITOR
            _sceneReflection = default;
#endif

        }


        public void ApplySettings() {

            foreach ( KeyValuePair<Camera, ReflectionData> pair in _reflectionData ) {
                _reflectionData[pair.Key].RegenerateTextures( _settings );
            }

#if UNITY_EDITOR
            if ( _sceneReflection != null )
                _sceneReflection.RegenerateTextures( _settings );
#endif

        }






        public void RenderURPReflection( ScriptableRenderContext context, Camera cam ) {


            if ( !_reflectionData.ContainsKey( cam )
#if UNITY_EDITOR
                ||
                _sceneReflection == null ||
                !_sceneReflection.ReflectionCamera
#endif
                ) {

#if UNITY_EDITOR

                if ( cam.cameraType == CameraType.SceneView ) {

                    if ( _sceneReflection == null ) {
                        var rCam = new GameObject( "_URPSceneReflectionCamera", typeof( Camera ), typeof( Skybox ), typeof( UniversalAdditionalCameraData ) );

                        _reflectionCameras[0] = rCam.GetComponent<Camera>();

                        rCam.hideFlags = HideFlags.HideAndDontSave;
                        rCam.GetComponent<Camera>().enabled = false;
                        rCam.GetComponent<Camera>().cameraType = CameraType.Game;

#if UNITY_POST_PROCESSING_STACK_V2
                        rCam.AddComponent<PostProcessLayer>();
                        rCam.GetComponent<PostProcessLayer>().Init( internalPostFXResources );
                        rCam.GetComponent<PostProcessLayer>().enabled = false;
#endif

                        _sceneReflection = new ReflectionData( _reflectionCameras[0], _settings );
                    }
                    else {
                        if ( !_sceneReflection.ReflectionCamera ) {
                            var rCam = new GameObject( "_URPSceneReflectionCamera", typeof( Camera ), typeof( Skybox ), typeof( UniversalAdditionalCameraData ) );

                            _reflectionCameras[0] = rCam.GetComponent<Camera>();

#if UNITY_POST_PROCESSING_STACK_V2
                        rCam.AddComponent<PostProcessLayer>();
                        rCam.GetComponent<PostProcessLayer>().Init( internalPostFXResources );
                        rCam.GetComponent<PostProcessLayer>().enabled = false;
#endif

                            rCam.hideFlags = HideFlags.HideAndDontSave;
                            rCam.GetComponent<Camera>().enabled = false;
                            rCam.GetComponent<Camera>().cameraType = CameraType.Game;
                        }

                        _sceneReflection.ForceSetCamera( _reflectionCameras[0] );
                    }

                    //var rCam = new GameObject( "_URPReflectionCamera", typeof( Camera ), typeof( Skybox ), typeof(UniversalAdditionalCameraData) );


                }
                else

#endif
                if ( cam.cameraType == CameraType.Game && !_reflectionData.ContainsKey( cam ) && !cam.gameObject.name.Contains( "ReflectionCamera" ) && cam.gameObject.hideFlags == HideFlags.None ) {
                    var rCam = new GameObject( "_URPGameReflectionCamera", typeof( Camera ), typeof( Skybox ), typeof( UniversalAdditionalCameraData ) );

                    _reflectionCameras.Add( rCam.GetComponent<Camera>() );

                    rCam.hideFlags = HideFlags.HideAndDontSave;
                    rCam.GetComponent<Camera>().enabled = false;
                    rCam.GetComponent<Camera>().cameraType = CameraType.Game;

                    _reflectionData.Add( cam, new ReflectionData( rCam.GetComponent<Camera>(), _settings ) );
                }


#if UNITY_EDITOR
                if ( cam.cameraType == CameraType.SceneView ) {
                    if ( _sceneReflection == null ) {
                        _sceneReflection = new ReflectionData( _reflectionCameras[0], _settings );
                    }
                    else {
                        _sceneReflection.ForceSetCamera( _reflectionCameras[0] );
                    }

                }
                else
# endif
                if ( cam.cameraType == CameraType.Game && !_reflectionData.ContainsKey( cam ) && !cam.gameObject.name.Contains( "ReflectionCamera" ) && cam.gameObject.hideFlags == HideFlags.None ) {
                    _reflectionData.Add( cam, new ReflectionData( _reflectionCameras[0], _settings ) );
                }
                else if ( cam.gameObject.hideFlags != HideFlags.None ) {
                    return;
                }
            }


            Plane reflectionPlane = new Plane( transform.up, transform.position );

            if ( Mathf.Abs( Vector3.Dot( transform.up, cam.transform.forward ) ) < 0.01f && ( cam.orthographic || reflectionPlane.GetDistanceToPoint( cam.transform.position ) < 0.025f ) ) {
                return;
            }

#if UNITY_EDITOR

            var isSceneCam = cam.cameraType == CameraType.SceneView;


            if ( !_reflectionData.ContainsKey( cam ) && !isSceneCam ) {
                return;
            }


            if ( isSceneCam ) {
                if ( Screen.width != _sceneReflection.screenRes.x || Screen.height != _sceneReflection.screenRes.y ) {
                    _sceneReflection.RegenerateTextures( _settings );
                }
            }
            else {
                if ( Screen.width != _reflectionData[cam].screenRes.x || Screen.height != _reflectionData[cam].screenRes.y ) {
                    _reflectionData[cam].RegenerateTextures( _settings );
                }
            }



            var currentData = isSceneCam ? _sceneReflection : _reflectionData[cam];
#else
            if ( Screen.width != _reflectionData[cam].screenRes.x || Screen.height != _reflectionData[cam].screenRes.y ) {
                _reflectionData[cam].RegenerateTextures( _settings );
            }

            var currentData = _reflectionData[cam];
#endif
            var refCamera = currentData.ReflectionCamera;

            refCamera.CopyFrom( cam );
            refCamera.allowHDR = cam.allowHDR;
            refCamera.allowMSAA = false;
            refCamera.useOcclusionCulling = false;
            refCamera.cullingMask = _settings.reflectLayers;
            refCamera.clearFlags = _settings.clearToColor ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            refCamera.backgroundColor = _settings.backgroundColor;
            refCamera.cameraType = CameraType.Game;
            refCamera.renderingPath = cam.renderingPath;

            refCamera.useOcclusionCulling = false;

            refCamera.targetTexture = externalReflectionTex ? externalReflectionTex : currentData._reflectionTex;


            Skybox skyComp;

            if ( refCamera.TryGetComponent<Skybox>( out skyComp ) ) {
                skyComp.material = _settings.customSkybox;
            }

#if UNITY_POST_PROCESSING_STACK_V2

            var uData = refCamera.GetUniversalAdditionalCameraData();

            PostProcessLayer postFX;

                if ( refCamera.TryGetComponent<PostProcessLayer>( out postFX ) ) {
                    postFX.enabled = _settings.usePostFX;
                    postFX.volumeLayer = _settings.postFXVolumeMask;
                }
#else

            var uData = refCamera.GetUniversalAdditionalCameraData();


#endif

            Vector3 worldSpaceViewDir = cam.transform.forward;
            Vector3 worldSpaceViewUp = cam.transform.up;
            Vector3 worldSpaceCamPos = cam.transform.position;

            Vector3 planeSpaceViewDir = transform.InverseTransformDirection( worldSpaceViewDir );
            Vector3 planeSpaceViewUp = transform.InverseTransformDirection( worldSpaceViewUp );
            Vector3 planeSpaceCamPos = transform.InverseTransformPoint( worldSpaceCamPos );

            planeSpaceViewDir.y *= -1.0f;
            planeSpaceViewUp.y *= -1.0f;
            planeSpaceCamPos.y *= -1.0f;

            worldSpaceViewDir = transform.TransformDirection( planeSpaceViewDir );
            worldSpaceViewUp = transform.TransformDirection( planeSpaceViewUp );
            worldSpaceCamPos = transform.TransformPoint( planeSpaceCamPos );

            refCamera.transform.position = worldSpaceCamPos;
            refCamera.transform.LookAt( worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp );

            refCamera.nearClipPlane = _settings.nearClipPlane;
            refCamera.farClipPlane = _settings.farClipPlane;

            refCamera.rect = new Rect( 0, 0, 1, 1 );

            refCamera.aspect = cam.aspect;


            if ( _settings.accurateMatrix ) {
                refCamera.projectionMatrix = refCamera.CalculateObliqueMatrix( CameraSpacePlane( refCamera, transform.position, transform.up ) );
            }


            var tempLOD = QualitySettings.lodBias;
            var maxLod = QualitySettings.maximumLODLevel;

            QualitySettings.lodBias *= _settings.customLODBias;
            QualitySettings.maximumLODLevel = _settings.maxLODLevel;


            uData.renderShadows = _settings.renderShadows;
            uData.volumeLayerMask = _settings.postFXVolumeMask;
            uData.antialiasing = _settings.useAntialiasing ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;

            if (
#if UNITY_EDITOR
            !Application.isPlaying ||
#endif
            ( Time.realtimeSinceStartup > currentData.timer ) ) {


                if ( _settings.renderFog ) {
                    refCamera.targetTexture = currentData._reflectionFog;
#if UNITY_POST_PROCESSING_STACK_V2
                    if (postFX)
                        postFX.enabled = false;
#else
                    uData.renderPostProcessing = false;
#endif
                    uData.SetRenderer( _settings.fogRendererIndex );
                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );
                    uData.SetRenderer( 0 );
                }


                if ( _settings.renderDepth ) {
                    refCamera.targetTexture = currentData._reflectionDepth;
                    refCamera.depthTextureMode = DepthTextureMode.Depth;
#if UNITY_POST_PROCESSING_STACK_V2
                    if (postFX)
                        postFX.enabled = false;
#else
                    uData.renderPostProcessing = false;
#endif
                    uData.requiresColorOption = CameraOverrideOption.Off;
                    uData.renderShadows = false;                    

                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );

#if UNITY_EDITOR
                    if ( isSceneCam ) {
                        uData.renderPostProcessing = false;
                    }
                    else {
#endif
#if UNITY_POST_PROCESSING_STACK_V2
                    if (!postFX)
#endif
                        uData.renderPostProcessing = _settings.usePostFX;
#if UNITY_EDITOR
                    }
#endif
                    uData.volumeLayerMask = _settings.postFXVolumeMask;
                }

                refCamera.targetTexture = currentData._reflectionTex;

#if UNITY_EDITOR
                if ( isSceneCam ) {
                    uData.renderPostProcessing = false;
                }
                else {
#endif
                    uData.renderPostProcessing = _settings.usePostFX;
#if UNITY_EDITOR
                }
#endif
                uData.volumeLayerMask = _settings.postFXVolumeMask;

                uData.requiresDepthOption = CameraOverrideOption.Off;
                uData.requiresColorOption = CameraOverrideOption.Off;

                if ( _settings.usePostFX
#if UNITY_EDITOR
                    && !isSceneCam
#endif
                    ) {
                    refCamera.enabled = true;
                }
                else {
                    refCamera.enabled = false;
                    UniversalRenderPipeline.RenderSingleCamera( context, refCamera );
                }
            }

            QualitySettings.maximumLODLevel = maxLod;
            QualitySettings.lodBias = tempLOD;



            if (
#if UNITY_EDITOR
                Application.isPlaying &&
#endif
                Time.realtimeSinceStartup > currentData.timer && _settings.reflectionFramerate > 0 ) {
                currentData.timer = Time.realtimeSinceStartup + ( 1.0f / _settings.reflectionFramerate );
            }

#if UNITY_EDITOR
            if ( cam.cameraType == CameraType.SceneView && showPreviewReflector )
                DrawReflectorMesh( cam, currentData );
#endif
        }




        private void LateUpdate() {

            if ( _settings.usePostFX ) {

                if ( updateCams.Length != _reflectionData.Keys.Count ) {
                    updateCams = new Camera[_reflectionData.Keys.Count];
                }

                _reflectionData.Keys.CopyTo( updateCams, 0 );

                for ( int c = 0; c < updateCams.Length; c++ ) {
                    if ( updateCams[c] != null ) {

                        Vector3 worldSpaceViewDir = updateCams[c].transform.forward;
                        Vector3 worldSpaceViewUp = updateCams[c].transform.up;
                        Vector3 worldSpaceCamPos = updateCams[c].transform.position;

                        Vector3 planeSpaceViewDir = transform.InverseTransformDirection( worldSpaceViewDir );
                        Vector3 planeSpaceViewUp = transform.InverseTransformDirection( worldSpaceViewUp );
                        Vector3 planeSpaceCamPos = transform.InverseTransformPoint( worldSpaceCamPos );

                        planeSpaceViewDir.y *= -1.0f;
                        planeSpaceViewUp.y *= -1.0f;
                        planeSpaceCamPos.y *= -1.0f;

                        worldSpaceViewDir = transform.TransformDirection( planeSpaceViewDir );
                        worldSpaceViewUp = transform.TransformDirection( planeSpaceViewUp );
                        worldSpaceCamPos = transform.TransformPoint( planeSpaceCamPos );

                        if ( _reflectionData[updateCams[c]].ReflectionCamera ) {
                            _reflectionData[updateCams[c]].ReflectionCamera.transform.position = worldSpaceCamPos;
                            _reflectionData[updateCams[c]].ReflectionCamera.transform.LookAt( worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp );
                        }

                    }
                }
            }
        }



        private Vector4 CameraSpacePlane( Camera forCamera, Vector3 planeCenter, Vector3 planeNormal ) {
            Vector3 offsetPos = planeCenter;
            Matrix4x4 mtx = forCamera.worldToCameraMatrix;
            Vector3 cPos = mtx.MultiplyPoint( offsetPos );
            Vector3 cNormal = mtx.MultiplyVector( planeNormal ).normalized * 1;
            return new Vector4( cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot( cPos, cNormal ) );
        }


#if UNITY_EDITOR
        private void DrawReflectorMesh( Camera sceneCam, ReflectionData data ) {


            var matrix = new Matrix4x4();
            matrix.SetTRS( transform.position, transform.rotation, Vector3.one * 10 );

            if ( _sceneReflectorMatBlock == null ) {
                _sceneReflectorMatBlock = new MaterialPropertyBlock();
            }

            _sceneReflectorMatBlock.SetTexture( "_ReflectionTex", data._reflectionTex ? (Texture)data._reflectionTex : (Texture)Texture2D.blackTexture );
            Graphics.DrawMesh( defaultReflectorMesh, matrix, defaultReflectorMaterial, 0, sceneCam, 0, _sceneReflectorMatBlock );

        }

        public void OnDrawGizmos() {

            Gizmos.matrix = Matrix4x4.TRS( transform.position, transform.rotation, Vector3.one );
            Gizmos.DrawIcon( transform.position + transform.rotation * Vector3.up, "Planar4Logo_Gizmos.png" );
            Gizmos.color = Color.clear;
            Gizmos.DrawCube( Vector3.zero, new Vector3( 1, 0.01f, 1 ) * 10 );
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube( Vector3.zero, new Vector3( 1, 0, 1 ) * 10 );
            Gizmos.matrix = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, Vector3.one );

        }


        public void OnDrawGizmosSelected() {

        }


#endif


    }

}