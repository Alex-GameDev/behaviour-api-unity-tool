using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomPropertyDrawer(typeof(GraphIdentificatorAttribute))]
    public class GraphIdentificatorDrawer : PropertyDrawer
    {
        private void SetSubgraph(SerializedProperty property, GraphData data)
        {
            property.stringValue = data.id;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String) return;

            if (string.IsNullOrEmpty(property.stringValue))
            {
                if (GUILayout.Button("Assign subgraph"))
                {
                    var provider = ElementSearchWindowProvider<GraphData>.Create<GraphSearchWindowProvider>((g) => SetSubgraph(property, g));
                    provider.Data = BehaviourSystemEditorWindow.instance.System.Data;
                    SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition + BehaviourSystemEditorWindow.instance.position.position), provider);
                }
            }
            else
            {
                var subgraph = BehaviourSystemEditorWindow.instance.System.Data.graphs.Find(g => g.id == property.stringValue);
                EditorGUILayout.LabelField(subgraph?.name ?? "missing subgraph");
                if (GUILayout.Button("Remove subgraph"))
                {
                    property.stringValue = string.Empty;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
