using BehaviourAPI.BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
namespace BehaviourAPI.UnityTool.Framework
{
    public class SerializationTests : MonoBehaviour
    {
        [ContextMenu("Test")]
        public void Test()
        {
            var system = new BehaviourSystemData();

            var graphData = new GraphData(typeof(BehaviourTree));

            NodeData n1 = new NodeData(typeof(LeafNode), Vector2.zero);
            var leafNode = n1.node as LeafNode;
            leafNode.Action = new CustomAction();
            graphData.nodes.Add(n1);

            NodeData n2 = new NodeData(typeof(LoopUntilNode), Vector2.one);
            var loop = n2.node as LoopUntilNode;
            loop.TargetStatus = Core.Status.Failure;
            n2.childIds.Add(n1.id);
            n1.parentIds.Add(n2.id);
            graphData.nodes.Add(n2);

            system.graphs.Add(graphData);

            var copy = (BehaviourSystemData)system.Clone();

            Debug.Log(copy != system);
            Debug.Log(copy.graphs[0] != system.graphs[0]);
            Debug.Log(copy.graphs[0].graph != system.graphs[0].graph);
            Debug.Log(copy.graphs[0].nodes[0] != system.graphs[0].nodes[0]);
            Debug.Log(copy.graphs[0].nodes[0].parentIds != system.graphs[0].nodes[0].parentIds);

            Debug.Log(copy.graphs[0].nodes[0].parentIds[0] == system.graphs[0].nodes[0].parentIds[0]);

            var g = copy.BuildSystem();

            Debug.Log(g.NodeList[0].GetParentAt(0) == g.NodeList[1]);
            Debug.Log(g.NodeList[1].GetChildAt(0) == g.NodeList[0]);
        }
    }
}
