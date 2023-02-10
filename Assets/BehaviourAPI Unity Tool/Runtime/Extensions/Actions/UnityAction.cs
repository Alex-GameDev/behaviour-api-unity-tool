using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public abstract class UnityAction : Action
    {
        public virtual string DisplayInfo => "Unity Action";
    }
}
