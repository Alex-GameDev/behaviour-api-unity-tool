using BehaviourAPI.Unity.Runtime;
using Codice.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class InspectorView<T> : VisualElement where T :ScriptableObject
    {
        private static string inspectorPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/inspector.uxml";
        public enum Side { Left, Right }

        protected VisualElement _inspectorContent;
        protected VisualElement _root;
        protected VisualElement _mainContainer;
        protected Label _titleLabel;

        protected T _selectedElement;

        public InspectorView(string title, Side side)
        {
            AddLayout();            

            _titleLabel.text = title;
            if (side == Side.Left) _root.style.left = new StyleLength(0f);
            else if(side == Side.Right) _root.style.right = new StyleLength(0f);
        }

        protected virtual void AddLayout()
        {
            var inspectorFromUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(inspectorPath).Instantiate();
            Add(inspectorFromUXML);
            _inspectorContent = this.Q("iw-inspector-container");
            _root = this.Q("iw-root");
            _titleLabel = this.Q<Label>("iw-title");
            _mainContainer = this.Q("iw-main-container");

        }

        public virtual void UpdateInspector(T asset)
        {
            _inspectorContent.Clear();
            _selectedElement = asset;

            if (asset == null) return;

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
