using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;
using Object = UnityEngine.Object;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Displays a <see cref="Action"/> from a <see cref="Node"/> inside a <see cref="NodeView"/>
    /// </summary>
    public class PerceptionContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        string propertyPath;

        NodeView _nodeView;

        Button _assignButton;
        VisualElement _container;

        public PerceptionContainerView(NodeAsset asset, SerializedProperty perceptionProperty, NodeView nodeView)
        {
            nodeAsset = asset;
            propertyPath = perceptionProperty.propertyPath;

            _nodeView = nodeView;
            AddLayout();
            SetUpContextualMenu();
            UpdateView();
        }

        void AddLayout()
        {
            _container = new VisualElement();
            Add(_container);

            _assignButton = new Button(OnAssignPerception) { text = "Assign perception" };
            Add(_assignButton);
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear perception", dd => ClearPerception(),
                    (_) => GetSerializedProperty().objectReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        void OnAssignPerception()
        {
            Debug.Log("Open perception selector");
            _nodeView.GraphView.PerceptionSearchWindow.Open(SetPerception);
        }

        private void ClearPerception()
        {
            UpdatePerceptionValue(null);
            UpdateView();
        }

        void SetPerception(PerceptionAsset perceptionAsset)
        {
            UpdatePerceptionValue(perceptionAsset);
            UpdateView();
        }

        void UpdatePerceptionValue(Object perceptionAsset)
        {
            var obj = new SerializedObject(nodeAsset);
            obj.FindProperty(propertyPath).objectReferenceValue = perceptionAsset;
            obj.ApplyModifiedPropertiesWithoutUndo();
        }

        SerializedProperty GetSerializedProperty()
        {
            var obj = new SerializedObject(nodeAsset);
            return obj.FindProperty(propertyPath);
        }

        void UpdateView()
        {
            var perceptionProperty = GetSerializedProperty();
            if (perceptionProperty.objectReferenceValue == null)
            {
                _assignButton.Enable();
                _container.Clear();
                _container.Disable();
            }
            else
            {
                _assignButton.Disable();
                _container.Enable();

                var perceptionAsset = perceptionProperty.objectReferenceValue as PerceptionAsset;

                var label = new Label("Perception");
                label.AddToClassList("node-text");
                _container.Add(label);
            }
        }

        public void RefreshView() => UpdateView();
    }
}
