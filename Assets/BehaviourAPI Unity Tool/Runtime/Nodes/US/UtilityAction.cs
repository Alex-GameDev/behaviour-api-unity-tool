using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPIUnityTool.UtilitySystems
{
    public class UtilityAction : BehaviourAPI.UtilitySystems.UtilityAction
    {
        [SerializeReference] Action _action;
    }
}
