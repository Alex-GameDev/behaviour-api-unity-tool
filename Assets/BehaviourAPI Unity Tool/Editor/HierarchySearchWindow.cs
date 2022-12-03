using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class HierarchySearchWindow : ScriptableObject, ISearchWindowProvider
    {
        HierarchicalTypeNode rootTypeNode;

        public void SetRootType(Type rootType)
        {
            var assemblies = VisualSettings.GetOrCreateSettings().assemblies;
            var types = TypeUtilities.GetTypesDerivedFrom(rootType, assemblies);
            rootTypeNode = new HierarchicalTypeNode(rootType, types);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return GetSubSearchTree(rootTypeNode, 0);
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
                list.Add(new SearchTreeEntry(new GUIContent(typeNode.Type.Name))
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
            Debug.Log((Type)SearchTreeEntry.userData);
            return true;
        }

        struct HierarchicalTypeNode
        {
            public Type Type;
            public List<HierarchicalTypeNode> Childs;

            public HierarchicalTypeNode(Type rootType, IEnumerable<Type> derivedTypes)
            {
                Type = rootType;
                Childs = derivedTypes.Where(t => t.BaseType == rootType).ToList()
                    .Select(subType => new HierarchicalTypeNode(subType, 
                    derivedTypes.Where(t => t.IsSubclassOf(subType) && t != subType))).ToList();
            }
        }
    }

}