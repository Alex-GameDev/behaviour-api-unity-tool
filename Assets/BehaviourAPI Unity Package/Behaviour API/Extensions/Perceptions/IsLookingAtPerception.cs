using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class IsLookingAtPerception : UnityPerception
    {
        public Transform OtherTransform;

        public float minDist, maxDist;
        public float maxAngle;

        public override bool Check()
        {
            var delta = OtherTransform.position - context.Transform.position;

            if (delta.magnitude < minDist || delta.magnitude > maxDist) return false;

            var lookAt = context.Transform.forward;

            return Vector3.Angle(lookAt, delta) < maxAngle;
        }

        public override string DisplayInfo => "if is looking at $Other";
    }
}
