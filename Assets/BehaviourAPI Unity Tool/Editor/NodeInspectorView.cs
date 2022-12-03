using BehaviourAPI.Unity.Runtime;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class NodeInspectorView : VisualElement
    {
        VisualElement _inspectorContent;
        public NodeInspectorView()
        {
            AddLayout();
            AddStyles();
        }

        private void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().InspectorLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
            _inspectorContent = this.Q("inspector-container");
        }

        private void AddStyles()
        {
            var styleSheet = VisualSettings.GetOrCreateSettings().InspectorStylesheet;
            styleSheets.Add(styleSheet);
        }

        public void UpdateInspector(NodeAsset nodeAsset)
        {
            _inspectorContent.Clear();
            var editor = UnityEditor.Editor.CreateEditor(nodeAsset);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (editor && editor.target)
                    editor.OnInspectorGUI();
            });
            _inspectorContent.Add(container);
        }
    }
}
