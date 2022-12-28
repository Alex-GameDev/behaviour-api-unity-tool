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
        VisualElement _emptyDiv, _assignedDiv;
  
        public ActionContainerView(NodeAsset asset, SerializedProperty actionProperty)
        {
            nodeAsset = asset;
            _actionProperty = actionProperty;

            AddLayout();
            SetUpContextualMenu();
            UpdateView();
        }

        void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().ContainerLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);

            _emptyDiv = this.Q("ac-empty-div");
            _assignedDiv = this.Q("ac-assigned-div");
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear action", dd => ClearAction(),
                    (_) => _actionProperty.managedReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Set subgraph action", dd => SetSubgraphAction(),
                    (_) => _actionProperty.managedReferenceValue == null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Set exit action", dd => SetExitAction(),
                    (_) => _actionProperty.managedReferenceValue == null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Set patrol action", dd => SetFleeAction(),
                    (_) => _actionProperty.managedReferenceValue == null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        private void SetFleeAction()
        {
            _actionProperty.managedReferenceValue = new PatrolAction();
            _actionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        private void SetExitAction()
        {
            _actionProperty.managedReferenceValue = new ExitAction();
            _actionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
        }

        private void SetSubgraphAction()
        {
            _actionProperty.managedReferenceValue = new SubgraphAction();
            _actionProperty.serializedObject.ApplyModifiedProperties();
            UpdateView();
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
                    _assignedDiv.Add(new SubgraphActionView(subgraphAction));
                }
                else if (action is ExitAction exitAction)
                {
                    _assignedDiv.Add(new ExitActionView(exitAction));
                }
            }
        }
    }
}
