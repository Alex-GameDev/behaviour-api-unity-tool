using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Search window used to select a node from all graphs.
    /// </summary>
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        BehaviourSystemAsset system;

        Action<NodeAsset> _callback;
        Func<NodeAsset, bool> _filter;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.Add(new SearchTreeGroupEntry(new GUIContent("Graphs"), 0));
            system.Graphs.ForEach(g =>
            {
                list.Add(new SearchTreeGroupEntry(new GUIContent($"{g.Name} ({g.Graph.GetType().Name})"), 1));
                g.Nodes.ForEach(n =>
                {
                    if(_filter.Invoke(n))
                    {
                        list.Add(new SearchTreeEntry(new GUIContent($"{n.Name} ({n.Node.GetType().Name})"))
                        {
                            userData = n,
                            level = 2
                        });
                    }
                });
            });
            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as NodeAsset);
            return true;
        }

        public static NodeSearchWindow Create(BehaviourSystemAsset system)
        {
            var nodeSearchWindow = CreateInstance<NodeSearchWindow>();
            nodeSearchWindow.system = system;
            return nodeSearchWindow;
        }

        public void Open(Func<NodeAsset, bool> filter, Action<NodeAsset> callback)
        {
            _callback = callback;
            _filter = filter; 
            SearchWindow.Open(new SearchWindowContext(), this);
        }
    }
}
