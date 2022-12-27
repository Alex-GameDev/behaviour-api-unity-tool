using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Show a tree-formed menu whit a type hierarchy class and executes a callback with the type selected.
    /// </summary>
    public class TypeHierarchySearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public int MaxLevel = 3;

        Action<Type> OnSelectEntryTemporaryAction;
        public void Open(Action<Type> temporaryAction = null)
        {
            OnSelectEntryTemporaryAction = temporaryAction;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            throw new System.NotImplementedException();
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext _)
        {
            var type = (Type)SearchTreeEntry.userData;
            OnSelectEntryTemporaryAction?.Invoke(type);
            return true;
        }


        
    }
}
