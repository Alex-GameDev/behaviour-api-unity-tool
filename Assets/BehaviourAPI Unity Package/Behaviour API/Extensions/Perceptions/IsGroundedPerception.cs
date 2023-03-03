using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class IsGroundedPerception : UnityPerception
    {
        public override bool Check()
        {
            return context.CharacterController.isGrounded;
        }

        public override string DisplayInfo => "if is grounded";
    }
}
