using System;
using System.Collections.Generic;
using System.Text;

namespace BehaviourAPI.BehaviourTrees
{
    using Core;

    /// <summary>
    /// Composite node that selects one of its branch to execute it.
    /// </summary>
    public abstract class BranchNode : CompositeNode
    {
        /// <summary>
        /// Use this property to read and modify the current selected node.
        /// </summary>
        /// <value>The current selected node.</value>
        protected BTNode SelectedNode { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// Select a branch and starts it.
        /// </summary>
        public override void OnStarted()
        {
            base.OnStarted();

            SelectedNode = SelectBranch() ?? GetBTChildAt(0);

            SelectedNode?.OnStarted();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Stops the selected branch node.
        /// </summary>
        public override void OnStopped()
        {
            base.OnStopped();
            SelectedNode?.OnStopped();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Pauses the selected branch node.
        /// </summary>
        public override void OnPaused()
        {
            SelectedNode?.OnPaused();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Unpauses the selected branch node.
        /// </summary>
        public override void OnUnpaused()
        {
            SelectedNode?.OnUnpaused();
        }

        /// <summary>
        /// <inheritdoc/>
        /// Returns the status of its selected branch.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override Status UpdateStatus()
        {
            SelectedNode.OnUpdated();
            return SelectedNode?.Status ?? Status.Failure;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Override this method to define how to select the branch that will be executed.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected abstract BTNode SelectBranch();
    }
}
