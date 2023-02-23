using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
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
