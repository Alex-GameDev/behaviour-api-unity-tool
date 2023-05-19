using System.Collections.Generic;

namespace BehaviourAPI.Core.Actions
{
    /// <summary>
    /// Action that runs a sequence of actions until one of them returns the specified Status value or the entire sequence is executed.
    /// </summary>
    public class SequenceAction : CompoundAction
    {
        public Status TargetStatus;

        int currentChildIdx = 0;

        public SequenceAction(Status targetStatus, List<Action> subActions) : base(subActions)
        {
            TargetStatus = targetStatus;
        }

        public SequenceAction() : base()
        {
            TargetStatus = Status.Failure;
        }

        public SequenceAction(Status targetStatus, params Action[] subActions) : base(subActions)
        {
            TargetStatus = targetStatus;
        }

        public override void Start()
        {
            if (SubActions.Count == 0) return;
            SubActions[0].Start();
        }

        public override Status Update()
        {
            Action currentAction = SubActions[currentChildIdx];
            var status = currentAction.Update();

            if (status == TargetStatus && currentChildIdx < SubActions.Count - 1)
            {
                currentAction.Stop();
                currentChildIdx++;
                currentAction = SubActions[currentChildIdx];
                currentAction.Start();
                return Status.Running;
            }
            else
            {
                return status;
            }
        }

        public override void Stop()
        {
            currentChildIdx = 0;
            SubActions[currentChildIdx].Stop();
        }
    }
}
