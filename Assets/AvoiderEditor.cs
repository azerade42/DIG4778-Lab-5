using UnityEditor;


[CustomEditor(typeof(Avoider))]
public class AvoiderEditor : Editor
{
    private SerializedProperty avoideeProperty;
    private SerializedProperty visionRangeProperty;
    private SerializedProperty agentSpeedProperty;
    private SerializedProperty showGizmosProperty;
    private SerializedProperty sampleTickRateProperty;

    private void OnEnable()
    {
        avoideeProperty = serializedObject.FindProperty("objToAvoid");
        visionRangeProperty = serializedObject.FindProperty("visionRange");
        agentSpeedProperty = serializedObject.FindProperty("agentSpeed");
        sampleTickRateProperty = serializedObject.FindProperty("sampleTickRate");
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
        if (agentSpeedProperty.floatValue < 0)
            EditorGUILayout.HelpBox("Agent speed cannot be negative.", MessageType.Warning);

        EditorGUILayout.PropertyField(sampleTickRateProperty);
        if (sampleTickRateProperty.floatValue < 0)
            EditorGUILayout.HelpBox("Tick rate cannot be negative.", MessageType.Warning);

        EditorGUILayout.PropertyField(showGizmosProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
