using BehaviourAPI.Core;
using Action = BehaviourAPI.Core.Actions.Action;


namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class CustomAction : Action
    {
        public SerializedAction start;
        public SerializedStatusFunction update;
        public SerializedAction stop;

        public override void SetExecutionContext(ExecutionContext context)
        {
            return;
        }

        public override void Start() => start.GetFunction()?.Invoke();

        public override void Stop() => stop.GetFunction()?.Invoke();

        public override Status Update() => update.GetFunction()?.Invoke() ?? Status.Running;


    }
}
