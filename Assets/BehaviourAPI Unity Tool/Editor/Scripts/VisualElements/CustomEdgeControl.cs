using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class CustomEdgeControl : EdgeControl
    {
        protected override void ComputeControlPoints()
        {
            base.ComputeControlPoints();
            controlPoints[1] = Vector2.LerpUnclamped(controlPoints[0], controlPoints[1], .1f);
            controlPoints[2] = Vector2.LerpUnclamped(controlPoints[3], controlPoints[2], .1f);
        }
    }
}
