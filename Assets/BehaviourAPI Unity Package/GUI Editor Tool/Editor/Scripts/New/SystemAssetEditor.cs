using BehaviourAPI.UnityTool.Framework;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.New.Unity.Editor
{
    [CustomEditor(typeof(SystemAsset))]
    public class SystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SystemAsset asset = target as SystemAsset;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if (asset.data.graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {asset.data.graphs.Count}");
                EditorGUILayout.Space(5f);
                foreach (var graph in asset.data.graphs)
                {
                    //EditorGUILayout.LabelField($"\t- {(string.IsNullOrWhiteSpace(graph.name) ? "unnamed" : graph.name)}({graph.graph.GetType().Name ?? "null"}, {graph.nodes.Count} data(s))");
                }
                EditorGUILayout.Space(5f);
                //EditorGUILayout.LabelField($"Pull perceptions: \t {asset.PullPerceptions.Count}");
                EditorGUILayout.LabelField($"Push Perceptions: \t {asset.data.pushPerception.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            if (GUILayout.Button($"EDIT"))
            {
                if (Application.isPlaying)
                {
                    EditorWindow.GetWindow<EditorWindow>().ShowNotification(new GUIContent("Cannot edit binded behaviour system on runtime"));
                    return;
                }

                EditorWindow.Open(asset);
            }

            GUILayout.EndVertical();
        }
    }
}
