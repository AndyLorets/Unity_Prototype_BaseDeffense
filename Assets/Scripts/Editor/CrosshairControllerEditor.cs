using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrosshairController))]
[CanEditMultipleObjects]
public class CrosshairControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_camera"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_worldY"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_sensitivity"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_boundsCenter"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_boundsSize"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Smoothing", EditorStyles.boldLabel);
        SerializedProperty useSmoothing = serializedObject.FindProperty("_useSmoothing");
        EditorGUILayout.PropertyField(useSmoothing, new GUIContent("Use Smoothing"));
        if (useSmoothing.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_smoothSpeed"), new GUIContent("Smooth Speed"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target Detection", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_detectionRadius"), new GUIContent("Detection Radius"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_meshRenderer"), new GUIContent("Mesh Renderer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_settings"), new GUIContent("Game Settings"));

        serializedObject.ApplyModifiedProperties();
    }
}
