using UnityEngine;
using UnityEditor;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    using Runtime;
    [CustomEditor(typeof(BehaviourSystem))]
    public class BehaviourSystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            BehaviourSystem system = target as BehaviourSystem;

            EditorGUILayout.Space(10f, true);
            GUIStyle centeredLabelstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.BeginVertical("- BEHAVIOUR SYSTEM -", "window");

            if (system.Data.graphs.Count != 0)
            {
                EditorGUILayout.LabelField($"Graphs: \t {system.Data.graphs.Count}");
                EditorGUILayout.Space(5f);
                foreach (var graph in system.Data.graphs)
                {
                    EditorGUILayout.LabelField($"\t- {(string.IsNullOrWhiteSpace(graph.name) ? "unnamed" : graph.name)}({graph.graph?.TypeName() ?? "null"}, {graph.nodes.Count} node(s))");
                }
                EditorGUILayout.Space(5f);
                EditorGUILayout.LabelField($"Push Perceptions: \t {system.Data.pushPerceptions.Count}");
            }
            else
            {
                EditorGUILayout.LabelField($"Empty", centeredLabelstyle);
            }

            if (GUILayout.Button($"EDIT"))
            {
                if (Application.isPlaying && !AssetDatabase.Contains(system))
                {
                    EditorWindow.GetWindow<BehaviourEditorWindow>().ShowNotification(new GUIContent("Cannot edit binded behaviour system on runtime"));
                    return;
                }

                BehaviourEditorWindow.OpenSystem(system);
            }

            GUILayout.EndVertical();
        }
    }
}
