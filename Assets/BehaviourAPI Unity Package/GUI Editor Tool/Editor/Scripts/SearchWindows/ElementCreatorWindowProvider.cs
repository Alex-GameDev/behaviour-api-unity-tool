using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class ElementCreatorWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        Action<Type> _callback;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>();
            var hierarchyNode = GetHierarchyNode();

            searchTreeEntries.AddGroup(hierarchyNode.name, 0);
            foreach (var subNode in hierarchyNode.Childs)
            {
                GetSubSearchTreeEntry(subNode, searchTreeEntries, 1);
            }
            return searchTreeEntries;
        }

        void GetSubSearchTreeEntry(EditorHierarchyNode node, List<SearchTreeEntry> list, int level)
        {
            if (node.Childs.Count() == 0)
            {
                list.AddEntry(node.name, level, node.Type);
            }
            else
            {
                list.AddGroup(node.name, level);
                foreach (var subNode in node.Childs)
                {
                    GetSubSearchTreeEntry(subNode, list, level + 1);
                }
            }
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as Type);
            return true;
        }

        protected abstract EditorHierarchyNode GetHierarchyNode();

        public static T Create<T>(Action<Type> callback) where T : ElementCreatorWindowProvider
        {
            T window = CreateInstance<T>();
            window._callback = callback;
            return window;
        }
    }

    /// <summary>
    /// Creation window for actions
    /// </summary>
    public class ActionCreationWindow : ElementCreatorWindowProvider
    {
        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.ActionHierarchy;
        }
    }

    /// <summary>
    /// Creation window for perceptions
    /// </summary>
    public class PerceptionCreationWindow : ElementCreatorWindowProvider
    {
        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.PerceptionHierarchy;
        }
    }

    /// <summary>
    /// Creation window for nodes
    /// </summary>
    public class NodeCreationWindow : ElementCreatorWindowProvider
    {
        private Type _adapterType;

        protected override EditorHierarchyNode GetHierarchyNode()
        {
            return BehaviourAPISettings.instance.NodeHierarchy(_adapterType);
        }

        public void SetAdapterType(Type type) => _adapterType = type;
    }
}
