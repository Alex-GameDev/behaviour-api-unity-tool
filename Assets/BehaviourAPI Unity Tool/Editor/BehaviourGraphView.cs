using System;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Visual element that represents a behaviour graph
    /// </summary>
    public class BehaviourGraphView : GraphView
    {
        BehaviourGraphAsset GraphAsset;
        HierarchySearchWindow searchWindow;
        EditorWindow editorWindow;
        public BehaviourGraphView(BehaviourGraphAsset graphAsset, EditorWindow parentWindow)
        {
            GraphAsset = graphAsset;
            editorWindow = parentWindow;
            AddGridBackground();
            AddManipulators();
            AddCreateNodeWindow();
            AddStyles();
        }

        void AddCreateNodeWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<HierarchySearchWindow>();
                searchWindow.SetRootType(typeof(BTNode));
                searchWindow.SetOnSelectEntryCallback(CreateNode);
            }

            nodeCreationRequest = context =>
            {
                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, searchWindow);
            };
        }

        void AddStyles()
        {
            StyleSheet styleSheet = VisualSettings.GetOrCreateSettings().GraphStylesheet;
            styleSheets.Add(styleSheet);
        }

        void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }

        void CreateNode(Type type, Vector2 position) 
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeAsset asset = GraphAsset.CreateNode(type, pos);

            if(asset != null)
            {
                DrawNodeView(asset);
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }
        }

        void DrawNodeView(NodeAsset asset)
        {
            NodeView nodeView = new NodeView(asset);
            AddElement(nodeView);
        }

        void Connect(Node source, Node target, int sourceIdx, int targetIdx) { }

    }
}