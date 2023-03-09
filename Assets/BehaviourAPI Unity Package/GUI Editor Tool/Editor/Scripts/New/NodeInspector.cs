using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Editor;
using BehaviourAPI.UnityTool.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.New.Unity.Editor
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
        private static readonly string _nodeProperty = "node";
        private static readonly string _endProperty = "parentIds";
        public NodeInspector() : base("Node", Side.Left)
        {

        }

        public override void UpdateInspector(NodeData element)
        {
            base.UpdateInspector(element);

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                var obj = new SerializedObject(EditorWindow.Instance.System);
                var path = "data.graphs.Array.data[0].nodes.Array.data[0]";
                var prop = obj.FindProperty("data.graphs.Array.data[0].nodes.Array.data[0].node");
                var end = obj.FindProperty("data.graphs.Array.data[0].nodes.Array.data[0].parentIds");
                bool child = true;
                while (prop.Next(child) && !SerializedProperty.EqualContents(prop, end))
                {
                    EditorGUILayout.PropertyField(prop, true);
                    child = false;
                }
                obj.ApplyModifiedProperties();

            });
            _inspectorContent.Add(container);

            //var obj = new SerializedObject(EditorWindow.Instance.System);
            //var path = "data.graphs.Array.data[0].nodes.Array.data[0].node";
            //var prop = obj.FindProperty(path);
            
            //while(prop.Next(true))
            //{
            //    Debug.Log(prop.propertyPath);
            //}

            //foreach(SerializedProperty p in obj.GetIterator())
            //{
            //    Debug.Log(p.propertyPath);
            //}
        }

        public void UpdateInspector(string propertyPath)
        {
            base.UpdateInspector(null);

            if (propertyPath == null) return;

            IMGUIContainer container = new IMGUIContainer(() =>
            {
                var obj = new SerializedObject(EditorWindow.Instance.System);

                EditorGUILayout.PropertyField(obj.FindProperty(propertyPath + ".name"));
                EditorGUILayout.Space(10f);
                var prop = obj.FindProperty(propertyPath + ".node");
                var end = obj.FindProperty(propertyPath + ".parentIds");
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
