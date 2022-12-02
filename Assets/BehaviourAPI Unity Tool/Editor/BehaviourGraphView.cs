using System;
using BehaviourAPI.Unity.Runtime;
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
        public BehaviourGraphView(BehaviourGraphAsset graphAsset)
        {

            GraphAsset = graphAsset;
            AddGridBackground();
            AddManipulators();
            AddStyles();
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

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void CreateNode(Type type, Vector2 position) { }
        public void Connect(Node source, Node target, int sourceIdx, int targetIdx) { }

    }
}