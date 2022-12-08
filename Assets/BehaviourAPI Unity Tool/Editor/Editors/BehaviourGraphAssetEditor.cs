using UnityEngine;
using UnityEditor;
using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystemAsset))]
    public class BehaviourGraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviourSystemAsset asset = target as BehaviourSystemAsset;

            if (GUILayout.Button($"Edit graph"))
            {
                BehaviourGraphEditorWindow.OpenGraph(asset);
            }
        }
    }
}
