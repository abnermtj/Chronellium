
#if UNITY_EDITOR
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


using UnityEditor;

class PlanarAPI{
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void ModifyDefines() {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup );
        if ( AssetDatabase.FindAssets( "PlanarReflectionsRenderer" ).Length > 0 ) {
            if ( !defines.Contains( "UPDATE_PLANAR3" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines + ";UPDATE_PLANAR3" );
            }
        }
        else {

           

            if ( defines.Contains( "UPDATE_PLANAR3" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace( "UPDATE_PLANAR3", "" ) );
            }

            if ( defines.Contains( "PLANAR3_PRO" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace( "PLANAR3_PRO", "" ) );
            }

            if ( defines.Contains( "PLANAR3_HDRP" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace( "PLANAR3_HDRP", "" ) );
            }

            if ( defines.Contains( "PLANAR3_LWRP" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace( "PLANAR3_LWRP", "" ) );
            }

            if ( defines.Contains( "PLANAR3_URP" ) ) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines.Replace( "PLANAR3_URP", "" ) );
            }
        }
    }



}


#endif