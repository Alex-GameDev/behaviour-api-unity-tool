using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphInspectorView : InspectorView<GraphAsset>
    {
        public BehaviourGraphInspectorView() : base("Graph", Side.Right)
        {
        }     
    }
}
