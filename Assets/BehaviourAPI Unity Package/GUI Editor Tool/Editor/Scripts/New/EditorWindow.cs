
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Editor;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UnityTool.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.New.Unity.Editor
{
    public class EditorWindow : UnityEditor.EditorWindow
    {
        #region --------------------------------- File paths ----------------------------------
        private static string PATH => BehaviourAPISettings.instance.EditorLayoutsPath + "windows/behavioursystemwindow.uxml";

        private static string EMPTYPANELPATH => BehaviourAPISettings.instance.EditorLayoutsPath + "emptygraphpanel.uxml";

        #endregion

        #region ------------------------------- public fields --------------------------------

        /// <summary>
        /// The singleton instance of the window.
        /// </summary>
        public static EditorWindow Instance;

        /// <summary>
        /// The system that is being edited
        /// </summary>
        public SystemAsset System;

        /// <summary>
        /// Is the window in runtime mode?
        /// </summary>
        public bool IsRuntime { get; private set; }

        #endregion

        #region ------------------------------- private fields --------------------------------

        GraphView _graphView;

        VisualElement _container, _currentInspector;

        Toolbar _editToolbar;
        ToolbarMenu _selectGraphMenu;

        NodeInspector _nodeInspector;
        GraphInspector _graphInspector;
        PushPerceptionInspector _pushPerceptionInspector;

        #endregion

        #region --------------------------- Create window methods ----------------------------

        [MenuItem("BehaviourAPI/Open window")]
        public static void Open()
        {
            EditorWindow window = GetWindow<EditorWindow>();
            window.minSize = new UnityEngine.Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            window.Refresh();
        }

        public static void Open(SystemAsset system, bool runtime = false)
        {
            EditorWindow window = GetWindow<EditorWindow>();
            window.minSize = new UnityEngine.Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");

            window.System = system;
            window.Refresh();
        }

        private void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            Instance = null;
            System = null;
            IsRuntime = false;
        }

        #endregion

        #region ----------------------------- Create GUI -----------------------------

        void CreateGUI()
        {
            var windownFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PATH).Instantiate();
            rootVisualElement.Add(windownFromUXML);

            _container = rootVisualElement.Q("bw-content");

            _nodeInspector = AddInspector<NodeInspector>();

            _graphInspector = AddInspector<GraphInspector>();
            _graphInspector.Disable();

            _pushPerceptionInspector = AddInspector<PushPerceptionInspector>();
            _pushPerceptionInspector.Disable();

            rootVisualElement.Q<Button>("im-graph-btn").clicked += () => ChangeInspector(_graphInspector);
            rootVisualElement.Q<Button>("im-pushperceptions-btn").clicked += () => ChangeInspector(_pushPerceptionInspector);

            _graphView = AddGraphView();
            Debug.Log("Graph view added");
            SetUpToolbar();

            Undo.undoRedoPerformed += Refresh;
        }

        private T AddInspector<T>() where T : VisualElement, new()
        {
            var inspector = new T();
            _container.Add(inspector);
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

            _selectGraphMenu = mainToolbar.Q<ToolbarMenu>("bw-toolbar-graph-menu");
            _editToolbar = rootVisualElement.Q<Toolbar>("bw-toolbar-edit");

            _editToolbar.Q<ToolbarButton>("bw-toolbar-setroot-btn").clicked += ChangeMainGraph;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-clear-btn").clicked += OpenClearGraphWindow;
            _editToolbar.Q<ToolbarButton>("bw-toolbar-delete-btn").clicked += DisplayDeleteGraphAlertWindow;
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

        void OpenClearGraphWindow()
        {
            if (_graphView.graphData == null) return;
            AlertWindow.CreateAlertWindow("¿Clear current graph?", ClearCurrentGraph);
        }

        void DisplayDeleteGraphAlertWindow()
        {
            if (System == null || System.data.graphs.Count == 0) return;
            AlertWindow.CreateAlertWindow("Are you sure to delete the current graph?", DeleteCurrentGraph);
        }

        #endregion

        #region ---------------------------- Update layout ----------------------------

        void Refresh()
        {
            if(System != null)
            {
                DisplayGraph(System.data.graphs.FirstOrDefault(), forceRefresh: true);
            }
            else
            {
                _graphView.ClearView();
            }

            UpdateGraphSelectionToolbar();
        }

        void ChangeInspector(VisualElement inspector)
        {
            _currentInspector?.Disable();
            if (_currentInspector == inspector)
            {
                _currentInspector = null;
            }
            else
            {
                _currentInspector = inspector;
                _currentInspector?.Enable();
            }
        }

        void DisplayGraph(GraphData data, bool forceRefresh = false)
        {
            if (_graphView == null) Debug.Log("e");
            if (data != null && _graphView.graphData == data && !forceRefresh) return;

            _graphView.SetGraphData(data);

            if (data != null)
            {
                //_noGraphPanel.Disable();
                _graphView.SetGraphData(data);
            }
            else
            {
                _graphView.ClearView();
                //_noGraphPanel.Enable();
            }

            _graphInspector.UpdateInspector(data);

            UpdateGraphSelectionToolbar();
        }

        void UpdateGraphSelectionToolbar()
        {
            _selectGraphMenu?.menu.MenuItems().Clear();

            if (System == null) return;

            for (int i = 0; i < System.data.graphs.Count; i++)
            {
                var graphData = System.data.graphs[i];

                if (graphData != null)
                {
                    var id = i;
                    _selectGraphMenu.menu.AppendAction(
                        actionName: $"{i + 1} - {graphData.name} ({graphData.graph.GetType().Name})",
                        action: _ => DisplayGraph(graphData),
                        status: _graphView.graphData == graphData ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal
                    );
                }
            }
        }

        #endregion

        #region --------------------------------- Modify system ---------------------------------

        /// <summary>
        /// Sets the current selected graphData as the main graphData of <see cref="System"/> 
        /// </summary>
        void ChangeMainGraph()
        {
            if (System == null || _graphView.graphData == null) return;

            if (System.data.graphs.Count == 0 || System.data.graphs[0] == _graphView.graphData) return;

            System.data.graphs.MoveAtFirst(_graphView.graphData);
            OnModifyAsset();
            UpdateGraphSelectionToolbar();
            Toast("MainGraphChanged");
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
                OnModifyAsset();
            }

            DisplayGraph(graph);
            Toast("Graph created");
        }

        /// <summary>
        /// Removes <see cref="CurrentGraphAsset"/> from the graphData asset list in <see cref="System"/>.
        /// </summary>
        private void DeleteCurrentGraph()
        {
            if (System == null || _graphView.graphData == null)
            {
                Debug.LogWarning("Can't delete graph if no system is selected");
                return;
            }

            if (System.data.graphs.Remove(_graphView.graphData))
            {
                //TODO: Comprobar referencias perdidas en subgrafos + push perceptions
                OnModifyAsset();

            }

            DisplayGraph(System.data.graphs.FirstOrDefault());

            Toast("Graph deleted");
        }

        /// <summary>
        /// Remove all nodes from <see cref="CurrentGraphAsset"/>.
        /// </summary>
        private void ClearCurrentGraph()
        {
            if (System == null || _graphView.graphData == null) return;
            _graphView.ClearView();

            _graphView.graphData.nodes.Clear();
            OnModifyAsset();

            //TODO: Comprobar referencias perdidas en push perceptions
            Toast("Graph clean");
        }

        internal void OnModifyAsset()
        {
            EditorUtility.SetDirty(System);
        }

        #endregion

        #region ------------------------------------ Utils ------------------------------------

        public void Toast(string message, float timeout = .5f)
        {
            ShowNotification(new GUIContent(message), timeout);
        }

        public void OpenGraphSearchWindow(Action<GraphData> setSubgraph)
        {
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                ElementSearchWindowProvider<GraphData>.Create<GraphSearchWindowProvider>(this, setSubgraph, g => g != _graphView.graphData));
        }

        #endregion
    }
}