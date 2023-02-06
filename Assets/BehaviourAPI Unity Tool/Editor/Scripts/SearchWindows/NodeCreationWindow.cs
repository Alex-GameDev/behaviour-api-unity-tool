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
        HierarchicalTypeNode rootTypeNode;
        public Action<Type, Vector2> TreeEntrySelected;

        public void SetEntryHierarchy(List<SearchTreeEntry> entries)
        {
            this.entries = entries;
        }

        public void SetRootType(Type rootType)
        {
            var types = rootType.GetSubClasses();
            rootTypeNode = new HierarchicalTypeNode(rootType, types);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return entries ?? GetSubSearchTree(rootTypeNode, 0);
        }

        List<SearchTreeEntry> GetSubSearchTree(HierarchicalTypeNode typeNode, int level)
        {
            var list = new List<SearchTreeEntry>();
            if(typeNode.Childs.Count != 0)
            {
                list.Add(new SearchTreeGroupEntry(new GUIContent(typeNode.Type.Name), level));
                if (!typeNode.Type.IsAbstract) list.Add(new SearchTreeEntry(new GUIContent(typeNode.Type.Name))
                {
                    level = level + 1,
                    userData = typeNode.Type
                });
                typeNode.Childs.ForEach(child => list.AddRange(GetSubSearchTree(child, level + 1)));
            }
            else
            {
                list.Add(new SearchTreeEntry(new GUIContent($"      {typeNode.Type.Name}"))
                {
                    level = level,
                    userData = typeNode.Type
                });
            }
            return list;
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