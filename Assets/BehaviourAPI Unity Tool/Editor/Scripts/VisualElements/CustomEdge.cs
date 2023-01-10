using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class CustomEdge : Edge
    {
        protected override EdgeControl CreateEdgeControl()
        {
            return new CustomEdgeControl();
        }
    }
}
