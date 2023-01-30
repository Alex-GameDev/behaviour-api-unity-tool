using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class CyclicNodeView : NodeView
    {
        public CyclicNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/CG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/CG Node.uxml";
    }

    public class LayeredNodeView : NodeView
    {
        public LayeredNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/DAG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/DAG Node.uxml";
    }

    public class TreeNodeView : NodeView
    {
        public TreeNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/Tree Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/Tree Node.uxml";
    }
}
