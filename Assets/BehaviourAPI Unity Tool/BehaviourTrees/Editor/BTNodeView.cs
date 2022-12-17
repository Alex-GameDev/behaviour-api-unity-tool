using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class BTNodeView : NodeView
    {
        public BTNodeView(NodeAsset node) : base(node)
        {
            Debug.Log("A");
        }
    }
}
