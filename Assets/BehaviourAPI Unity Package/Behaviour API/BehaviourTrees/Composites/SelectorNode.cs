namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Serial Composite node that executes its children until one of them returns Succeded.
    /// </summary>
    public class SelectorNode : SerialNode
    {
        protected override bool KeepExecuting(Status status)
        {
            return status == Status.Failure;
        }

        protected override Status GetFinalStatus(Status status)
        {
            return status;
        }
    }
}