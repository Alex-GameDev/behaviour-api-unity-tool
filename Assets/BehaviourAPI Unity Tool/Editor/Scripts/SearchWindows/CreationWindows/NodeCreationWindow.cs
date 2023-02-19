using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Creates a window menu to select a type
    /// </summary>
    public class NodeCreationWindow : CreationWindow
    {
        private Type _adapterType;

        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.NodeHierarchy(_adapterType);
        }

        public void SetAdapterType(Type type) => _adapterType = type;
    }
}