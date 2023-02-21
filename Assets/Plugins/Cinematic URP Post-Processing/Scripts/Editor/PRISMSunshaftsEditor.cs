using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;


[VolumeComponentEditor(typeof(PRISMSunshafts))]
public class PRISMSunshaftsEditor : VolumeComponentEditor
{

    SerializedObject serObj;

    SerializedDataParameter intensity;
    SerializedDataParameter rayDecay;
    SerializedDataParameter rayDensity;
    SerializedDataParameter sunTransform;
    SerializedDataParameter sunTransformPosition;
    SerializedDataParameter useUltraQuality;
    SerializedDataParameter useDownsampling, sunColor;


     GUIContent raysCasterContent = new GUIContent("   >Ray Caster Transform", "The transform that the rays should come from (Usuall a directional light)");



    public override void OnEnable()
    {
        serObj = new SerializedObject(target);

        var prism = new PropertyFetcher<PRISMSunshafts>(serializedObject);

        //var prismset = new PropertyModification
        intensity = Unpack(prism.Find(x => x.rayIntensity));
        rayDecay = Unpack(prism.Find(x => x.rayDecay));
        rayDensity = Unpack(prism.Find(x => x.rayDensity));
        sunTransformPosition = Unpack(prism.Find(x => x.sunTransformPosition));
        useUltraQuality = Unpack(prism.Find(x => x.useUltraQuality));
        useDownsampling = Unpack(prism.Find(x => x.useMobileDownsampling));
        sunColor
             = Unpack(prism.Find(x => x.sunColor));

    }

    public static float SnapTo(float a, float snap)
    {
        return Mathf.Round(a / snap) * snap;
    }

    public override void OnInspectorGUI()
    {
        target.SetAllOverridesTo(true);

        string helpString = "Tip: Try and use a low Intensity for smoother looking rays where possible. \n Ray density controls the total reach of your rays across the screen, Ray decay controls how fast they fall off.";
        EditorGUILayout.HelpBox(helpString, MessageType.Info);

        PropertyField(intensity);
        PropertyField(rayDecay);
        PropertyField(rayDensity);
        PropertyField(useUltraQuality);
        PropertyField(useDownsampling);
        PropertyField(sunColor);


        var prismRef = target as PRISMSunshafts;

        EditorGUI.BeginChangeCheck();
        Transform raysT = (Transform)EditorGUILayout.ObjectField(raysCasterContent, prismRef.sunTransform.value, typeof(Transform), true);
        if (EditorGUI.EndChangeCheck())
        {
            PRISMSunshafts_URP.SetSunTransform(raysT);
            Undo.RecordObject(target, "Rays transform");
            prismRef.sunTransform.value = raysT;
            sunTransformPosition.value.vector3Value = raysT.transform.position;

            if (sunTransformPosition.value.vector3Value != raysT.transform.position)
            {
                sunTransformPosition.value.vector3Value = raysT.transform.position;
            }
        }



        if (GUILayout.Button("Set Rays Transform To Directional Light"))
        {
            var sunsInScene = Object.FindObjectsOfType((typeof(Light))) as Light[];
            foreach (var v in sunsInScene)
            {
                if (v.type == LightType.Directional)
                {
                    prismRef.sunTransform.value = v.transform;
                }
            }
        }

        /*
        if(Camera.main.depthTextureMode == DepthTextureMode.None)
        {
            string helpString = "Your main camera, on the gameobject: ";
            helpString += Camera.main.gameObject.name;
            helpString += " does not have a Depth Texture enabled. Ensure the depth texture is enabled for Godrays to work.";
            EditorGUILayout.HelpBox(helpString, MessageType.Info);

            if (GUILayout.Button("Enable Main Camera Depth"))
            {
                Camera.main.depthTextureMode = DepthTextureMode.Depth;
            }
        }*/



        /* prism.useRays = EditorGUILayout.Toggle(raysContent, prism.useRays);
         if (EditorGUI.EndChangeCheck())
         {
             if (!prism.rayTransform)
             {

             }
         }*/
         


        serializedObject.ApplyModifiedProperties();

    }
}
