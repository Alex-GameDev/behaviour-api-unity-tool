using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.UnityToolkit.GUIDesigner.Runtime;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class BadBoyEditorRunner : EditorBehaviourRunner
    {
        public Transform[] routePoints;

        protected override void ModifyGraphs()
        {
            FindGraph("main").FindNode<LeafNode>("patrol").Action = new PathingAction(routePoints.Select(tf => tf.position).ToList(), .1f);
        }
    }

}