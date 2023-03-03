namespace BehaviourAPI.BehaviourTrees
{
    using Core;
    using Core.Exceptions;

    /// <summary>
    /// Node that inverts the result returned by its child node (Success/Failure).
    /// </summary>

    public class InverterNode : DirectDecoratorNode
    {
        #region --------------------------------------- Runtime methods --------------------------------------

        protected override Status ModifyStatus(Status childStatus)
        {
            return childStatus.Inverted();
        }

        #endregion
    }
}