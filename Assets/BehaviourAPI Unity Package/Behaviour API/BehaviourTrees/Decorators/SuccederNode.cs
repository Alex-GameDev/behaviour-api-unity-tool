namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Node that changes the result returned by its child node to Succeded if it's Failure.
    /// </summary>
    public class SuccederNode : DirectDecoratorNode
    {
        #region --------------------------------------- Runtime methods --------------------------------------

        protected override Status ModifyStatus(Status childStatus)
        {
            if (childStatus == Status.Failure) childStatus = Status.Success;
            return childStatus;
        }

        #endregion
    }
}