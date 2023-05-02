using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class BadBoyEditorRunner : EditorBehaviourRunner
    {
        public Transform[] routePoints;

        protected override void ModifyGraphs()
        {
            FindGraph("main").FindNode<LeafNode>("patrol").Action = new PathingAction(routePoints.Select(tf => tf.position).ToList(), 3f, .1f);
        }
    }

}