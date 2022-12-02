using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class HierarchySearchWindow : ScriptableObject, ISearchWindowProvider
    {
        Type rootType;
        public void SetRootType(Type type) => rootType = type;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var assemblies = VisualSettings.GetOrCreateSettings().assemblies;
            var types = TypeUtilities.GetTypesDerivedFrom(rootType, assemblies);
            return types.Select(type => new SearchTreeEntry(new GUIContent(type.Name))
            {
                level = 0,
                userData = type
            }).ToList();
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var pos = context.screenMousePosition;
            Debug.Log((Type)SearchTreeEntry.userData);
            return true;
        }
    }

}