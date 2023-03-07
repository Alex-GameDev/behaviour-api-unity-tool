﻿using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;

namespace BehaviourAPI.BehaviourTrees
{
    using BehaviourAPI.Core.Exceptions;
    using Core.Actions;
    /// <summary>
    /// BTNode type that has no children.
    /// </summary>
    public class LeafNode : BTNode
    {
        #region ------------------------------------------ Properties -----------------------------------------
        public sealed override int MaxOutputConnections => 0;
        public Action Action; 

        #endregion

        #region ---------------------------------------- Build methods ---------------------------------------

        public LeafNode SetAction(Action action)
        {
            Action = action;
            return this;
        }

        public override object Clone()
        {
            var node = (LeafNode) base.Clone();
            node.Action = (Action)Action?.Clone();
            return node;
        }

        #endregion

        #region --------------------------------------- Runtime methods --------------------------------------

        public override void Start()
        {
            base.Start();
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Start();
        }

        protected override Status UpdateStatus()
        {
            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Status = Action.Update();
            return Status;
        }

        public override void Stop()
        {
            base.Stop();

            if (Action == null)
                throw new MissingActionException(this, "Leaf nodes need an action to work.");

            Action.Stop();
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            Action?.SetExecutionContext(context);
        }

        #endregion
    }
}