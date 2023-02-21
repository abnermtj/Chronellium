namespace PlanarReflections4 {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PlanarReflectionRenderer))]
    public class PlanarReflectionRenderer_Editor : Editor {


        public GUISkin pidiSkin2;

        public Texture2D reflectionsLogo;

        bool[] _folds = new bool[16];

        public override void OnInspectorGUI() {

            GUI.color = EditorGUIUtility.isProSkin ? new Color( 0.1f, 0.1f, 0.15f, 1 ) : new Color( 0.5f, 0.5f, 0.6f );
            GUILayout.BeginVertical( EditorStyles.helpBox );
            GUI.color = Color.white;

            GUILayout.Space( 8 );

            AssetLogoAndVersion(); 

            GUILayout.Space( 4 );


            if ( BeginCenteredGroup("Main Settings", ref _folds[0] ) ) {
                GUILayout.Space( 8 );

                CenteredLabel( "Basic Settings" );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty("_settings.reflectLayers"), new GUIContent( "Reflect Layers" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.camerasTag" ), new GUIContent( "Camera's Tag", "Filters the in-game cameras so that a reflection is generated and displayed only for those with the given tag" ) );
                Toggle( new GUIContent( "Render Shadows", "Whether this reflection will display shadows or not" ), serializedObject, "_settings.renderShadows", 2 );

                GUILayout.Space( 8 );

                Toggle( new GUIContent( "Preview Reflection", "Enables or disables a preview reflection plane in the Scene view to show the reflection and its properties regardless of if it has been assigned to a Caster" ), serializedObject, "showPreviewReflector", 1 );

                GUILayout.Space( 4 );

                Toggle( new GUIContent( "Accurate Matrix", "When enabled, the reflection will use a custom oblique projection matrix to avoid clipping and present more accurate reflections.\nIt may, however, interfere with some PostFX and projection-dependant features" ), serializedObject, "_settings.accurateMatrix", 1 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.nearClipPlane" ), new GUIContent( "Near Clip Plane" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.farClipPlane" ), new GUIContent( "Far Clip Plane" ) );


                GUILayout.Space( 8 );

                CenteredLabel( "Reflection Features" );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "externalReflectionTex" ), new GUIContent( "External Ref. Texture (Optional)", "An optional RenderTexture asset to which the reflection will be rendered to, instead of the internally managed resources. May not work accurately with multiple in-game cameras" ), GUILayout.Height( EditorGUIUtility.singleLineHeight ) );

                if ( serializedObject.FindProperty( "_settings.renderDepth" ).boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "externalReflectionDepth" ), new GUIContent( "External Ref. Depth (Optional)", "An optional RenderTexture asset to which the reflection's depth will be rendered to, instead of the internally managed resources. May not work accurately with multiple in-game cameras. Must be in 'Depth' texture format." ), GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
                }

                GUILayout.Space( 4 );

                Toggle( new GUIContent( "PostFX Support", "Allows the reflection to render Post Process FX with its own custom settings, useful to display ambient occlusion, bloom or other effects within the reflection itself" ) , serializedObject, "_settings.usePostFX", 1 );
                Toggle( new GUIContent( "Reflection Depth", "Whether this reflection will render its own depth texture or not" ), serializedObject, "_settings.renderDepth", 1 );
                Toggle( new GUIContent( "Reflection Fog", "Whether this reflection will render its own fog pass or not" ), serializedObject, "_settings.renderFog", 1 );
                
                if ( serializedObject.FindProperty( "_settings.renderFog" ).boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.fogRendererIndex" ), new GUIContent( "Fog Renderer Index" ) );
                }
                
                Toggle( new GUIContent( "Clear to Color", "Controls the clear flags of the reflection so that instead of reflecting the skybox it reflects a solid color" ), serializedObject, "_settings.clearToColor", 2 );
                
                if ( serializedObject.FindProperty("_settings.clearToColor").boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.backgroundColor" ), new GUIContent( "Background Color", "The color that this reflection will use as a solid background instead of the skybox" ) );
                }
                else {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.customSkybox" ), new GUIContent( "Custom Skybox" ) );
                }

                GUILayout.Space( 8 );

                CenteredLabel( "Reflection Quality" );

                GUILayout.Space( 8 );                

                Toggle( new GUIContent( "Screen Based Resolution", "The resolution of the reflection will be determined by the resolution of the screen / game's window" ), serializedObject, "_settings.screenBasedResolution", 1 );

                if ( !serializedObject.FindProperty( "_settings.screenBasedResolution" ).boolValue ) {
                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.explicitResolution" ), new GUIContent( "Base Resolution" ) );
                }

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.outputResolutionMultiplier" ), new GUIContent( "Resolution Multiplier", "The factor by which the reflection's resolution will be multiplied" ) );

                GUILayout.Space( 4 );

                Toggle( new GUIContent( "HDR Reflection", "Whether the reflection texture will use an HDRP compatible format" ), serializedObject, "_settings.forceHDR", 1 );
                Toggle( new GUIContent( "Mip Maps", "Whether the reflection texture will use mip maps or not" ), serializedObject, "_settings.useMipMaps", 1 );
                Toggle( new GUIContent( "Anti-aliasing", "Whether the reflection texture will use anti-aliasing or not" ), serializedObject, "_settings.useAntialiasing", 1 );

                GUILayout.Space( 16 );
            }
            EndCenteredGroup();


            if ( BeginCenteredGroup("Optimization Settings", ref _folds[1] ) ) {
                GUILayout.Space( 8 );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.customLODBias" ), new GUIContent( "Custom LOD Bias","Further adjusts the LOD bias of the game in order to render lower quality models for the reflections if necessary" ) );
                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.maxLODLevel" ), new GUIContent( "Max. LOD Level", "The maximum LOD level allowed in reflections" ) );

                GUILayout.Space( 8 );

                EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.reflectionFramerate" ), new GUIContent( "Reflection's Framerate", "The maximum framerate of this reflection. Set to 0 to remove any FPS cap and render the reflection in sync with the in-game cameras" ) );

                GUILayout.Space( 16 );
            }
            EndCenteredGroup();
            
            if ( serializedObject.FindProperty( "_settings.usePostFX" ).boolValue ) {

                if ( BeginCenteredGroup( "PostFX Settings", ref _folds[2] ) ){

                    GUILayout.Space( 8 );

                    EditorGUILayout.PropertyField( serializedObject.FindProperty( "_settings.postFXVolumeMask" ), new GUIContent( "Post FX Volume Mask", "The layer mask for the PostFX volumes with which this reflection will interact" ) );

                    GUILayout.Space( 16 );

                }
                EndCenteredGroup();
            }
            
            if (BeginCenteredGroup("Help & Support", ref _folds[3] ) ) {

                GUILayout.Space( 16 );

                CenteredLabel( "Support & Assistance" );
                GUILayout.Space( 10 );

                EditorGUILayout.HelpBox( "Please make sure to include the following information with your request :\n - Invoice number\n- Unity version used\n- Universal RP / HDRP version used (if any)\n- Target platform\n - Screenshots of the PlanarReflectionRenderer component and its settings\n - Steps to reproduce the issue.\n\nOur support service usually takes 2-4 business days to reply, so please be patient. We always reply to all emails and support requests as soon as possible.", MessageType.Info );

                GUILayout.Space( 8 );
                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                GUILayout.Label( "For support, contact us at : support@irreverent-software.com" );
                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

                GUILayout.Space( 24 );

                if ( CenteredButton( "Online Documentation", 500 ) ) {
                    Help.BrowseURL( "https://irreverent-software.com/docs/pidi-planar-reflections-4/" );
                }
                GUILayout.Space( 16 );

            }
            EndCenteredGroup();

            GUILayout.Space( 8 );


            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

            var lStyle = new GUIStyle();
            lStyle.fontStyle = FontStyle.Italic;
            lStyle.normal.textColor = EditorGUIUtility.isProSkin?Color.white:Color.black;
            lStyle.fontSize = 8;

            GUILayout.Label( "Copyright© 2017-2021,   Jorge Pinal N.", lStyle );

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.Space( 24 );

            GUILayout.EndVertical();


            if ( serializedObject.hasModifiedProperties ) {
                ( (PlanarReflectionRenderer)target ).ApplySettings();
            }

            serializedObject.ApplyModifiedProperties();

        }



        private void AssetLogoAndVersion() {

            GUILayout.BeginVertical( reflectionsLogo, pidiSkin2 ? pidiSkin2.customStyles[1] : null );
            GUILayout.Space( 45 );
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label( "v4.0.7", pidiSkin2.customStyles[2] );
            GUILayout.Space( 6 );
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        void CenteredLabel( string label ) {

            
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

            var tempStyle = new GUIStyle();
            tempStyle.fontStyle = FontStyle.Bold;
            tempStyle.normal.textColor = EditorGUIUtility.isProSkin?Color.white:Color.black;

            GUILayout.Label( label, tempStyle );

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

        }


        bool CenteredButton( string label, float width = 400 ) {

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var btn = GUILayout.Button( label, GUILayout.MaxWidth( width ) );
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            return btn;
        }

        private bool BeginCenteredGroup( string label, ref bool groupFoldState ) {

            if ( GUILayout.Button( label, EditorGUIUtility.isProSkin?pidiSkin2.customStyles[0]:EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).button ) ) {
                groupFoldState = !groupFoldState;
            }
            GUILayout.BeginHorizontal(); GUILayout.Space( 12 );
            GUILayout.BeginVertical();
            return groupFoldState;
        }


        private void EndCenteredGroup() {
            GUILayout.EndVertical();
            GUILayout.Space( 12 );
            GUILayout.EndHorizontal();
        }




        public static void PopupField( GUIContent label, SerializedObject serializedObject, string propertyID, string[] options ) {

           
            GUILayout.BeginHorizontal();


            var tempStyle = new GUIStyle();
            EditorGUILayout.PrefixLabel( label );


            var inValue = serializedObject.FindProperty( propertyID );

            if ( inValue.hasMultipleDifferentValues ) {
                var result = EditorGUILayout.Popup( -1, options );

                if ( result > -1 ) {
                    inValue.intValue = result;
                }
            }
            else {
                inValue.intValue = EditorGUILayout.Popup( inValue.intValue, options );
            }

            GUILayout.EndHorizontal();

        }



        private static void Toggle( GUIContent label, SerializedObject serializedObject, string propertyID, int toggleType = 0 ) {

            
            GUILayout.BeginHorizontal();

            var inValue = serializedObject.FindProperty( propertyID );

            switch ( toggleType ) {

                case 0:
                    EditorGUILayout.PropertyField( inValue, label );
                    break;

                case 1:
                    if ( inValue.hasMultipleDifferentValues ) {
                        var result = EditorGUILayout.Popup( label, -1, new string[] { "Enabled", "Disabled" } );

                        if ( result > -1 ) {
                            inValue.boolValue = result == 0;
                        }
                    }
                    else {
                        inValue.boolValue = EditorGUILayout.Popup( label, inValue.boolValue ? 0 : 1, new string[] { "Enabled", "Disabled" } ) == 0;
                    }
                    break;

                case 2:
                    if ( inValue.hasMultipleDifferentValues ) {
                        var result = EditorGUILayout.Popup( label, -1, new string[] { "True", "False" } );

                        if ( result > -1 ) {
                            inValue.boolValue = result == 0;
                        }
                    }
                    else {
                        inValue.boolValue = EditorGUILayout.Popup( label, inValue.boolValue ? 0 : 1, new string[] { "True", "False" } ) == 0;
                    }
                    break;

            }

            GUILayout.EndHorizontal();
        }



    }

}