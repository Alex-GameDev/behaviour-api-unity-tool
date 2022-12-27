using BehaviourAPI.Unity.Runtime;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class CustomActionView : ActionView<CustomAction>
    {
        public CustomActionView(CustomAction customAction) :
            base(customAction, VisualSettings.GetOrCreateSettings().CustomActionLayout)
        {
        }

        protected override void AddLayout()
        {
            this.Q<Label>("cac-label").text = "Custom action";
        }
    }
}
