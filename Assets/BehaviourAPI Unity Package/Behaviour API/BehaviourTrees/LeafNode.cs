﻿using BehaviourAPI.Core;

namespace BehaviourAPI.BehaviourTrees
{
    using Core.Actions;

    /// <summary>
    /// BTNode type that has no children and executes an <see cref="Core.Actions.Action"/>.
    /// </summary>
    public class LeafNode : BTNode
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public sealed override int MaxOutputConnections => 0;

        /// <summary>
        /// The action executed by this node.
        /// </summary>
        public Action Action;

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public override object Clone()
        {
            var node = (LeafNode)base.Clone();
            node.Action = (Action)Action?.Clone();
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        /// <summary>
        /// <inheritdoc/>
        /// Starts the action execution.
        /// </summary>
        /// <exception cref="MissingActionException">If the action is null</exception>
        public override void Start()
        {
            base.Start();
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Start();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Updates the action execution.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MissingActionException"></exception>
        protected override Status UpdateStatus()
        {
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Status = Action.Update();
            return Status;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stops the action execution.
        /// </summary>
        /// <exception cref="MissingActionException"></exception>
        public override void Stop()
        {
            base.Stop();

            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Stop();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Pass the execution context to the action.
        /// </summary>
        /// <param name="context"><inheritdoc/></param>
        public override void SetExecutionContext(ExecutionContext context)
        {
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.SetExecutionContext(context);
        }

        public override void Pause()
        {
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Pause();
        }

        public override void Unpause()
        {
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Unpause();
        }

        #endregion
    }
}
