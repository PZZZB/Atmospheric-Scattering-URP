using UnityEditor;

namespace AtmosphericScattering.Editor
{
    /*
    [CustomEditor(typeof(RuntimeSkySetting))]
    public class ScatteringSettingEditor : UnityEditor.Editor
    {
        private SerializedProperty lutUpdateMode;
        private SerializedProperty mainLight;
        private SerializedProperty lightFromOuterSpace;
        
        private SerializedProperty planetRadius;
        private SerializedProperty atmosphereHeight;
        private SerializedProperty surfaceHeight;
        private SerializedProperty rDensityScale;
        private SerializedProperty mDensityScale;

        private SerializedProperty sunIntensity;
        private SerializedProperty sunMieG;
        private SerializedProperty computeShader;

        private SerializedProperty showFrustumCorners;
        private SerializedProperty mainLightColor;
        private SerializedProperty ambientColor;
        private SerializedProperty intergateCPDensityLUT;
        private SerializedProperty sunOnSurfaceLUT;
        private SerializedProperty ambientLUT;
        private SerializedProperty inScatteringLUT;
        
        private void OnEnable()
        {
            lutUpdateMode = serializedObject.FindProperty("lutUpdateMode");
            mainLight = serializedObject.FindProperty("mainLight");
            lightFromOuterSpace = serializedObject.FindProperty("lightFroOuterSpace");

            planetRadius = serializedObject.FindProperty("planetRadius");
            atmosphereHeight = serializedObject.FindProperty("atmosphereHeight");
            surfaceHeight = serializedObject.FindProperty("surfaceHeight");

            rDensityScale = serializedObject.FindProperty("rDensity");
            mDensityScale = serializedObject.FindProperty("mDensity");

            sunIntensity = serializedObject.FindProperty("sunIntensity");
            sunMieG = serializedObject.FindProperty("sunMieG");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug/Output",EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mainLightColor);
        }
        
        
    }
    */
}