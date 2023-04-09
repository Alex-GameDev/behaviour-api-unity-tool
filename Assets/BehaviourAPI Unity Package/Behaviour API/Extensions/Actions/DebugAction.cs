using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action to print a message in the debug console.
    /// </summary>
    public class DebugAction : UnityAction
    {
        /// <summary>
        /// The message printed
        /// </summary>
        public string message;

        public override string DisplayInfo => "Debug Log \"$message\"";


        /// <summary>
        /// Create a DebugAction
        /// </summary>
        /// <param name="message">The message printed</param>
        public DebugAction(string message)
        {
            this.message = message;
        }

        public override Status Update()
        {
            Debug.Log(message, context.GameObject);
            return Status.Success;
        }
    }
}
