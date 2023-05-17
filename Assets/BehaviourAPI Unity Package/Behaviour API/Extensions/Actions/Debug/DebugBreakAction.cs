using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that pauses the execution of the unity editor when started.
    /// </summary>

    [SelectionGroup("DEBUG")]
    public class DebugBreakAction : UnityAction
    {
        public override string DisplayInfo => "Debug Break";

        /// <summary>
        /// Create a Debug breaak
        /// </summary>
        public DebugBreakAction()
        {
        }

        public override void Start()
        {
            Debug.Break();
        }

        public override Status Update()
        {
            return Status.Success;
        }
    }
}
