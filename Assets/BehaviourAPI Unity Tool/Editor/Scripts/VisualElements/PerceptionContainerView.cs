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
        VisualElement _emptyDiv, _assignedDiv;

        Button _assignBtn;

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
            var visualTree = VisualSettings.GetOrCreateSettings().ContainerLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);

            _emptyDiv = this.Q("tc-empty-div");
            _assignedDiv = this.Q("tc-assigned-div");

            _assignBtn = this.Q<Button>("tc-assign-button");
            _assignBtn.text = "Assign perception";
            _assignBtn.clicked += OnAssignPerception;
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
                _emptyDiv.style.display = DisplayStyle.Flex;
                _assignedDiv.style.display = DisplayStyle.None;
                _assignedDiv.Clear();
            }
            else
            {
                _emptyDiv.style.display = DisplayStyle.None;
                _assignedDiv.style.display = DisplayStyle.Flex;
                Perception perception = _perceptionProperty.managedReferenceValue as Perception;

                if (perception is CustomPerception customAction)
                {
                    _assignedDiv.Add(new CustomPerceptionView(customAction));
                }
                else if (perception is UnityPerception unityAction)
                {
                    _assignedDiv.Add(new UnityPerceptionView(unityAction));
                }
                else if (perception is CompoundPerception compoundPerception)
                {
                    _assignedDiv.Add(new CompoundPerceptionView(compoundPerception));
                }
                else if (perception is StatusPerception statusPerception)
                {
                    _assignedDiv.Add(new StatusPerceptionView(statusPerception));
                }
            }
        }
    }
}
