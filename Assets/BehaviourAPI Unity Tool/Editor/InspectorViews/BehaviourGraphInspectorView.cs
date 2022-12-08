using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourGraphInspectorView : VisualElement
    {
        VisualElement _inspectorContent;

        public BehaviourGraphInspectorView()
        {
            AddLayout();
            AddStyles();
        }

        private void AddStyles()
        {
            var styleSheet = VisualSettings.GetOrCreateSettings().InspectorStylesheet;
            styleSheets.Add(styleSheet);
        }

        private void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().graphInspectorLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
            _inspectorContent = this.Q("inspector-container");
        }

        public void UpdateInspector(GraphAsset graphAsset)
        {
            _inspectorContent.Clear();
            var editor = UnityEditor.Editor.CreateEditor(graphAsset);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (editor && editor.target)
                    editor.OnInspectorGUI();
            });
            _inspectorContent.Add(container);
        }

        public void Show() => style.display = DisplayStyle.Flex;
        public void Hide() => style.display = DisplayStyle.None;
    }
}
