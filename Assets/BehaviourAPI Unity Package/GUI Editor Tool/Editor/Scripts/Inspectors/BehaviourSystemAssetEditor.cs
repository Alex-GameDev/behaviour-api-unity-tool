using UnityEngine;
using UnityEditor;
using System.Linq;
using BehaviourAPI.Unity.Framework;
using static PlasticGui.PlasticTableColumn;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystemAsset))]
    public class BehaviourSystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviourSystemAsset asset = target as BehaviourSystemAsset;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if (asset.Graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {asset.Graphs.Count}");
                EditorGUILayout.Space(5f);
                foreach (var graph in asset.Graphs)
                {
                    EditorGUILayout.LabelField($"\t- {(string.IsNullOrWhiteSpace(graph.Name) ? "unnamed" : graph.Name)}({graph.Graph?.TypeName() ?? "null"}, {graph.Nodes.Count} node(s))");
                }
                EditorGUILayout.Space(5f);
                EditorGUILayout.LabelField($"Pull perceptions: \t {asset.PullPerceptions.Count}");
                EditorGUILayout.LabelField($"Push Perceptions: \t {asset.PushPerceptions.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            if (GUILayout.Button($"EDIT"))
            {
                if (Application.isPlaying && !AssetDatabase.Contains(asset))
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
