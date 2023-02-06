using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class PerceptionSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        BehaviourSystemAsset system;

        Action<PerceptionAsset> _callback;
        Func<PerceptionAsset, bool> _filter;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.Add(new SearchTreeGroupEntry(new GUIContent("Perceptions"), 0));
            system.Perceptions.ForEach(p =>
            {
                if(_filter.Invoke(p))
                {
                    list.Add(new SearchTreeEntry(new GUIContent($"{p.Name}"))
                    {
                        userData = p,
                        level = 1
                    });
                }
            });
            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as PerceptionAsset);
            return true;
        }

        public static PerceptionSearchWindow Create(BehaviourSystemAsset system)
        {
            var nodeSearchWindow = CreateInstance<PerceptionSearchWindow>();
            nodeSearchWindow.system = system;
            return nodeSearchWindow;
        }

        public void Open(Action<PerceptionAsset> callback, Func<PerceptionAsset, bool> filter = null)
        {
            _callback = callback;
            _filter = filter ?? (_ => true);
            SearchWindow.Open(new SearchWindowContext(), this);
        }
    }
}
