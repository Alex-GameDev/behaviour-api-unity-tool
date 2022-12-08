using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Core;
using System.Linq;
using System;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(BehaviourSystemAsset))]
    public class BehaviourSystemAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BehaviourSystemAsset graphAsset = target as BehaviourSystemAsset;

            if (GUILayout.Button($"Edit graph"))
            {
                BehaviourGraphEditorWindow.OpenGraph(graphAsset);
            }           

        }
    }
}
