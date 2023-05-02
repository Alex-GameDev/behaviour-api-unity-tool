using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public interface IAgentMovement
    {
        Status Move(Vector3 targetPos);
        Status Move(Vector3 targetPos, Quaternion targetRot);
    }
}
