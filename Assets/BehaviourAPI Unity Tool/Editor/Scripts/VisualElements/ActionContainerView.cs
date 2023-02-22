using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
        string propertyPath;

        NodeView _nodeView;

        Button _assignButton;
        VisualElement _container;

        Button _assignSubgraphBtn, _removeSubgraphBtn;
        Label _subgraphLabel;
  
        public ActionContainerView(NodeAsset asset, SerializedProperty actionProperty, NodeView nodeView)
        {
            nodeAsset = asset;
            propertyPath = actionProperty.propertyPath;

            _nodeView = nodeView;
            AddLayout();
            SetUpContextualMenu();
            UpdateView();
        }

        void AddLayout()
        {
            _container = new VisualElement();
            Add(_container);

            _assignButton = new Button(OnAssignAction) { text = "Assign action" };
            Add(_assignButton);
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear action", dd => ClearAction(),
                    (_) => GetSerializedProperty().managedReferenceValue != null ?
                    DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        void OnAssignAction()
        {
            _nodeView.GraphView.ActionCreationWindow.Open(SetActionType);
        }

        private void ClearAction()
        {
            UpdateActionValue(null);
            UpdateView();
        }

        void SetActionType(Type actionType)
        {
            if (!actionType.IsSubclassOf(typeof(Action))) return;

            UpdateActionValue(Activator.CreateInstance(actionType));
            UpdateView();
        }

        void UpdateActionValue(object action)
        {
            var obj = new SerializedObject(nodeAsset);
            obj.FindProperty(propertyPath).managedReferenceValue = action;
            obj.ApplyModifiedPropertiesWithoutUndo();

            BehaviourEditorWindow.Instance.OnModifyAsset();
        }

        SerializedProperty GetSerializedProperty()
        {
            var obj = new SerializedObject(nodeAsset);
            return obj.FindProperty(propertyPath);
        }

        void UpdateView()
        {

            var actionProperty = GetSerializedProperty();
            if (actionProperty.managedReferenceValue == null)
            {
                _assignButton.Enable();
                _container.Clear();
                _container.Disable();
            }
            else
            {
                _assignButton.Disable();
                _container.Enable();

                Action action = actionProperty.managedReferenceValue as Action;

                if (action is CustomAction)
                {
                    var label = new Label("Custom Action");
                    label.AddToClassList("node-text");
                    _container.Add(label);
                }
                else if(action is ContextCustomAction)
                {
                    var label = new Label("Custom Action (context)");
                    label.AddToClassList("node-text");
                    _container.Add(label);
                }
                else if (action is UnityAction unityAction)
                {
                    var label = new Label(unityAction.DisplayInfo);
                    label.AddToClassList("node-text");
                    _container.Add(label);
                }
                else if (action is SubgraphAction subgraphAction)
                {
                    DisplaySubgraphAction(subgraphAction);
                }
            }
        }

        void DisplaySubgraphAction(SubgraphAction subgraphAction)
        {
            _subgraphLabel = new Label("-");
            _subgraphLabel.bindingPath = "Name";
            _subgraphLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _container.Add(_subgraphLabel);



            _assignSubgraphBtn = new Button(() => OpenGraphSelectionMenu(subgraphAction)) { text = "Assign subgraph" };
            _removeSubgraphBtn = new Button(() => RemoveSubgraph(subgraphAction)) { text = "Remove subgraph" };
            _container.Add(_assignSubgraphBtn);
            _container.Add(_removeSubgraphBtn);

            UpdateSubgraphLayout(subgraphAction);
        }
        void OpenGraphSelectionMenu(SubgraphAction subgraphAction)
        {
            // TODO: A�adir men� para elegir subgrafo y llamar al m�todo SetSubgraph
            _nodeView.GraphView.SubgraphSearchWindow.OpenWindow((g) => SetSubgraph(g, subgraphAction), (g) => g != _nodeView.GraphView.GraphAsset);
        }

        void SetSubgraph(GraphAsset graphAsset, SubgraphAction subgraphAction)
        {
            subgraphAction.Subgraph = graphAsset;
            UpdateSubgraphLayout(subgraphAction);
            BehaviourEditorWindow.Instance.OnModifyAsset();
        }

        void RemoveSubgraph(SubgraphAction subgraphAction)
        {
            subgraphAction.Subgraph = null;
            UpdateSubgraphLayout(subgraphAction);
            BehaviourEditorWindow.Instance.OnModifyAsset();
        }

        void UpdateSubgraphLayout(SubgraphAction subgraphAction)
        {
            var subgraph = subgraphAction.Subgraph;
            if (subgraph != null)
            {
                _subgraphLabel.Bind(new SerializedObject(subgraphAction.Subgraph));
                _assignSubgraphBtn.Disable();
                _removeSubgraphBtn.Enable();
            }
            else
            {
                _subgraphLabel.Unbind();
                _subgraphLabel.text = "-";
                _assignSubgraphBtn.Enable();
                _removeSubgraphBtn.Disable();
            }

        }
    }
}
