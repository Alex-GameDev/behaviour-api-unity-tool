using BehaviourAPI.Unity.Editor;
using BehaviourAPI.UnityTool.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.New.Unity.Editor
{
    public abstract class ElementSearchWindowProvider<T> : ScriptableObject, ISearchWindowProvider where T : class
    {
        protected EditorWindow _window;
        Action<T> _callback;
        protected Func<T, bool> _filter;

        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as T);
            return true;
        }

        public static E Create<E>(EditorWindow editorWindow, Action<T> callback, Func<T, bool> filter = null) where E : ElementSearchWindowProvider<T>
        {
            E window = CreateInstance<E>();
            window._window = editorWindow;
            window._callback = callback;
            window._filter = filter;
            return window;
        }
    }

    public class GraphSearchWindowProvider : ElementSearchWindowProvider<GraphData>
    {
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.AddGroup("Graphs", 0);

            var graphList = _window.System.data.graphs;
            for (int i = 0; i < graphList.Count; i++)
            {
                if (_filter == null || _filter(graphList[i]))
                {
                    list.AddEntry($"{i + 1} - {graphList[i].name}", 1, graphList[i]);
                }
            }
            return list;
        }
    }

    public class NodeSearchWindowProvider : ElementSearchWindowProvider<NodeData>
    {
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            list.AddGroup("Nodes", 0);

            var graphList = _window.System.data.graphs;

            for (int i = 0; i < graphList.Count; i++)
            {
                list.AddGroup($"{i + 1} - {graphList[i].name}", 1);
                for(int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    if(_filter == null || _filter(graphList[i].nodes[j]))
                    {
                        var nodeData = graphList[i].nodes[j];
                        list.AddEntry($"{nodeData.name} ({nodeData.node.GetType().Name})", 2, nodeData);
                    }
                }
            }
            return list;
        }
    }
}
