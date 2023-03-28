using BehaviourAPI.Unity.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class Inspector<T> : VisualElement
    {
        private static string inspectorPath => BehaviourAPISettings.instance.EditorLayoutsPath + "/inspector.uxml";
        public enum Side { Left, Right }

        protected VisualElement _inspectorContent;
        protected VisualElement _root;
        protected VisualElement _mainContainer;

        protected Label _titleLabel;

        protected T _selectedElement;

        public Inspector(string title, Side side)
        {
            AddLayout();

            _titleLabel.text = title;
            if (side == Side.Left) _root.style.left = new StyleLength(0f);
            else if (side == Side.Right) _root.style.right = new StyleLength(0f);
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

        public virtual void UpdateInspector(T element)
        {
            _inspectorContent.Clear();
            _selectedElement = element;
        }
    }


    public class NodeInspector : Inspector<NodeData>
    {
        SerializedObject obj;
        private static readonly string _nodeProperty = ".node";
        private static readonly string _endProperty = ".parentIds";
        public NodeInspector() : base("Node", Side.Left)
        {
        }

        public override void UpdateInspector(NodeData element)
        {
            base.UpdateInspector(element);
            obj?.Dispose();

            if (BehaviourEditorWindow.Instance.System == null) return;

            obj = new SerializedObject(BehaviourEditorWindow.Instance.System.ObjectReference);

            if (BehaviourEditorWindow.Instance.IsRuntime) return;
            if (element == null) return;

            var system = BehaviourEditorWindow.Instance.System;
            var graphId = BehaviourEditorWindow.Instance.GetSelectedGraphIndex();
            var nodeId = system.Data.graphs[graphId].nodes.IndexOf(element);

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                var path = $"data.graphs.Array.data[{graphId}].nodes.Array.data[{nodeId}]";

                var namePath = path + ".name";
                var nameProp = obj.FindProperty(namePath);
                EditorGUILayout.PropertyField(nameProp, true);

                var prop = obj.FindProperty(path + _nodeProperty);
                var end = obj.FindProperty(path + _endProperty);
                bool child = true;
                while (prop.Next(child) && !SerializedProperty.EqualContents(prop, end))
                {
                    EditorGUILayout.PropertyField(prop, true);
                    child = false;
                }
                obj.ApplyModifiedProperties();

            });
            _inspectorContent.Add(container);
        }
    }
}
