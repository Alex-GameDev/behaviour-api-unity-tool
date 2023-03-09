using BehaviourAPI.UnityTool.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.New.Unity.Editor
{
    public class GraphInspector : Inspector<GraphData>
    {
        public GraphInspector() : base("graph", Side.Right)
        {
        }
    }
}
