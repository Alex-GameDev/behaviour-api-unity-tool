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

        /// <summary>
        /// <inheritdoc/>
        /// If <paramref name="childStatus"/> is failure returns success.
        /// </summary>
        /// <param name="childStatus"></param>
        /// <returns>Success if <paramref name="childStatus"/> is failure, <paramref name="childStatus"/> otherwise.</returns>
        protected override Status ModifyStatus(Status childStatus)
        {
            if (childStatus == Status.Failure) childStatus = Status.Success;
            return childStatus;
        }

        #endregion
    }
}