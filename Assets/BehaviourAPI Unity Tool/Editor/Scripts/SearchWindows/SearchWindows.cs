using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class SearchWindow<T> : ScriptableObject, ISearchWindowProvider where T : ScriptableObject
    {
        protected Action<T> _callback;
        protected Func<T, bool> _filter;

        protected BehaviourEditorWindow _editorWindow;

        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            _callback?.Invoke(SearchTreeEntry.userData as T);
            return true;
        }

        public void SetEditorWindow(BehaviourEditorWindow editorWindow) => _editorWindow = editorWindow;

        public void OpenWindow(Action<T> callback, SearchWindowContext context, Func<T, bool> filter = null)
        {
            _callback = callback;
            _filter = filter;
            SearchWindow.Open(context, this);
        }

        public void OpenWindow(Action<T> callback, Func<T, bool> filter = null)
        {
            var mousePos = Event.current.mousePosition;
            mousePos += BehaviourEditorWindow.Instance.position.position;

            _callback = callback;
            _filter = filter;
            SearchWindow.Open(new SearchWindowContext(mousePos), this);
        }
    }

    /// <summary>
    /// Search window to select a node from any graph of the system
    /// </summary>
    public class NodeSearchWindow : SearchWindow<NodeAsset>
    {
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            var system = _editorWindow.System;

            list.Add(new SearchTreeGroupEntry(new GUIContent("Graphs"), 0));
            system.Graphs.ForEach(g =>
            {
                list.Add(new SearchTreeGroupEntry(new GUIContent($"{g.Name} ({g.Graph.GetType().Name})"), 1));
                g.Nodes.ForEach(n =>
                {
                    if (_filter?.Invoke(n) ?? true)
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
    }

    /// <summary>
    /// Search a graph from all the graphs of the system
    /// </summary>
    public class SubgraphSearchWindow : SearchWindow<GraphAsset>
    {
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            var system = _editorWindow.System;
            var currentGraph = _editorWindow.CurrentGraphAsset;

            list.Add(new SearchTreeGroupEntry(new GUIContent("Graphs"), 0));
            system.Graphs.ForEach(g =>
            {
                if (g != currentGraph)
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
    }

    /// <summary>
    /// Search a perception from all the perceptions of the system
    /// </summary>
    public class PerceptionSearchWindow : SearchWindow<PerceptionAsset>
    {
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = new List<SearchTreeEntry>();

            var system = _editorWindow.System;

            list.Add(new SearchTreeGroupEntry(new GUIContent("Perceptions"), 0));
            system.PullPerceptions.ForEach(p =>
            {
                if (_filter?.Invoke(p) ?? true)
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
    }
}
