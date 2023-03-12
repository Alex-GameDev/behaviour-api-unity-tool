using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(EditorBehaviourRunner),editorForChildClasses: true)]
    public class EditorBehaviourRunnerEditor : UnityEditor.Editor
    {        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorBehaviourRunner runner = (EditorBehaviourRunner)target;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if(runner.Data != null && runner.Data.graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {runner.Data.graphs.Count}");
                EditorGUILayout.Space(5f);
                foreach (var graph in runner.Data.graphs)
                {
                    EditorGUILayout.LabelField($"\t- {(string.IsNullOrWhiteSpace(graph.name) ? "unnamed" : graph.name)}({graph.graph?.TypeName() ?? "null"}, {graph.nodes.Count} node(s))");
                }
                EditorGUILayout.Space(5f);
                EditorGUILayout.LabelField($"Push Perceptions: \t {runner.Data.pushPerceptions.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            bool isPartOfAPrefab = PrefabUtility.IsPartOfAnyPrefab(runner);
            bool isOnScene = runner.gameObject.scene.name != null;
            bool isOnPreviewScene = isOnScene && EditorSceneManager.IsPreviewScene(runner.gameObject.scene);

            if (!isOnPreviewScene)
            {
                if (GUILayout.Button("EDIT"))
                {

                    if (Application.isPlaying)
                    {
                        EditorWindow.GetWindow<BehaviourEditorWindow>().ShowNotification(new GUIContent("Cannot bind behaviour system on runtime"));
                        return;
                    }

                    //Debug.Log("OpenWindow editor");
                    BehaviourEditorWindow.OpenSystem(runner);
                }

                if(isPartOfAPrefab && !isOnPreviewScene)
                    EditorGUILayout.HelpBox("If you edit the behaviourSystem in a prefab instance, the original system will be override", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("BehaviourSystems that belongs to a prefab can only be edited from the prefab in asset mode.", MessageType.Warning);
            }


            GUILayout.EndVertical();
        }
    }
}
