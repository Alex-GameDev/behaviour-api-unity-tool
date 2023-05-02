using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    /// <summary>
    /// Check if the agent is grounded. 
    /// <para>Requires <see cref="CharacterController"/> component.</para>
    /// </summary>
    public class IsGroundedPerception : UnityPerception
    {
        public override string DisplayInfo => "is grounded";

        public override bool Check()
        {
            return context.CharacterController.isGrounded;
        }
    }
}
