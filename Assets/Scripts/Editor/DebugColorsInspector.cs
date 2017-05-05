#pragma warning disable IDE0007 // Use implicit type
using GrandTheftAuto.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace GrandTheftAuto.Editor {
    [CustomEditor(typeof(LayerDebugColors))]
    public class DebugColorsInspector : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var colorsProperties = serializedObject.FindProperty("colors");

            for(var i = 0; i < 32; i++) {
                var layer = new Layer(i);
                var colorProp = colorsProperties.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(colorProp, new GUIContent(string.Format("{0} {1}", layer.Value, layer.Name)));

            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}