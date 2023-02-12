using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class IsGroundedPerception : UnityPerception
    {
        public CharacterController Character;
        public override bool Check()
        {
            return Character.isGrounded;
        }

        public override string DisplayInfo => "if $Character is grounded";
    }
}
