using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class ActionCreationWindow : CreationWindow
    {
        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.ActionHierarchy;
        }
    }
}
