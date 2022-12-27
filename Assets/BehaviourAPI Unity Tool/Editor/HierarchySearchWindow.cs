using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Creates a window menu to select a type
    /// </summary>
    public class HierarchySearchWindow : ScriptableObject, ISearchWindowProvider
    {
        HierarchicalTypeNode rootTypeNode;

        Action<Type, Vector2> OnSelectEntryAction;

        public void SetRootType(Type rootType)
        {
            var assemblies = VisualSettings.GetOrCreateSettings().assemblies;
            var types = TypeUtilities.GetTypesDerivedFrom(rootType, assemblies);
            rootTypeNode = new HierarchicalTypeNode(rootType, types);
        }

        public void SetOnSelectEntryCallback(Action<Type, Vector2> callback) => OnSelectEntryAction = callback;

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
            OnSelectEntryAction?.Invoke(type, pos);
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