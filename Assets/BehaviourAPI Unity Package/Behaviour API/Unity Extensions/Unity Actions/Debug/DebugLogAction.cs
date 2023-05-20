using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action to print a message in the debug console.
    /// </summary>
    [SelectionGroup("DEBUG")]
    public class DebugLogAction : UnityAction
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
        public DebugLogAction(string message)
        {
            this.message = message;
        }

        public override void Start()
        {
            Debug.Log(message, context.GameObject);
        }

        public override Status Update()
        {
            return Status.Success;
        }
    }
}