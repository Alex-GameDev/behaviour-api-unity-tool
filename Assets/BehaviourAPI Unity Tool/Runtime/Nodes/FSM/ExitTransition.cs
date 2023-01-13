using BehaviourAPI.Core.Perceptions;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
{
    public class ExitTransition : BehaviourAPI.StateMachines.ExitTransition
    {
        [SerializeReference] Action _action;
        [SerializeReference] Perception perception;
    }
}
