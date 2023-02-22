using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;
using Node = BehaviourAPI.Core.Node;

public class Duplicator
{

    Dictionary<GraphAsset, GraphAsset> graphCopyMap = new Dictionary<GraphAsset, GraphAsset>();
    Dictionary<NodeAsset, NodeAsset> nodeCopyMap = new Dictionary<NodeAsset, NodeAsset>();

    Dictionary<PerceptionAsset, PerceptionAsset> perceptionCopyMap = new Dictionary<PerceptionAsset, PerceptionAsset>();

    /// <summary>
    /// generates a copy of a behaviourSystemAsset
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public BehaviourSystemAsset Duplicate(BehaviourSystemAsset asset)
    {
        // Copy perceptions:
        List<PerceptionAsset> perceptionAssetList = new List<PerceptionAsset>();

        foreach(var perception in asset.PullPerceptions)
        {
            var p = CopyPerceptionAsset(perception);
            perceptionAssetList.Add(p);
        }

        // Copy Graphs:
        List<GraphAsset> graphAssets = new List<GraphAsset>();
        foreach (var graph in asset.Graphs)
        {
            var graphCopy = ScriptableObject.CreateInstance<GraphAsset>();
            graphCopy.Graph = (BehaviourGraph)graph.Graph.Clone();
            graphAssets.Add(graphCopy);
            graphCopyMap[graph] = graphCopy;
        }

        // Copy nodes:
        foreach (var graph in asset.Graphs)
        {
            CopyGraphAsset(graph, graphCopyMap[graph]);
        }

        // Copy pushPerceptions:
        List<PushPerceptionAsset> pushPerceptionAssetList = new List<PushPerceptionAsset>();
        foreach (var pushPerception in asset.PushPerceptions)
        {
            var p = PushPerceptionAsset.Create(pushPerception.Name);
            foreach(var target in pushPerception.Targets)
            {
                p.Targets.Add(nodeCopyMap[target]);
            }
            pushPerceptionAssetList.Add(p);
        }

        return BehaviourSystemAsset.CreateSystem(graphAssets, perceptionAssetList, pushPerceptionAssetList);
    }

    void CopyGraphAsset(GraphAsset original, GraphAsset copy)
    {
        foreach (var node in original.Nodes)
        {
            var nodeClone = ScriptableObject.CreateInstance<NodeAsset>();
            nodeClone.Node = (Node)node.Node.Clone();
            nodeClone.Position = node.Position;
            nodeClone.Name = node.Name;

            if (nodeClone.Node == node.Node) Debug.LogWarning("EEEEEEEEE");

            copy.Nodes.Add(nodeClone);
            nodeCopyMap[node] = nodeClone;
        }

        foreach (var node in original.Nodes)
        {
            var nodeCopy = nodeCopyMap[node];
            for (int i = 0; i < node.Parents.Count; i++)
            {
                nodeCopy.Parents.Add(nodeCopyMap[node.Parents[i]]);
            }

            for (int i = 0; i < node.Childs.Count; i++)
            {
                nodeCopy.Childs.Add(nodeCopyMap[node.Childs[i]]);
            }

            if (nodeCopy.Childs.Count != node.Childs.Count) Debug.LogWarning("E");
            if (nodeCopy.Parents.Count != node.Parents.Count) Debug.LogWarning("E");

            // Fix the action:
            if (node.Node is IActionAssignable originalHandler && 
                nodeCopy.Node is IActionAssignable copyHandler)
            {
                copyHandler.ActionReference = (Action)originalHandler.ActionReference.Clone();

                if(originalHandler.ActionReference is SubgraphAction sgo &&
                    copyHandler.ActionReference is SubgraphAction sgc)
                {
                    sgc.Subgraph = graphCopyMap[sgo.Subgraph];
                }
            }

            // Fix the perception reference
            if (node.Node is IPerceptionAssignable originalPHandler &&
                nodeCopy.Node is IPerceptionAssignable copyPHandler)
            {
                copyPHandler.PerceptionReference = perceptionCopyMap[originalPHandler.PerceptionReference];
            }
        }

        if(original.Nodes.Count != copy.Nodes.Count) Debug.LogWarning("E");
    }

    PerceptionAsset CopyPerceptionAsset(PerceptionAsset original)
    {
        if(perceptionCopyMap.TryGetValue(original, out var copy))
        {
            return copy;
        }

        PerceptionAsset perceptionAsset;

        var perception = (Perception)original.perception.Clone();
        if (original is StatusPerceptionAsset)
        {
            perceptionAsset = ScriptableObject.CreateInstance<StatusPerceptionAsset>(); 
        }
        else if(original is CompoundPerceptionAsset cpa)
        {
            var copyAsset = ScriptableObject.CreateInstance<CompoundPerceptionAsset>();
            copyAsset.subperceptions = cpa.subperceptions.Select(sub => CopyPerceptionAsset(sub)).ToList();
            perceptionAsset = copyAsset;
        }
        else
        {
            perceptionAsset = ScriptableObject.CreateInstance<PerceptionAsset>();
        }

        perceptionAsset.perception = perception;
        perceptionCopyMap[original] = perceptionAsset;
        return perceptionAsset;       
    }
}
