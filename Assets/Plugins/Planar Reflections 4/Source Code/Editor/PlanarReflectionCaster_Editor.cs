namespace PlanarReflections4 {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor( typeof( PlanarReflectionCaster ) )]
    public class PlanarReflectionCaster_Editor : Editor {

        public GUISkin pidiSkin2;

        public Texture2D reflectionsLogo;

        bool[] _folds = new bool[16];

        bool supportFold;

        private void OnEnable() {



            if ( ((PlanarReflectionCaster)target).GetComponent<Renderer>() ) {
                _folds = new bool[( (PlanarReflectionCaster)target ).GetComponent<Renderer>().sharedMaterials.Length];
            }

        }


        public override void OnInspectorGUI() {

            var rend = ( (PlanarReflectionCaster)target ).GetComponent<Renderer>();
            GUI.color = EditorGUIUtility.isProSkin ? new Color( 0.1f, 0.1f, 0.15f, 1 ) : new Color( 0.5f, 0.5f, 0.6f );
            GUILayout.BeginVertical( EditorStyles.helpBox );
            GUI.color = Color.white;

            GUILayout.Space( 8 );

            AssetLogoAndVersion();

            GUILayout.Space( 4 );

            EditorGUILayout.PropertyField( serializedObject.FindProperty( "castFromRenderer" ), new GUIContent( "Cast From Renderer", "The Planar Reflection Renderer from which this Caster will take the reflections" ) );

            GUILayout.Space( 8 );

            for ( int i = 0; i < _folds.Length; i++ ) {

                if ( BeginCenteredGroup( rend.sharedMaterials[i].name, ref _folds[i] ) ) {
                    GUILayout.Space( 8 );

                    Toggle( new GUIContent( "Reflection Color", "Wether this material requires a reflection color texture" ), serializedObject, "castReflection", 1, i );
                    Toggle( new GUIContent( "Reflection Depth", "Wether this material requires a reflection depth texture" ), serializedObject, "castDepth", 1, i );
                    Toggle( new GUIContent( "Reflection Fog", "Wether this material requires a reflection fog texture" ), serializedObject, "castFog", 1, i );

                    GUILayout.Space( 4 );


                    var blurProp = serializedObject.FindProperty( "blurSettings" ).GetArrayElementAtIndex( i );

                    Toggle( new GUIContent( "Blur Pass", "Enables a blur pass on the reflection texture that can be used internally by reflection-compatible shaders" ), blurProp.FindPropertyRelative( "useBlur" ), 1 );


                    if ( blurProp.FindPropertyRelative( "useBlur" ).boolValue ) {
                        GUILayout.Space( 8 );
                        CenteredLabel( "Blur Settings" );
                        GUILayout.Space( 8 );
                        Toggle( new GUIContent( "Fast Blur", "A very simple, faked blur approximation designed for low end platforms" ), blurProp.FindPropertyRelative( "forceFakeBlur" ), 1 );
                        
                        PopupField( new GUIContent( "Blur Pass Mode", "The way in which the blurred reflection will be sent to the material\n\nFinal Output - Blurs the reflection itself\nSeparate Pass - Stores the blurred reflection on its own texture (_BlurReflectionTex)" ), blurProp.FindPropertyRelative( "blurPassMode" ), new string[] { "Final Result", "Separate Pass" } );
                        EditorGUILayout.PropertyField( blurProp.FindPropertyRelative( "blurRadius" ), new GUIContent("Blur Radius","The spread of the blur effect") );
                        EditorGUILayout.PropertyField( blurProp.FindPropertyRelative( "blurDownscale" ), new GUIContent("Blur Downscale","The downscaling applied to the blurred reflection. Increases the overall blurriness") );
                    }

                    GUILayout.Space( 16 );
                }
                EndCenteredGroup();

            }
            if ( BeginCenteredGroup( "Help & Support", ref supportFold ) ) {

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

            GUILayout.Space( 16 );

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

            var lStyle = new GUIStyle();
            lStyle.fontStyle = FontStyle.Italic;
            lStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            lStyle.fontSize = 8;

            GUILayout.Label( "Copyright© 2017-2021,   Jorge Pinal N.", lStyle );

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.Space( 24 );

            GUILayout.EndVertical();

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
            tempStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

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

            if ( GUILayout.Button( label, EditorGUIUtility.isProSkin ? pidiSkin2.customStyles[0] : EditorGUIUtility.GetBuiltinSkin( EditorSkin.Inspector ).button ) ) {
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




        public static void PopupField( GUIContent label, SerializedProperty inValue, string[] options ) {


            GUILayout.BeginHorizontal();

            var tempStyle = new GUIStyle();
            EditorGUILayout.PrefixLabel( label );


          
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



        private static void Toggle( GUIContent label, SerializedObject serializedObject, string propertyID, int toggleType = 0, int atIndex = -1 ) {


            GUILayout.BeginHorizontal();

            var inValue = serializedObject.FindProperty( propertyID );

            if (atIndex > -1 ) {
                inValue = inValue.GetArrayElementAtIndex( atIndex );
            }

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



        private static void Toggle( GUIContent label, SerializedProperty inValue, int toggleType = 0 ) {


            GUILayout.BeginHorizontal();


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