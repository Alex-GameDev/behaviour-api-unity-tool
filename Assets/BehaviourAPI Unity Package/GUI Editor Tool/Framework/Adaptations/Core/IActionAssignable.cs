using BehaviourAPI.Core.Actions;

namespace behaviourAPI.Unity.Framework.Adaptations
{
    public interface IActionAssignable
    {
        public Action ActionReference { get; set; }
    }
}
