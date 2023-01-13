using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
{
    public class FinishExecutionTransition : BehaviourAPI.StateMachines.StateTransition
    {
        [SerializeField] StatusFlags _statusFlags;
        [SerializeReference] Action _action;
    }
}
