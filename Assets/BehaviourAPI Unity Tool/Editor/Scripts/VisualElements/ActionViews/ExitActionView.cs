using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor
{
    public class ExitActionView : ActionView<ExitAction>
    {
        DropdownField statusDropdown;
        public ExitActionView(ExitAction exitAction) :
            base(exitAction, BehaviourAPISettings.instance.ExitActionLayout)
        {            
        }

        protected override void AddLayout()
        {
            statusDropdown = this.Q<DropdownField>("ec-status-dropdown");
            statusDropdown.choices = new List<string> { "Success", "Failure" };
            statusDropdown.RegisterValueChangedCallback(StatusValueChanged);
            statusDropdown.index = _action.Status == Status.Success ? 0 : 1;
        }

        private void StatusValueChanged(ChangeEvent<string> evt)
        {
            if(evt.newValue == "Success")
            {
                _action.Status = Status.Success;
            }
            else
            {
                _action.Status = Status.Failure;
            }
        }
    }
}
