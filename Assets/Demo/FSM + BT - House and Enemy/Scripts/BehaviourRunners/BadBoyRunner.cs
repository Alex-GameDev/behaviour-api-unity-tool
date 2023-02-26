using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BadBoyRunner : CodeBehaviourRunner
{
    public Transform[] routePoints;
    protected override BehaviourGraph CreateGraph()
    {
        var patrol = new PathingAction(routePoints.Select(tf => tf.position).ToList(),  3f, .1f);

        var bt = new BehaviourTree();
        var leaf = bt.CreateLeafNode(patrol);
        var root = bt.CreateDecorator<IteratorNode>(leaf);
        bt.SetRootNode(root);
        RegisterGraph(bt, "main");
        return bt;
    }
}
