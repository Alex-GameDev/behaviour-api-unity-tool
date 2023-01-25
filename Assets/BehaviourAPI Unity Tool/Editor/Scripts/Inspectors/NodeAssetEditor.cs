using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomEditor(typeof(NodeAsset))]
    public class NodeAssetEditor : UnityEditor.Editor
    {


        public override VisualElement CreateInspectorGUI()
        {
            return new Label("This is a Label in a Custom Editor");
        }
    }
}
