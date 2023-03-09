
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Editor;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UnityTool.Framework;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.New.Unity.Editor
{
    public class EditorWindow : UnityEditor.EditorWindow
    {
        private static string PATH => BehaviourAPISettings.instance.EditorLayoutsPath + "windows/behavioursystemwindow.uxml";

        public static EditorWindow Instance;

        GraphView graphView;
        Toolbar _editToolbar;

        NodeInspector _nodeInspector;

        VisualElement _container;

        public SystemAsset System;
        public static void Open(SystemAsset system, bool runtime = false)
        {
            EditorWindow window = GetWindow<EditorWindow>();
            window.minSize = new UnityEngine.Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");
            Instance = window;

            window.System = system;
            window.Refresh();
        }

        void CreateGUI()
        {
            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PATH).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            _container = rootVisualElement.Q("bw-content");

            _nodeInspector = AddInspector<NodeInspector>();

            graphView = AddGraphView();

            SetUpToolbar();
        }

        private T AddInspector<T>(bool swapable = false) where T : VisualElement, new()
        {
            var inspector = new T();
            _container.Add(inspector);

            if (swapable)
            {
                inspector.Disable();
            }

            return inspector;
        }

        GraphView AddGraphView()
        {
            var graphView = new GraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Insert(0, graphView);
            graphView.NodeSelected += (nodeData) =>
            {
                if(nodeData != null)
                {
                    var index = graphView.graphData.nodes.IndexOf(nodeData);
                    _nodeInspector.UpdateInspector($"data.graphs.Array.data[0].nodes.Array.data[{index}]");
                }
                else
                {
                    _nodeInspector.UpdateInspector(null);
                }

            };
            return graphView;
        }


        void SetUpToolbar()
        {
            var mainToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-main");            

            _editToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-edit");

            //_editToolbar.Q<ToolbarButton>("bw-toolbar-setroot-btn").clicked += ChangeMainGraph;
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-clear-btn").clicked += OpenClearGraphWindow;
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-delete-btn").clicked += DisplayDeleteGraphAlertWindow;
            //_editToolbar.Q<ToolbarButton>("bw-toolbar-generatescript-btn").clicked += OpenCreateScriptWindow;

            var addGraphMenu = _editToolbar.Q<ToolbarMenu>("bw-toolbar-add-menu");

            var adapters = typeof(GraphAdapter).GetSubClasses().FindAll(ad => ad.GetCustomAttribute<CustomAdapterAttribute>() != null);

            foreach (var adapter in adapters)
            {
                var graphType = adapter.GetCustomAttribute<CustomAdapterAttribute>().type;
                if (graphType.IsSubclassOf(typeof(BehaviourGraph)))
                {
                    addGraphMenu.menu.AppendAction(graphType.Name,
                        _ => CreateGraph($"new {graphType.Name}", graphType)
                    );
                }
            }
        }

        void Refresh()
        {
            if(System.data.graphs.Count > 0)
            {
                graphView.SetGraphData(System.data.graphs[0]);

                if (System.data.graphs[0].nodes.Count > 0)
                    _nodeInspector.UpdateInspector(System.data.graphs[0].nodes[0]);
            }
        }


        private void CreateGraph(string name, Type type)
        {
            if (System == null)
            {
                Debug.LogWarning("Can't create graph if no system is selected");
                return;
            }

            var graph = new GraphData(type);

            if (graph != null)
            {
                System.data.graphs.Add(graph);
                EditorUtility.SetDirty(System);
            }

            //DisplayGraph(graphAsset);
            Toast("Graph created");
        }

        public void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }

        internal void OnModifyAsset()
        {
            EditorUtility.SetDirty(System);
        }
    }
}