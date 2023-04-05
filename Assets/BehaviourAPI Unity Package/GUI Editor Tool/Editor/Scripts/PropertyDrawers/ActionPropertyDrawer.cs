using System;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Framework;
    using Framework.Adaptations;
    using Action = Core.Actions.Action;
    /// <summary>
    /// Default property drawer for actions
    /// </summary>
    [CustomPropertyDrawer(typeof(Action))]

    public class ActionPropertyDrawer : PropertyDrawer
    {
        private void AssignAction(SerializedProperty property, Type actionType)
        {
            property.managedReferenceValue = (Action)Activator.CreateInstance(actionType);
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
                if (GUILayout.Button("Assign action"))
                {
                    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), 
                        ElementCreatorWindowProvider.Create<ActionCreationWindow>((aType) => AssignAction(property, aType)));
                }
            }
            else
            {
                var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, position.height);
                var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);
                EditorGUI.LabelField(labelRect, property.managedReferenceValue.TypeName());
                if (GUI.Button(removeRect, "X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
        }
    }

    /// <summary>
    /// Property drawer for custom actions
    /// </summary>
    [CustomPropertyDrawer(typeof(CustomAction))]
    public class CustomActionPropertyDrawer : ActionPropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null)
            {
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);

                var lineHeight = position.height / 8;
                var space = lineHeight / 3;

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var labelRect = new Rect(position.x, position.y, position.width * 0.8f - 5, lineHeight);
                var removeRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, lineHeight);
                var startComponentRect = new Rect(position.x, position.y + lineHeight + space, position.width, lineHeight);
                var startMethodRect = new Rect(position.x, position.y + lineHeight * 2 + space, position.width, lineHeight);
                var updateComponentRect = new Rect(position.x, position.y + lineHeight * 3 + space * 2, position.width, lineHeight);
                var updateMethodRect = new Rect(position.x, position.y + lineHeight * 4 + space * 2, position.width, lineHeight);
                var stopComponentRect = new Rect(position.x, position.y + lineHeight * 5 + space * 3, position.width, lineHeight);
                var stopMethodRect = new Rect(position.x, position.y + lineHeight * 6 + space * 3, position.width, lineHeight);

                SerializedProperty startComponentProp = property.FindPropertyRelative("start.componentName");
                SerializedProperty startMethodProp = property.FindPropertyRelative("start.methodName");
                SerializedProperty updateComponentProp = property.FindPropertyRelative("update.componentName");
                SerializedProperty updateMethodProp = property.FindPropertyRelative("update.methodName");
                SerializedProperty stopComponentProp = property.FindPropertyRelative("stop.componentName");
                SerializedProperty stopMethodProp = property.FindPropertyRelative("stop.methodName");

                EditorGUI.LabelField(labelRect, "Custom action", EditorStyles.boldLabel);

                if (GUI.Button(removeRect, "X"))
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    EditorGUI.PropertyField(startComponentRect, startComponentProp, new GUIContent("Start - Component"));
                    EditorGUI.PropertyField(startMethodRect, startMethodProp, new GUIContent("Start - Method"));
                    EditorGUI.PropertyField(updateComponentRect, updateComponentProp, new GUIContent("Update - Component"));
                    EditorGUI.PropertyField(updateMethodRect, updateMethodProp, new GUIContent("Update - Method"));
                    EditorGUI.PropertyField(stopComponentRect, stopComponentProp, new GUIContent("Stop - Component"));
                    EditorGUI.PropertyField(stopMethodRect, stopMethodProp, new GUIContent("Stop - Method"));
                }

                EditorGUI.EndProperty();
                EditorGUI.indentLevel = indent;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 8f;
        }
    }

    /// <summary>
    /// Property drawer for subgraph actions
    /// </summary>
    [CustomPropertyDrawer(typeof(SubgraphAction))]
    public class SubgraphActionPropertyDrawer : ActionPropertyDrawer
    {
        private void SetSubgraph(SerializedProperty property, GraphData data)
        {
            property.stringValue = data.id;
            property.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            if (property.managedReferenceValue == null) return;

            if (CustomEditorWindow.instance == null)
            {
                EditorGUILayout.HelpBox("Cannot assign subgraph outside the editor window", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Subgraph", EditorStyles.centeredGreyMiniLabel);

            var subGraphProperty = property.FindPropertyRelative("subgraphId");
            if(string.IsNullOrEmpty(subGraphProperty.stringValue))
            {
                if (GUILayout.Button("Assign subgraph"))
                {
                    var provider = ElementSearchWindowProvider<GraphData>.Create<GraphSearchWindowProvider>((g) => SetSubgraph(subGraphProperty, g));
                    provider.Data = CustomEditorWindow.instance.System.Data;
                    SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition + CustomEditorWindow.instance.position.position), provider);
                }
            }
            else
            {
                var subgraph = CustomEditorWindow.instance.System.Data.graphs.Find(g => g.id == subGraphProperty.stringValue);
                EditorGUILayout.LabelField(subgraph?.name ?? "missing subgraph");
                if (GUILayout.Button("Remove subgraph"))
                {
                    subGraphProperty.stringValue = string.Empty;
                    subGraphProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
