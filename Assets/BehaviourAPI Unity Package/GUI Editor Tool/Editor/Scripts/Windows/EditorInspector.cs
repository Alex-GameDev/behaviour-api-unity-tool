using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Inspector attached in the window
    /// </summary>
    public class EditorInspector : VisualElement
    {
        private static readonly string k_MainPropertyName = "data";
        private static readonly string k_GraphsPropertyName = "graphs";
        private static readonly string k_PushPerceptionsPropertyName = "pushPerceptions";
        private static readonly string k_NodesPropertyName = "nodes";

        private static readonly string[] k_InspectorTabNames = new string[]
        {
            "Graph",
            "Nodes"
        };

        private SerializedProperty rootProperty;
        private SerializedProperty graphsProperty;
        private SerializedProperty pushPerceptionsProperty;

        private SerializedProperty selectedGraphProperty;
        private SerializedProperty selectedNodeProperty;
        private SerializedProperty selectedPushPerceptionProperty;

        Vector2 pushPerceptionScrollPos, pushPerceptionTargetScrollPos;

        public int InspectorMode = 0;
        private int selectedGraphId = -1;
        private int selectedNodeId = -1;


        public void UpdateSystem(SerializedObject serializedObject)
        {
            rootProperty = serializedObject.FindProperty(k_MainPropertyName);
            graphsProperty = rootProperty.FindPropertyRelative(k_GraphsPropertyName);
            pushPerceptionsProperty = rootProperty.FindPropertyRelative(k_PushPerceptionsPropertyName);
        }

        public void OnGUIHandler(SerializedObject serializedObject)
        {
            if (selectedGraphId >= 0 && selectedGraphId < graphsProperty.arraySize)
            {
                selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphId);
                var nodeListProperty = selectedGraphProperty.FindPropertyRelative(k_NodesPropertyName);
                if (selectedNodeId >= 0 && selectedNodeId < nodeListProperty.arraySize)
                {
                    selectedNodeProperty = nodeListProperty.GetArrayElementAtIndex(selectedNodeId);
                }
                else selectedNodeProperty = null;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            DrawInspectorTab();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        private void DrawInspectorTab()
        {

            InspectorMode = GUILayout.Toolbar(InspectorMode, k_InspectorTabNames);

            switch (InspectorMode)
            {
                case 0:

                    break;
                case 1:
                    DisplayCurrentGraphProperty();
                    DisplayPushPerceptionListProperty();
                    break;
            }
        }

        private void DisplayCurrentGraphProperty()
        {
            if (selectedGraphProperty != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                DrawField(selectedGraphProperty.FindPropertyRelative("name"));
                EditorGUILayout.Space(10);
                
                foreach(SerializedProperty p in selectedGraphProperty.FindPropertyRelative("graph"))
                {
                    DrawField(p);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            }
            else
            {
                EditorGUILayout.HelpBox("This system has no graphs. Create a graph to start editing", MessageType.Info);
            }
            EditorGUILayout.Space(20);
        }

        private void DisplayPushPerceptionListProperty()
        {
            EditorGUILayout.LabelField("Push perceptions", EditorStyles.centeredGreyMiniLabel);

            if (pushPerceptionsProperty == null) return;

            pushPerceptionScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionScrollPos, "window", GUILayout.MinHeight(100));

            for (int i = 0; i < pushPerceptionsProperty.arraySize; i++)
            {
                SerializedProperty p = pushPerceptionsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(p.displayName, GUILayout.ExpandWidth(true)))
                {
                    selectedPushPerceptionProperty = p;
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    selectedPushPerceptionProperty = null;
                    pushPerceptionsProperty.DeleteArrayElementAtIndex(i);

                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create push perception"))
            {
                pushPerceptionsProperty.InsertArrayElementAtIndex(pushPerceptionsProperty.arraySize);
            }

            if (selectedPushPerceptionProperty == null) return;

            DrawField(selectedPushPerceptionProperty.FindPropertyRelative("name"));

            pushPerceptionTargetScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionTargetScrollPos, "window", GUILayout.MinHeight(100));

            SerializedProperty targetNodesProperty = selectedPushPerceptionProperty.FindPropertyRelative("targetNodeIds");

            for (int i = 0; i < targetNodesProperty.arraySize; i++)
            {
                SerializedProperty p = targetNodesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(p.stringValue);

                if(GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    targetNodesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);
    }
}
