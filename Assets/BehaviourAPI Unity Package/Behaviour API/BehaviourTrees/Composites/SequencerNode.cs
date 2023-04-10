namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Serial Composite node that executes its children until one of them returns Failure.
    /// </summary>
    public class SequencerNode : SerialNode
    {
        protected override bool KeepExecuting(Status status)
        {
            return status == Status.Success;
        }

        protected override Status GetFinalStatus(Status status)
        {
            return status;
        }
    }
}