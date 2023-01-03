using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Displays a <see cref="Action"/> from a <see cref="Node"/> inside a <see cref="NodeView"/>
    /// </summary>
    public class ActionContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        SerializedProperty _actionProperty;

        NodeView _nodeView;
        VisualElement _emptyDiv, _assignedDiv;
  
        public ActionContainerView(NodeAsset asset, SerializedProperty actionProperty, NodeView nodeView)
        {
            nodeAsset = asset;
            _actionProperty = actionProperty;

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

            this.Q<Button>("tc-assign-button").clicked += OnAssignAction;
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear action", dd => ClearAction(),
                    (_) => _actionProperty.managedReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        void OnAssignAction()
        {
            _nodeView.GraphView.ActionSearchWindow.Open(SetActionType);
        }

        private void ClearAction()
        {
            _actionProperty.managedReferenceValue = null;
            _actionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        void SetActionType(Type actionType)
        {
            if (!actionType.IsSubclassOf(typeof(Action))) return;

            _actionProperty.managedReferenceValue = Activator.CreateInstance(actionType);
            _actionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        void UpdateView()
        {
            if (_actionProperty.managedReferenceValue == null)
            {
                _emptyDiv.style.display = DisplayStyle.Flex;
                _assignedDiv.style.display = DisplayStyle.None;
                _assignedDiv.Clear();
            }
            else
            {
                _emptyDiv.style.display = DisplayStyle.None;
                _assignedDiv.style.display = DisplayStyle.Flex;
                Action action = _actionProperty.managedReferenceValue as Action;

                if (action is CustomAction customAction)
                {
                    _assignedDiv.Add(new CustomActionView(customAction));
                }
                else if (action is UnityAction unityAction)
                {
                    _assignedDiv.Add(new UnityActionView(unityAction));
                }
                else if (action is SubgraphAction subgraphAction)
                {
                    _assignedDiv.Add(new SubgraphActionView(subgraphAction, _nodeView));
                }
                else if (action is ExitAction exitAction)
                {
                    _assignedDiv.Add(new ExitActionView(exitAction));
                }
            }
        }
    }
}
