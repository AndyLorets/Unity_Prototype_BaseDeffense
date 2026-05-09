using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrosshairController))]
[CanEditMultipleObjects]
public class CrosshairControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Основные настройки
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_camera"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_worldY"));

        // НОВЫЙ БЛОК: Следование за игроком
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Movement & Following", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_followTarget"), new GUIContent("Follow Target", "Объект (Игрок), за которым будет следовать прицел и его границы"));

        // Ввод
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_sensitivity"), new GUIContent("Sensitivity (Delta)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_speed"), new GUIContent("Speed (Velocity)"));

        // Границы
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_boundsCenter"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_boundsSize"));

        // Сглаживание
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

        // Детекция целей
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Target Detection", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_detectionRadius"), new GUIContent("Detection Radius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetOffset"), new GUIContent("Target Offset (3D only)"));

        // Визуал
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_meshRenderer"), new GUIContent("Mesh Renderer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_settings"), new GUIContent("Game Settings"));

        serializedObject.ApplyModifiedProperties();
    }
}