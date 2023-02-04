using UnityEngine;
using UnityEditor;
using BehaviourAPI.Unity.Runtime;
using System.Linq;
using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystemAsset))]
    public class BehaviourSystemAssetEditor : UnityEditor.Editor
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
                if (Application.isPlaying && !AssetDatabase.Contains(asset))
                {
                    EditorWindow.GetWindow<SceneView>().ShowNotification(new GUIContent("Cannot edit binded behaviour system on runtime"));
                    return;
                }

                BehaviourSystemEditorWindow.OpenSystem(asset);
            }
        }
    }
}
