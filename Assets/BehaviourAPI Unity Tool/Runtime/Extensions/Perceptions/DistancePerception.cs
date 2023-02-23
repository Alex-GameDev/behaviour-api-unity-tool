using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class DistancePerception : UnityPerception
    {
        public Transform OtherTransform;
        public float Maxdistance;

        public override bool Check()
        {
            return Vector3.Distance(context.Transform.position, OtherTransform.position) < Maxdistance;
        }

        public override string DisplayInfo => "if dist < $maxDistance";
    }
}
