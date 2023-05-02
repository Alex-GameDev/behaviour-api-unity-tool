using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class BadBoyRunner : CodeBehaviourRunner
    {
        public Transform[] routePoints;
        protected override BehaviourGraph CreateGraph()
        {
            var patrol = new PathingAction(routePoints.Select(tf => tf.position).ToList(), 3f, .1f);

            var bt = new BehaviourTree();
            var leaf = bt.CreateLeafNode(patrol);
            var root = bt.CreateDecorator<LoopNode>(leaf);
            bt.SetRootNode(root);
            RegisterGraph(bt, "main");
            return bt;
        }
    }

}