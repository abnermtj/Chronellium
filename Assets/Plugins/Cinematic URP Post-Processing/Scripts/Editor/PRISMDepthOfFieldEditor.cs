using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Rendering;


[VolumeComponentEditor(typeof(PRISMDepthOfField))]
public class PRISMDepthOfFieldEditor : VolumeComponentEditor
{

    SerializedObject serObj;

    SerializedDataParameter enableDoF;
    SerializedDataParameter aperture;
    SerializedDataParameter focalLength;
    SerializedDataParameter enableGPUAutofocus;
    SerializedDataParameter autoFocusSpeed;
    SerializedDataParameter maxBlur;
    SerializedDataParameter focusDistance;
    SerializedDataParameter frontBlur;
    SerializedDataParameter mobile;
    [HideInInspector]
    SerializedDataParameter bokehIntensity;
    [HideInInspector]
    SerializedDataParameter bokehRings;
    [HideInInspector]
    SerializedDataParameter apertureEdgeCount;
    [HideInInspector]
    SerializedDataParameter dofSamplesPerRing;
    [HideInInspector]
    SerializedDataParameter dofSamplesPerEdge;

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
    GUIContent dofAFContent = new GUIContent("Use GPU Autofocus", "Select this to enable a GPU depth-based single-point, centre-screen autofocus");
    GUIContent dofFocalLengthContent = new GUIContent("Focal length", "Focal Length of the camera lens");
    GUIContent dofApertureContent = new GUIContent("Aperture", "Aperture of the lens. A lower aperture means more blur and a shallower plane of focus");

    public override void OnEnable()
    {
        serObj = new SerializedObject(target);

        var prism = new PropertyFetcher<PRISMDepthOfField>(serializedObject);

        //var prismset = new PropertyModification
        enableDoF = Unpack(prism.Find(x => x.enableDoF));
        aperture = Unpack(prism.Find(x => x.aperture));
        focalLength = Unpack(prism.Find(x => x.focalLength));
        enableGPUAutofocus = Unpack(prism.Find(x => x.enableGPUAutofocus));
        autoFocusSpeed = Unpack(prism.Find(x => x.autoFocusSpeed));
        focusDistance = Unpack(prism.Find(x => x.focusDistance));
        bokehIntensity = Unpack(prism.Find(x => x.bokehIntensity));
        bokehRings = Unpack(prism.Find(x => x.bokehRings));
        apertureEdgeCount = Unpack(prism.Find(x => x.apertureEdgeCount));
        dofSamplesPerRing = Unpack(prism.Find(x => x.dofSamplesPerRing));
        maxBlur = Unpack(prism.Find(x => x.maxBlur));
        frontBlur = Unpack(prism.Find(x => x.enableFrontBlur));
        mobile = Unpack(prism.Find(x => x.enableFastMobileDOF));

    }

    public static float SnapTo(float a, float snap)
    {
        return Mathf.Round(a / snap) * snap;
    }

    public override void OnInspectorGUI()
    {
        target.SetAllOverridesTo(true);

        PropertyField(enableDoF);
        //PropertyField(aperture);

        EditorGUILayout.HelpBox("A lower Aperture, or F-Stop creates a shallower Depth of Field with MORE blur. A HIGHER aperture creates less blur. A LARGER focal length creates more blur, a LOWER focal length creates less blur and a thinner area where everything is in focus.", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        float fAp = EditorGUILayout.Slider(dofApertureContent, aperture.value.floatValue, 1f, 32f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Lensap");
            aperture.value.floatValue = fAp;
        }

        //PropertyField(focalLength);

        EditorGUI.BeginChangeCheck();
        float fLength = EditorGUILayout.Slider(dofFocalLengthContent, focalLength.value.floatValue, 8f, 500f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Lens focal length");
            focalLength.value.floatValue = fLength;
        }


        PropertyField(maxBlur);

        PropertyField(mobile);

       if(mobile.value.boolValue == true)
        {
            EditorGUILayout.LabelField("    >Front-Field Blur - cannot use with Mobile DoF");
            //EditorGUILayout.LabelField("    >GPU Autofocus - cannot use with Mobile DoF");

            //PropertyField(focusDistance);
        }
        else
        {
            PropertyField(frontBlur);

        }

       if(mobile.value.boolValue == true)
        {
            frontBlur.value.boolValue = false;
        }

        PropertyField(focusDistance);
        /*else
        {
            PropertyField(frontBlur);

            EditorGUI.BeginChangeCheck();
            bool gpuAF = EditorGUILayout.Toggle(dofAFContent, enableGPUAutofocus.value.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "GPU AF");
                enableGPUAutofocus.value.boolValue = gpuAF;
            }

            if (!gpuAF)
            {
                PropertyField(focusDistance);
            }
            else
            {
                PropertyField(autoFocusSpeed);
            }
        }*/

        serializedObject.ApplyModifiedProperties();

    }
}
