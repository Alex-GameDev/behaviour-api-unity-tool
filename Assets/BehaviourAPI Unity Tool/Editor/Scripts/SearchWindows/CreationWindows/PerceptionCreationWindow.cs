using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class PerceptionCreationWindow : CreationWindow
    {
        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.PerceptionHierarchy;
        }
    }
}
