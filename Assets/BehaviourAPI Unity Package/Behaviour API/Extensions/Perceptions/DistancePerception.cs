using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class DistancePerception : UnityPerception
    {
        public Transform OtherTransform;
        public float Maxdistance;

        public DistancePerception(Transform otherTransform, float maxdistance)
        {
            OtherTransform = otherTransform;
            Maxdistance = maxdistance;
        }

        public DistancePerception()
        {
        }

        public override bool Check()
        {
            return Vector3.Distance(context.Transform.position, OtherTransform.position) < Maxdistance;
        }

        public override string DisplayInfo => "if dist < $maxDistance";
    }
}
