using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class NodeInspectorView : Inspector<NodeData>
    {
        public NodeInspectorView() : base("Node", Side.Left)
        {
        }
    }
}
