using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class CustomGraphDrawer
    {
        public abstract NodeView DrawNode(Node node);
    }
}
