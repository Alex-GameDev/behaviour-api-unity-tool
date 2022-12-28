using UnityEngine;
using UnityEditor;
using BehaviourAPI.Unity.Runtime;
using System.Linq;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystemAsset))]
    public class BehaviourGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviourSystemAsset asset = target as BehaviourSystemAsset;

            if(asset.Graphs.Count > 0)
            {
                EditorGUILayout.LabelField($"Root graph: \t {asset.RootGraph.Graph.GetType().Name}");
                EditorGUILayout.LabelField($"Total graphs: \t {asset.Graphs.Count}");
                EditorGUILayout.LabelField($"Total nodes: \t {asset.Graphs.Sum(g => g.Nodes.Count)}");
            }
            else
            {
                EditorGUILayout.LabelField("Empty graph");
            }


            if (GUILayout.Button($"Edit graph"))
            {
                BehaviourGraphEditorWindow.OpenGraph(asset);
            }
        }
    }
}
