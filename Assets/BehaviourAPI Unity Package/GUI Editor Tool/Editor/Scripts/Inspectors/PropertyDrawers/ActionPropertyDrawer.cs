using System;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using System.Linq;
    using BehaviourAPI.Core.Actions;
    using Framework;
    using Framework.Adaptations;

    /// <summary>
    /// Default property drawer for actions
    /// </summary>
    [CustomPropertyDrawer(typeof(Action))]
    public class ActionPropertyDrawer : PropertyDrawer
    {
        private void AssignAction(SerializedProperty property, Type actionType)
        {
            if (actionType.IsSubclassOf(typeof(CompoundAction)))
            {
                var compound = (CompoundAction)Activator.CreateInstance(actionType);
                property.managedReferenceValue = new CompoundActionWrapper(compound);
            }
            else
            {
                property.managedReferenceValue = (Action)Activator.CreateInstance(actionType);
            }
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
                }
                else
                {
                    int deep = property.propertyPath.Count(c => c == '.');
                    foreach (SerializedProperty p in property)
                    {
                        if (p.propertyPath.Count(c => c == '.') == deep + 1)
                        {
                            EditorGUILayout.PropertyField(p, true);
                        }
                    }
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    [CustomPropertyDrawer(typeof(CompoundActionWrapper))]
    public class CompoundActionWrapperDrawer : PropertyDrawer
    {
        Vector2 _scrollPos;

        private void AddSubAction(SerializedProperty arrayProperty, Type perceptionType)
        {
            arrayProperty.arraySize++;
            var lastElementProperty = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).FindPropertyRelative("action");

            if (perceptionType.IsSubclassOf(typeof(CompoundAction)))
            {
                var compound = (CompoundAction)Activator.CreateInstance(perceptionType);
                lastElementProperty.managedReferenceValue = new CompoundActionWrapper(compound);
            }
            else
            {
                lastElementProperty.managedReferenceValue = (Action)Activator.CreateInstance(perceptionType);
            }

            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue == null) return;

            var compoundActionProperty = property.FindPropertyRelative("compoundAction");

            var subActionProperty = property.FindPropertyRelative("subActions");

            EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width));
            var labelsize = position.width - 50;

            EditorGUILayout.LabelField(compoundActionProperty.managedReferenceValue?.TypeName(), GUILayout.Width(220));

            if (GUILayout.Button("X"))
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndHorizontal();

            foreach (SerializedProperty childProp in compoundActionProperty)
            {
                EditorGUILayout.PropertyField(childProp, true);
            }

            if (GUILayout.Button("Add element", EditorStyles.popup))
            {
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    ElementCreatorWindowProvider.Create<ActionCreationWindow>((pType) => AddSubAction(subActionProperty, pType)));
            }

            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            EditorGUILayout.LabelField("Sub actions", centeredLabelstyle);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "window", GUILayout.MinHeight(300));

            if (subActionProperty != null)
            {
                for (int i = 0; i < subActionProperty.arraySize; i++)
                {
                    var subperception = subActionProperty.GetArrayElementAtIndex(i);
                    var p = subperception.FindPropertyRelative("action");

                    EditorGUILayout.PropertyField(p);
                    if (GUILayout.Button("Remove"))
                    {
                        subActionProperty.DeleteArrayElementAtIndex(i);
                        property.serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();
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

            if (BehaviourSystemEditorWindow.instance == null)
            {
                EditorGUILayout.HelpBox("Cannot assign subgraph outside the editor window", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Subgraph", EditorStyles.centeredGreyMiniLabel);

            var subGraphProperty = property.FindPropertyRelative("subgraphId");
            if (string.IsNullOrEmpty(subGraphProperty.stringValue))
            {
                if (GUILayout.Button("Assign subgraph"))
                {
                    var provider = ElementSearchWindowProvider<GraphData>.Create<GraphSearchWindowProvider>((g) => SetSubgraph(subGraphProperty, g));
                    provider.Data = BehaviourSystemEditorWindow.instance.System.Data;
                    SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition + BehaviourSystemEditorWindow.instance.position.position), provider);
                }
            }
            else
            {
                var subgraph = BehaviourSystemEditorWindow.instance.System.Data.graphs.Find(g => g.id == subGraphProperty.stringValue);
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
