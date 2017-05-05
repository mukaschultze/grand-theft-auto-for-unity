#pragma warning disable IDE0007 // Use implicit type
using GrandTheftAuto.Ide;
using UnityEditor;

namespace GrandTheftAuto.Editor {
    [CustomEditor(typeof(ItemDefinitionComponent))]
    public class ItemDefinitionInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            var properties = serializedObject.FindProperty("properties");

            foreach(SerializedProperty prop in properties) {
                var name = prop.FindPropertyRelative("name");
                var value = prop.FindPropertyRelative("value");

                EditorGUILayout.LabelField(name.stringValue, value.stringValue);
            }

        }

    }
}