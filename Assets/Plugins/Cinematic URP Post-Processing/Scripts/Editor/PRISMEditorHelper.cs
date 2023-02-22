using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PRISMEditorHelper : MonoBehaviour
{
    static string path = "";
    // Add a menu item named "Do Something" to MyMenu in the menu bar.
    [MenuItem("Assets/Cinematic Post Processing - PRISM/Install Helper")]
    static void DoSomething()
    {
        //UnityEngine.Rendering.GraphicsSettings x = UnityEngine.Rendering.GraphicsSettings.GetGraphicsSettings() as UnityEngine.Rendering.GraphicsSettings;
        var x = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        //Debug.Log(x.name);
        Selection.activeObject = x;
        //x.GetRenderer(0).
        var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/ForwardRenderer.asset");
        if (data == null)
        {
            string[] guids1 = AssetDatabase.FindAssets("t:ForwardRendererData");
            try { 
               path = AssetDatabase.GUIDToAssetPath(guids1[0]);
            } catch(System.Exception e)
            {
                if(e == null)
                {

                }
            }
            data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
        }

        if(data != null)
        {
            Debug.Log("Found forward renderer asset: " + data.name);
            Selection.activeObject = data;
        }


        
    }
}
