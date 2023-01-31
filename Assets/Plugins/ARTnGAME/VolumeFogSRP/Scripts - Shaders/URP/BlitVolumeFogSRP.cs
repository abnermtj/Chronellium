using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal// LWRP //v1.1.8n
{
    public class BlitVolumeFogSRP : UnityEngine.Rendering.Universal.ScriptableRendererFeature
    {
        [System.Serializable]
        public class BlitSettings
        {
            //v1.9.9.7 - Ethereal v1.1.8f
            public int extraCameraID = 0; //assign 0 for reflection camera, 1 to N for choosing from the extra cameras list

            //v1.9.9.6 - Ethereal v1.1.8e
            public bool isForDualCameras = false;

            //v1.6
            public bool isForReflections = false;

            public float blendVolumeLighting = 0;
            //FOG URP /////////////
            //FOG URP /////////////
            //FOG URP /////////////
            //public float blend =  0.5f;
            public Color _FogColor = Color.white / 2;
            //fog params
            public Texture2D noiseTexture ;
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
            public float occlusionDrop =1f;
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
            public Vector4 Sun = new Vector4(0.0f, 0.0f, 0.0f, 0.0f) ;
            public bool FogSky = true;
            public float ClearSkyFac = 1f ;
            public Vector4 PointL = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            public Vector4 PointLParams = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            public bool _useRadialDistance = false;
            public bool _fadeToSkybox = true;
            //END FOG URP //////////////////
            //END FOG URP //////////////////
            //END FOG URP //////////////////

            public bool inheritFromController = true;

            public bool enableFog = true;

            public UnityEngine.Rendering.Universal.RenderPassEvent Event = UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingOpaques;
            
            public Material blitMaterial = null;
            //public Material blitMaterialFOG = null;
            public int blitMaterialPassIndex = -1;
            public Target destination = Target.Color;
            public string textureId = "_BlitPassTexture";

            /////SUN SHAFTS
            [Range(0f, 1f), Tooltip("SunShafts effect intensity.")]
            public float blend = 0.5f;

            public enum SunShaftsResolution
            {
                Low = 0,
                Normal = 1,
                High = 2,
            }

            public enum ShaftsScreenBlendMode
            {
                Screen = 0,
                Add = 1,
            }

            public SunShaftsResolution resolution = SunShaftsResolution.Normal;
            public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;
            public Vector3 sunTransform =  new Vector3(0f, 0f, 0f); // Transform sunTransform;
            public int radialBlurIterations = 2 ;
            public Color sunColor = Color.white ;
            public Color sunThreshold =new Color(0.87f, 0.74f, 0.65f) ;
            public float sunShaftBlurRadius = 2.5f ;
            public float sunShaftIntensity = 1.15f ;
            public float maxRadius = 0.75f ;
            public bool useDepthTexture = true ;
        }
        
        public enum Target
        {
            Color,
            Texture
        }

        public BlitSettings settings = new BlitSettings();
        UnityEngine.Rendering.Universal.RenderTargetHandle m_RenderTextureHandle;

        BlitPassVolumeFogSRP blitPass;
        
        public override void Create()
        {
            var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
            settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);                        

            blitPass = new BlitPassVolumeFogSRP(settings.Event, settings.blitMaterial, settings.blitMaterialPassIndex, name, settings);
            m_RenderTextureHandle.Init(settings.textureId);
        }

        public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            //v0.4
#if UNITY_2020_2_OR_NEWER           
                if (settings.blitMaterial == null)
                {
                    Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                    return;
                }

                blitPass.renderPassEvent = settings.Event;
                blitPass.settings = settings;

                renderer.EnqueuePass(blitPass);           
#else
            var src = renderer.cameraColorTarget;
            var dest = (settings.destination == Target.Color) ? UnityEngine.Rendering.Universal.RenderTargetHandle.CameraTarget : m_RenderTextureHandle;

            if (settings.blitMaterial == null)
            {
                Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
                return;
            }

            blitPass.Setup(src, dest);
            renderer.EnqueuePass(blitPass);
#endif
        }
    }
}

