using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;
    public static class Utilities
    {
        public static Color ToColor(this Status status)
        {
            if (status == Status.Success) return Color.green;
            if (status == Status.Failure) return Color.red;
            if (status == Status.Running) return Color.yellow;
            return Color.gray;
        }
    }
}
