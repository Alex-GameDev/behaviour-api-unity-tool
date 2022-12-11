using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ActionInspectorView : InspectorView<ActionAsset>
    {
        public ActionInspectorView() : base("Actions", Side.Right)
        {
        }
    }
}
