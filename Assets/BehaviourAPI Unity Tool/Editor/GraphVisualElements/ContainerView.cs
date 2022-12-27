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
    public class ContainerView : VisualElement
    {
        NodeAsset nodeAsset;
        SerializedProperty _actionProperty;
        VisualElement _assignDiv;
        VisualElement _actionDiv;
  
        public ContainerView(NodeAsset asset, SerializedProperty actionProperty)
        {
            nodeAsset = asset;
            _actionProperty = actionProperty;
            AddLayout();
            SetUpContextualMenu();
        }

        void AddLayout()
        {
            var visualTree = VisualSettings.GetOrCreateSettings().ContainerLayout;
            var inspectorFromUXML = visualTree.Instantiate();
            Add(inspectorFromUXML);
            _assignDiv = this.Q("container-assign-div");
            _assignDiv = this.Q("container-action-div");
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Change to patrol action", dd => SetPatrolAction());
                menuEvt.menu.AppendAction("Change to flee action", dd => SetFleeAction());
            }));
        }

        private void SetFleeAction()
        {
            _actionProperty.managedReferenceValue = null;
            _actionProperty.serializedObject.ApplyModifiedProperties();
        }

        private void SetPatrolAction()
        {
            _actionProperty.managedReferenceValue = new PatrolAction();
            _actionProperty.serializedObject.ApplyModifiedProperties();
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
                // Display assign action button
            }

            Action action = _actionProperty.managedReferenceValue as Action;

            if (action is CustomAction customAction)
            {
                // label.text = action.DisplayInfo;
            }
            else if (action is UnityAction unityAction)
            {
                // label.text = action.DisplayInfo;
            }
            else if(action is SubgraphAction subgraphAction)
            {
                //
            }
            else if(action is ExitAction exitAction)
            {
                //
            }
        }
    }
}
