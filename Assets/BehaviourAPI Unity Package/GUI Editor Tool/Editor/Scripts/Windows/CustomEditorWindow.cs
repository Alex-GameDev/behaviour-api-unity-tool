using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomEditorWindow : EditorWindow
    {
        private static readonly Vector2 k_MinWindowSize = new Vector2(500, 300);
        private static readonly string k_WindowTitle = "Behaviour System Editor";
        private static readonly string k_WindowPath = "gwindow.uxml";

        static readonly string[] k_inspectorOptions = new string[]
        {
            "Graph",
            "Node",
        };

        /// <summary>
        /// The singleton instance of the window.
        /// </summary>
        public static CustomEditorWindow instance { get; private set; }

        /// <summary>
        /// The reference to the system that is currently being edited
        /// </summary>
        public IBehaviourSystem System { get; private set; }

        /// <summary>
        /// True if the window is in runtime mode with editor tools disabled.
        /// </summary>
        public bool IsRuntime { get; private set; }

        #region ---------------------------------- Editor data ----------------------------------

        private SerializedObject serializedObject;
        private SerializedProperty rootProperty;
        private SerializedProperty graphsProperty;
        private SerializedProperty pushPerceptionsProperty;

        private SerializedProperty selectedGraphProperty;
        private SerializedProperty selectedNodeProperty;
        private SerializedProperty selectedPushPerceptionProperty;

        private SerializedProperty currentProperty;

        private Vector2 pushPerceptionScrollPos, pushPerceptionTargetScrollPos;

        private int inspectorMode = 0;

        private int selectedGraphIndex = -1;
        private List<int> selectedNodeIndexList = new List<int>();

        #endregion

        private DropdownField selectGraphDropdown;

        private GraphDataView graphDataView;

        CreateGraphPanel createGraphPanel;
        GenerateScriptPanel generateScriptPanel;

        ToolPanel currentPanel;

        #region -------------------------------------------- Create window --------------------------------------------

        private void OnEnable()
        {
            instance = this;
        }

        private void OnDisable()
        {
            instance = null;
        }

        /// <summary>
        /// Open the window without a behaviour system assigned.
        /// </summary>
        public static void Create()
        {
            CustomEditorWindow window = GetWindow<CustomEditorWindow>();
            window.minSize = k_MinWindowSize;
            window.titleContent = new GUIContent(k_WindowTitle);
        }

        /// <summary>
        /// Open the window from a behaviour system.
        /// </summary>
        /// <param name="system">The system object that will be edited.</param>
        /// <param name="runtime">True if the window is in runtime mode and the editor tools are disabled.</param>
        public static void Create(IBehaviourSystem system, bool runtime = false)
        {
            CustomEditorWindow window = GetWindow<CustomEditorWindow>();
            window.minSize = k_MinWindowSize;
            window.titleContent = new GUIContent(k_WindowTitle);
            window.UpdateSystem(system, runtime);
        }

        private void CreateGUI()
        {
            string windowAssetFullPath = BehaviourAPISettings.instance.EditorLayoutsPath + k_WindowPath;
            var windowAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(windowAssetFullPath);

            if (!windowAsset)
            {
                Debug.LogWarning($"Window layout path was not found ({windowAssetFullPath}). Check the path in BehaviourAPISettings script");
                return;
            }

            windowAsset.CloneTree(rootVisualElement);

            selectGraphDropdown = rootVisualElement.Q<DropdownField>("bw-graph-select");
            selectGraphDropdown.RegisterValueChangedCallback(OnSelectedGraphChanges);

            var mainContainer = rootVisualElement.Q("bw-main");

            // Add graph:
            createGraphPanel = new CreateGraphPanel(CreateGraph);
            mainContainer.Add(createGraphPanel);
            createGraphPanel.Disable();
            var addGraphBtn = rootVisualElement.Q<Button>("bw-add-graph-btn");
            addGraphBtn.clicked += OnClickAddGraphBtn;

            // Delete graph
            var deleteGraphBtn = rootVisualElement.Q<Button>("bw-remove-graph-btn");
            deleteGraphBtn.clicked += OnClickRemoveGraphBtn;

            // generate script
            generateScriptPanel = new GenerateScriptPanel();
            mainContainer.Add(generateScriptPanel);
            generateScriptPanel.Disable();
            var generateScriptBtn = rootVisualElement.Q<Button>("bw-script-btn");
            generateScriptBtn.clicked += OnClickGenerateScriptBtn;

            // Set main
            var setMainBtn = rootVisualElement.Q<Button>("bw-setmain-graph-btn");
            setMainBtn.clicked += OnClickSetMainBtn;

            // Clear graph
            var clearBtn = rootVisualElement.Q<Button>("bw-clear-graph-btn");
            clearBtn.clicked += OnClickClearBtn;

            IMGUIContainer container = rootVisualElement.Q<IMGUIContainer>("bw-inspector");
            container.onGUIHandler = OnGUIHandler;

            graphDataView = new GraphDataView();
            graphDataView.StretchToParentSize();
            var graphContainer = rootVisualElement.Q("bw-graph");
            graphContainer.Insert(0, graphDataView);
            graphDataView.DataChanged += () => EditorUtility.SetDirty(System.ObjectReference);
            graphDataView.SelectionNodeChanged += OnSelectionNodeChanged;
        }

        private void OnClickClearBtn()
        {
            if (selectedGraphProperty == null) return;
            if (System.Data.graphs[selectedGraphIndex].nodes.Count == 0) return;

            var pos = Event.current.mousePosition + position.position;
            AlertWindow.CreateAlertWindow("Clear selected graph?", pos, ClearSelectedGraph);
        }

        private void OnClickSetMainBtn()
        {
            if (selectedGraphProperty == null) return;
            if (selectedGraphIndex == 0) return;

            var pos = Event.current.mousePosition + position.position;
            AlertWindow.CreateAlertWindow("Convert selected graph to the main graph?", pos, ChangeMainGraph);
        }

        private void OnSelectionNodeChanged(List<int> selectedNodeIndexList)
        {
            this.selectedNodeIndexList = selectedNodeIndexList;
            inspectorMode = selectedNodeIndexList.Count > 0 ? 1 : 0;

            if(selectedNodeIndexList.Count == 1)
            {
                selectedNodeProperty = selectedGraphProperty.FindPropertyRelative("nodes").GetArrayElementAtIndex(selectedNodeIndexList[0]);
            }
            else
            {
                selectedNodeProperty = null;
            }
        }

        private void UpdateSystem(IBehaviourSystem system, bool runtime)
        {
            System = system;
            IsRuntime = runtime;

            if (system != null && !runtime)
            {
                serializedObject = new SerializedObject(system.ObjectReference);

                rootProperty = serializedObject.FindProperty("data");
                graphsProperty = rootProperty.FindPropertyRelative("graphs");
                pushPerceptionsProperty = rootProperty.FindPropertyRelative("pushPerceptions");
            }

            if (System.Data.graphs.Count > 0)
            {
                selectGraphDropdown.choices = System.Data.graphs.Select(g => g.name).ToList();
                selectGraphDropdown.index = 0;
            }
        }

        #endregion

        #region ----------------------------------- Toolbar events --------------------------------------

        private void OnClickAddGraphBtn() => ChangeToolPanel(createGraphPanel, true);

        private void OnClickGenerateScriptBtn() => ChangeToolPanel(generateScriptPanel, true);

        private void OnClickRemoveGraphBtn()
        {
            if (selectedGraphProperty == null) return;
            var pos = Event.current.mousePosition + position.position;
            AlertWindow.CreateAlertWindow($"Delete current graph?\n({selectedGraphProperty.displayName})", pos, DeleteSelectedGraph);
        }

        // Evento de cambiar grafo seleccionado
        private void OnSelectedGraphChanges(ChangeEvent<string> evt)
        {
            if (evt.previousValue == evt.newValue) return;

            UpdateGraphSelection();
        }

        private void UpdateGraphSelection()
        {
            selectedGraphIndex = selectGraphDropdown.index;

            if (selectedGraphIndex < 0) selectedGraphProperty = null;
            else selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex);

            ChangeSelectedGraph();
        }

        private void UpdateSelectionMenu()
        {
            selectGraphDropdown.choices = System.Data.graphs.Select(g => g.name).ToList();
        }

        // Cambia el grafo seleccionado, eliminando los nodos seleccionados
        private void ChangeSelectedGraph()
        {
            selectedNodeIndexList.Clear();
            selectedGraphIndex = selectGraphDropdown.index;

            if (selectedGraphIndex < 0) selectedGraphProperty = null;
            else selectedGraphProperty = graphsProperty.GetArrayElementAtIndex(selectedGraphIndex);
            UpdateGraphView();
        }

        private void ChangeToolPanel(ToolPanel toolPanel, bool canClose)
        {
            currentPanel?.ClosePanel();
            toolPanel?.Open(canClose);
            currentPanel = toolPanel;
        }

        #endregion

        #region ------------------------------------- Modify data ----------------------------------

        private void CreateGraph(string graphName, Type graphType)
        {
            var graphData = new GraphData(graphType);
            graphData.name = graphName;
            System.Data.graphs.Add(graphData);
            serializedObject.Update();

            UpdateSelectionMenu();
            selectGraphDropdown.index = System.Data.graphs.Count - 1;
        }

        private void DeleteSelectedGraph()
        {
            graphsProperty.DeleteArrayElementAtIndex(selectedGraphIndex);
            serializedObject.ApplyModifiedProperties();

            UpdateSelectionMenu();
            selectGraphDropdown.index = 0;
        }

        private void ChangeMainGraph()
        {
            if(selectedGraphIndex >= System.Data.graphs.Count) return;

            var currentGraph = System.Data.graphs[selectedGraphIndex];
            System.Data.graphs.MoveAtFirst(currentGraph);
            serializedObject.Update();

            UpdateSelectionMenu();
            selectGraphDropdown.index = 0;
            UpdateGraphSelection();
        }

        private void ClearSelectedGraph()
        {
            selectedGraphProperty.FindPropertyRelative("nodes").ClearArray();
            serializedObject.ApplyModifiedProperties();

            selectedNodeIndexList.Clear();

            UpdateGraphView();
        }

        #endregion

        #region -------------------------------------- GraphView -----------------------------------

        private void UpdateGraphView()
        {
            Debug.Log("Update graph view");
            if (selectedGraphIndex >= 0) graphDataView.UpdateGraph(System.Data.graphs[selectedGraphIndex], selectedGraphProperty.FindPropertyRelative("nodes"));
            else graphDataView.UpdateGraph(null);
        }

        #endregion

        #region --------------------------------- Editor Inspector ---------------------------------

        private void OnGUIHandler()
        {
            if (System == null || serializedObject == null) return;

            using(var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                selectGraphDropdown.choices = System.Data.graphs.Select(g => string.IsNullOrWhiteSpace(g.name) ? "unnamed" : g.name).ToList();
                selectGraphDropdown.index = selectedGraphIndex;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                DrawInspectorTab();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();                

                if(changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    graphDataView.RefreshSelectedNodesProperties();
                }
            }           
        }

        private void DrawInspectorTab()
        {
            inspectorMode = GUILayout.Toolbar(inspectorMode, k_inspectorOptions);

            switch(inspectorMode)
            {
                case 0:
                    DisplayCurrentGraphProperty();
                    DisplayPushPerceptionListProperty();
                    break;
                case 1:
                    DisplayCurrentNodeProperty();
                    break;
            }
        }

        private void DisplayCurrentNodeProperty()
        {
            if (selectedNodeIndexList.Count == 0) return;

            else if(selectedNodeIndexList.Count == 1)
            {
                if(selectedNodeProperty != null)
                {
                    currentProperty = selectedNodeProperty;
                    EditorGUILayout.LabelField("Type", selectedNodeProperty.FindPropertyRelative("node").managedReferenceValue.TypeName(), EditorStyles.wordWrappedLabel);
                    DrawField("name", true);
                    DrawFieldsWithoutFold("node", true);
                }
                else
                {
                    EditorGUILayout.HelpBox("No property selected", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Multiedit is not enabled", MessageType.Info);
            }
        }

        private void DisplayCurrentGraphProperty()
        {
            if(selectedGraphProperty != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Graph", EditorStyles.centeredGreyMiniLabel);
                currentProperty = selectedGraphProperty;

                EditorGUILayout.LabelField("Type", selectedGraphProperty.FindPropertyRelative("graph").managedReferenceValue.TypeName(), EditorStyles.wordWrappedLabel);
                DrawField("name", true);
                DrawFieldsWithoutFold("graph", true);

                EditorGUILayout.Space(20);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("This system has no graphs. Create a graph to start editing", MessageType.Info);
            }
        }

        private void DisplayPushPerceptionListProperty()
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Push perceptions", EditorStyles.centeredGreyMiniLabel);

            pushPerceptionScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionScrollPos, "window", GUILayout.MinHeight(100));
            if (pushPerceptionsProperty == null) return;

            for(int i = 0; i < pushPerceptionsProperty.arraySize; i++)
            {
                SerializedProperty p = pushPerceptionsProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(p.displayName, GUILayout.ExpandWidth(true)))
                {
                    selectedPushPerceptionProperty = p;
                }

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    selectedPushPerceptionProperty = null;
                    pushPerceptionsProperty.DeleteArrayElementAtIndex(i);

                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create push perception"))
            {
                int size = pushPerceptionsProperty.arraySize;
                pushPerceptionsProperty.InsertArrayElementAtIndex(size);
                pushPerceptionsProperty.GetArrayElementAtIndex(size).FindPropertyRelative("name").stringValue = "newpushPerception";
            }

            if (selectedPushPerceptionProperty == null) return;

            DrawField(selectedPushPerceptionProperty.FindPropertyRelative("name"));

            pushPerceptionTargetScrollPos = EditorGUILayout.BeginScrollView(pushPerceptionTargetScrollPos, "window", GUILayout.MinHeight(100));

            SerializedProperty targetNodesProperty = selectedPushPerceptionProperty.FindPropertyRelative("targetNodeIds");

            Dictionary<string, NodeData> nodeIdMap = System.Data.GetNodeIdMap();
            for (int i = 0; i < targetNodesProperty.arraySize; i++)
            {
                SerializedProperty p = targetNodesProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(nodeIdMap.GetValueOrDefault(p.stringValue)?.name);

                if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                {
                    targetNodesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add target"))
            {
                var provider = ElementSearchWindowProvider<NodeData>.Create<NodeSearchWindowProvider>((n) => OnSelectTargetNode(targetNodesProperty, n), n => n.node is IPushActivable);
                provider.Data = System.Data;
                SearchWindow.Open(new SearchWindowContext(Event.current.mousePosition + position.position), provider);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void OnSelectTargetNode(SerializedProperty prop, NodeData nodeData)
        {
            int size = prop.arraySize;
            prop.InsertArrayElementAtIndex(size);
            prop.GetArrayElementAtIndex(size).stringValue = nodeData.id;
            prop.serializedObject.ApplyModifiedProperties();
        }

        private void DrawField(string propName, bool relative)
        {
            if (relative && currentProperty != null)
            {
                EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propName), true);
            }
            else if (serializedObject != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);
            }
        }

        private void DrawFieldsWithoutFold(string propName, bool relative)
        {
            SerializedProperty prop = null;
            if (relative && currentProperty != null)
            {
                prop = currentProperty.FindPropertyRelative(propName);
            }
            else if (serializedObject != null)
            {
                prop = serializedObject.FindProperty(propName);                
            }

            int deep = prop.propertyPath.Count(c => c == '.');
            foreach (SerializedProperty p in prop)
            {
                if (p.propertyPath.Count(c => c == '.') == deep + 1)
                {
                    EditorGUILayout.PropertyField(p, true);
                }
            }
        }

        private void DrawField(SerializedProperty property) => EditorGUILayout.PropertyField(property, true);

        #endregion
    }
}
