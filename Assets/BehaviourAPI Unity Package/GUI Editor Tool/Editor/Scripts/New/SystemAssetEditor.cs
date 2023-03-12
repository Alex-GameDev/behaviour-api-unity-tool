using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityTool.Framework;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystem))]
    public class SystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            BehaviourSystem asset = target as BehaviourSystem;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if (asset.Data.graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {asset.Data.graphs.Count}");
                EditorGUILayout.Space(5f);
                foreach (var graph in asset.Data.graphs)
                {
                    EditorGUILayout.LabelField($"\t- {(string.IsNullOrWhiteSpace(graph.name) ? "unnamed" : graph.name)}({graph.graph.GetType().Name ?? "null"}, {graph.nodes.Count} data(s))");
                }
                EditorGUILayout.Space(5f);

                EditorGUILayout.LabelField($"Push Perceptions: \t {asset.Data.pushPerceptions.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            if (GUILayout.Button($"EDIT"))
            {
                if (Application.isPlaying)
                {
                    EditorWindow.GetWindow<BehaviourEditorWindow>().ShowNotification(new GUIContent("Cannot edit binded behaviour system on runtime"));
                    return;
                }

                BehaviourEditorWindow.OpenSystem(asset);
            }

            GUILayout.EndVertical();
        }
    }
}
