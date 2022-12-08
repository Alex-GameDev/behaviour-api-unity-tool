using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class InspectorView<T> : VisualElement where T :ScriptableObject
    {
        VisualElement _inspectorContent;

        public InspectorView(VisualTreeAsset layoutAsset)
        {
            AddLayout(layoutAsset);
            AddStyles();
        }

        private void AddStyles()
        {
            var styleSheet = VisualSettings.GetOrCreateSettings().InspectorStylesheet;
            styleSheets.Add(styleSheet);
        }
        private void AddLayout(VisualTreeAsset layoutAsset)
        {
            var inspectorFromUXML = layoutAsset.Instantiate();
            Add(inspectorFromUXML);
            _inspectorContent = this.Q("inspector-container");
        }
        public void UpdateInspector(T asset)
        {
            _inspectorContent.Clear();
            var editor = UnityEditor.Editor.CreateEditor(asset);
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
