using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class CreationWindow : ScriptableObject, ISearchWindowProvider
    {
        Action<Type> _selectEntryCallback;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();
            var hierarchyNode = GetHierarchyNode();

            list.AddGroup(hierarchyNode.name, 0);
            foreach(var subNode in hierarchyNode.Childs)
            {
                GetSubSearchTreeEntry(subNode, list, 1);
            }
            return list;
        }

        void GetSubSearchTreeEntry(EditorHierarchyNode node, List<SearchTreeEntry> list, int level)
        {
            if(node.Childs.Count() == 0)
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

        protected abstract EditorHierarchyNode GetHierarchyNode();

        public void Open(Action<Type> callback, SearchWindowContext context)
        {
            _selectEntryCallback = callback;
            SearchWindow.Open(context, this);
        }

        public void Open(Action<Type> callback)
        {
            _selectEntryCallback = callback;
            var mousePos = Event.current.mousePosition;
            mousePos += BehaviourEditorWindow.Instance.position.position;

            SearchWindow.Open(new SearchWindowContext(mousePos), this);
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _selectEntryCallback?.Invoke((Type)SearchTreeEntry.userData);
            _selectEntryCallback = null;
            return true;
        }
    }
}
