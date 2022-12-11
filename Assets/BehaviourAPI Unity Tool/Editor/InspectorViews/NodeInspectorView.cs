using BehaviourAPI.Unity.Runtime;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class NodeInspectorView : InspectorView<NodeAsset>
    {
        public NodeInspectorView() : base("Node", Side.Left)
        {
        }
    }
}
