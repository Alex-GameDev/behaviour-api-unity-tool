using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class SubgraphSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        BehaviourSystemAsset system;

        GraphAsset _currentGraph;
        Action<GraphAsset> _callback;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.Add(new SearchTreeGroupEntry(new GUIContent("Graphs"), 0));
            system.Graphs.ForEach(g =>
            {
                if(g != _currentGraph)
                {
                    list.Add(new SearchTreeEntry(new GUIContent($"{g.Name} ({g.Graph.GetType().Name})"))
                    {
                        userData = g,
                        level = 1
                       
                    });
                }
            });
            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as GraphAsset);
            return true;
        }

        public static SubgraphSearchWindow Create(BehaviourSystemAsset system)
        {
            var subgraphSearchWindow = CreateInstance<SubgraphSearchWindow>();
            subgraphSearchWindow.system = system;
            return subgraphSearchWindow;
        }

        public void Open(GraphAsset currentAsset, Action<GraphAsset> callback)
        {
            _currentGraph = currentAsset;
            _callback = callback;
            SearchWindow.Open(new SearchWindowContext(), this);
        }
    }
}
