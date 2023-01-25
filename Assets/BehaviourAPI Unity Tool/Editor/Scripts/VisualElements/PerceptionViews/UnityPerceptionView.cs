using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class UnityPerceptionView : PerceptionView<UnityPerception>
    {
        public UnityPerceptionView(UnityPerception perception) :
            base(perception, BehaviourAPISettings.instance.UnityTaskLayout)
        {
        }

        protected override void AddLayout()
        {
            this.Q<Label>("uac-label").text = _perception.DisplayInfo;
        }
    }
}
