using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Core;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphEditorWindow : EditorWindow
    {
        public static BehaviourSystemAsset BehaviourSystemAsset;
        public static bool IsAsset;

        VisualElement _container, _rootgraphBg;
        NodeInspectorView _nodeInspector;
        BehaviourGraphInspectorView _graphInspector;
        BehaviourGraphView _graphView;

        ToolbarMenu _selectGraphToolbarMenu;
        ToolbarToggle _autosaveToolbarToggle;
        ToolbarButton _saveToolbarButton;

        bool autoSave = false;

        public static void OpenGraph(BehaviourSystemAsset systemAsset, bool isAsset = true)
        {
            BehaviourSystemAsset = systemAsset;
            IsAsset = isAsset;
            BehaviourGraphEditorWindow window = GetWindow<BehaviourGraphEditorWindow>();
            window.minSize = new Vector2(550, 250);
            window.titleContent = new GUIContent($"Behaviour graph editor");
        }

        void CreateGUI()
        {
            AddLayout();

            if (BehaviourSystemAsset == null) return;            

            if (BehaviourSystemAsset.RootGraph != null)
            {
                _rootgraphBg.style.visibility = Visibility.Hidden;
                _nodeInspector = AddNodeInspectorView();
                DisplayGraph(BehaviourSystemAsset.RootGraph);
                SetGraphDropDown();
            }
            else
            {
                _graphInspector.Hide();
                _nodeInspector.Hide();
                DisplaySetRootGraphPanel();
            }
        }

        void AddLayout()
        {
            var windowLayout = VisualSettings.GetOrCreateSettings().BehaviourSystemWindowLayout.Instantiate();
            rootVisualElement.Add(windowLayout);

            _container = rootVisualElement.Q("bw-content");
            _graphView = AddGraphView();
            _nodeInspector = AddNodeInspectorView();
            _graphInspector = AddGraphInspectorView();
            _rootgraphBg = rootVisualElement.Q("bw-rootgraph");

            SetUpToolbar();            
        }

        void SetUpToolbar()
        {
            _selectGraphToolbarMenu = rootVisualElement.Q<ToolbarMenu>("bw-toolbar-graph-menu");
            _autosaveToolbarToggle = rootVisualElement.Q<ToolbarToggle>("bw-toolbar-autosave-toggle");
            _saveToolbarButton = rootVisualElement.Q<ToolbarButton>("bw-toolbar-save-btn");

            _saveToolbarButton.clicked += SaveSystemData;
            _autosaveToolbarToggle.RegisterValueChangedCallback((evt) => autoSave = evt.newValue);
        }

        void SaveSystemData()
        {
            Debug.Log("Saving graphs");
            if(IsAsset) AssetDatabase.SaveAssets();
            else EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        #region ---------------- Set root graph ----------------------

        /// <summary>
        /// Muestra un menú para crear un nuevo grafo y establecerlo como raíz
        /// </summary>
        void DisplaySetRootGraphPanel()
        {
            var graphscrollview = _rootgraphBg.Q<ScrollView>("bw-graphs-scrollview");
            var graphTypes = TypeUtilities.GetAllGraphTypes();
            graphTypes.ForEach(gType =>
            {
                graphscrollview.Add(new Button(() => SetRootGraph(gType)) { text = gType.Name });
            });
        }

        /// <summary>
        /// Crea un nuevo grafo dentro del sistema actual y lo establece como grafo raíz
        /// </summary>
        /// <param name="graphType"></param>
        void SetRootGraph(Type graphType)
        {
            if (BehaviourSystemAsset == null) return;

            BehaviourSystemAsset.CreateRootGraph(graphType);
            AddGraph(BehaviourSystemAsset.RootGraph);
            _rootgraphBg.style.visibility = Visibility.Hidden;
            _graphInspector.Show();
            _nodeInspector.Show();
            DisplayGraph(BehaviourSystemAsset.RootGraph);
            SetGraphDropDown();
        } 

        void AddGraph(GraphAsset asset)
        {
            if (IsAsset)
            {
                AssetDatabase.AddObjectToAsset(asset, BehaviourSystemAsset);                
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            if (autoSave) SaveSystemData();
        }

        #endregion

        /// <summary>
        /// Muestra uno de los grafos del sistema en el editor
        /// </summary>
        /// <param name="graphAsset"></param>
        void DisplayGraph(GraphAsset graphAsset)
        {
            Debug.Log("Loading graph");
            _graphInspector.UpdateInspector(graphAsset);
            _graphView.SetGraph(graphAsset);            
        }

        void SetGraphDropDown()
        {
            _selectGraphToolbarMenu.text = "Graph";
            BehaviourSystemAsset.Graphs.ForEach(g => _selectGraphToolbarMenu.menu.AppendAction(g.Graph.GetType().Name, (d) => DisplayGraph(g)));          
        }

        BehaviourGraphView AddGraphView()
        {
            var graphView = new BehaviourGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Insert(0, graphView);
            return graphView;
        }
        NodeInspectorView AddNodeInspectorView()
        {
            var nodeInspector = new NodeInspectorView();
            _container.Add(nodeInspector);
            return nodeInspector;
        }

        BehaviourGraphInspectorView AddGraphInspectorView()
        {
            var graphInspector = new BehaviourGraphInspectorView();
            _container.Add(graphInspector);
            return graphInspector;
        }
    }
}