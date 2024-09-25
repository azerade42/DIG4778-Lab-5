using UnityEditor;


[CustomEditor(typeof(Avoider))]
public class AvoiderEditor : Editor
{
    private SerializedProperty avoideeProperty;
    private SerializedProperty visionRangeProperty;
    private SerializedProperty agentSpeedProperty;
    private SerializedProperty showGizmosProperty;

    private void OnEnable()
    {
        avoideeProperty = serializedObject.FindProperty("objToAvoid");
        visionRangeProperty = serializedObject.FindProperty("visionRange");
        agentSpeedProperty = serializedObject.FindProperty("agentSpeed");
        showGizmosProperty = serializedObject.FindProperty("showGizmos");
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(avoideeProperty);

        if (avoideeProperty.objectReferenceValue == null)
            EditorGUILayout.HelpBox("An avoidee must be assigned.", MessageType.Warning);

        EditorGUILayout.PropertyField(visionRangeProperty);

        if (visionRangeProperty.floatValue < 0)
            EditorGUILayout.HelpBox("Range cannot be negative.", MessageType.Warning);

        EditorGUILayout.PropertyField(agentSpeedProperty);
        EditorGUILayout.PropertyField(showGizmosProperty);

        serializedObject.ApplyModifiedProperties();
    }

    
}
