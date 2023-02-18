using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(EditorBehaviourRunner))]
    public class EditorBehaviourRunnerEditor : UnityEditor.Editor
    {
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorBehaviourRunner runner = (EditorBehaviourRunner)target;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if(runner.Graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {runner.Graphs.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            if (GUILayout.Button("EDIT"))
            {

                if (Application.isPlaying)
                {
                    EditorWindow.GetWindow<BehaviourSystemEditorWindow>().ShowNotification(new GUIContent("Cannot bind behaviour system on runtime"));
                    return;
                }

                //Debug.Log("Open editor");
                BehaviourEditorWindow.OpenSystem(runner);
            }

            GUILayout.EndVertical();
        }
    }
}
