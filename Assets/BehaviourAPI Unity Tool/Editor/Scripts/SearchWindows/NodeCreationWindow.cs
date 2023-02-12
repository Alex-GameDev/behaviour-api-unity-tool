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
    public class NodeCreationWindow : ScriptableObject, ISearchWindowProvider
    {
        List<SearchTreeEntry> entries;
        public Action<Type, Vector2> TreeEntrySelected;

        public void SetEntryHierarchy(List<SearchTreeEntry> entries)
        {
            this.entries = entries;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return entries ?? new List<SearchTreeEntry>();
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var pos = context.screenMousePosition;
            var type = (Type)SearchTreeEntry.userData;
            TreeEntrySelected?.Invoke(type, pos);
            return true;
        }

        public static NodeCreationWindow Create(Action<Type, Vector2> createNode)
        {
            var searchWindow = CreateInstance<NodeCreationWindow>();
            searchWindow.TreeEntrySelected += createNode;
            return searchWindow;
        }
    }
}