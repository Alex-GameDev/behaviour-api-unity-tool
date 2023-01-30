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
using CompoundPerception = BehaviourAPI.Unity.Framework.Adaptations.CompoundPerception;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Displays a <see cref="Action"/> from a <see cref="Node"/> inside a <see cref="NodeView"/>
    /// </summary>
    public class PerceptionContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        SerializedProperty _perceptionProperty;

        NodeView _nodeView;

        Button _assignButton;
        VisualElement _container;

        public PerceptionContainerView(NodeAsset asset, SerializedProperty perceptionProperty, NodeView nodeView)
        {
            nodeAsset = asset;
            _perceptionProperty = perceptionProperty;

            _nodeView = nodeView;
            AddLayout();
            SetUpContextualMenu();
            UpdateView();
        }

        void AddLayout()
        {
            _container = new VisualElement();
            Add(_container);

            _assignButton = new Button(OnAssignPerception) { text = "Assign action" };
            Add(_assignButton);
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear perception", dd => ClearPerception(),
                    (_) => _perceptionProperty.managedReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        void OnAssignPerception()
        {
            _nodeView.GraphView.PerceptionSearchWindow.Open(SetPerceptionType);
        }

        private void ClearPerception()
        {
            _perceptionProperty.managedReferenceValue = null;
            _perceptionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        void SetPerceptionType(Type perceptionType)
        {
            if (!perceptionType.IsSubclassOf(typeof(Perception))) return;

            _perceptionProperty.managedReferenceValue = Activator.CreateInstance(perceptionType);
            _perceptionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        void UpdateView()
        {
            if (_perceptionProperty.managedReferenceValue == null)
            {
                _assignButton.Enable();
                _container.Clear();
                _container.Disable();
            }
            else
            {
                _assignButton.Disable();
                _container.Enable();

                Perception perception = _perceptionProperty.managedReferenceValue as Perception;

                if (perception is CustomPerception customAction)
                {
                    var label = new Label("Custom Perception");
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                    _container.Add(label);
                }
                else if (perception is UnityPerception unityAction)
                {
                    var label = new Label(unityAction.DisplayInfo);
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                    _container.Add(label);
                }
                else if (perception is CompoundPerception compoundPerception)
                {
                    
                }
                else if (perception is StatusPerception statusPerception)
                {

                }
            }
        }
    }
}
