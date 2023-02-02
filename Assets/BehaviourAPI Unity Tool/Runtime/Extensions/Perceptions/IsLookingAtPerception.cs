using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class IsLookingAtPerception : UnityPerception
    {
        public Transform SelfTransform;
        public Transform OtherTransform;

        public float minDist, maxDist;
        public float maxAngle;

        public override bool Check()
        {
            var delta = OtherTransform.position - SelfTransform.position;

            if (delta.magnitude < minDist || delta.magnitude > maxDist) return false;

            var lookAt = SelfTransform.forward;

            return Vector3.Angle(lookAt, delta) < maxAngle;
        }

        public override string DisplayInfo => "if $Self is looking at $Other";
    }
}
