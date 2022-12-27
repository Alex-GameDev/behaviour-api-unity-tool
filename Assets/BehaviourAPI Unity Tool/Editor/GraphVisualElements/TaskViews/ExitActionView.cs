using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class ExitActionView : ActionView<ExitAction>
    {
        public ExitActionView(ExitAction exitAction) :
            base(exitAction, VisualSettings.GetOrCreateSettings().ExitActionLayout)
        {
        }

        protected override void AddLayout()
        {
            var statusDropdown = this.Q<DropdownField>("ec-status-dropdown");
            statusDropdown.bindingPath = "ExitStatus";
        } 
    }
}
