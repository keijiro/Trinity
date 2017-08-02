using UnityEngine;
using UnityEditor;

namespace Klak.Wiring
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SprayOut))]
    public class SprayOutEditor : Editor
    {
        SerializedProperty _targets;

        void OnEnable()
        {
            _targets = serializedObject.FindProperty("_targets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targets, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
