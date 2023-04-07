using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class DistancePerception : UnityPerception
    {
        public Transform OtherTransform;
        public float MaxDistance;

        public DistancePerception(Transform otherTransform, float maxdistance)
        {
            OtherTransform = otherTransform;
            MaxDistance = maxdistance;
        }

        public DistancePerception()
        {
        }

        public override bool Check()
        {
            return Vector3.Distance(context.Transform.position, OtherTransform.position) < MaxDistance;
        }

        public override string DisplayInfo => "if dist to $OtherTransform < $MaxDistance";
    }
}
