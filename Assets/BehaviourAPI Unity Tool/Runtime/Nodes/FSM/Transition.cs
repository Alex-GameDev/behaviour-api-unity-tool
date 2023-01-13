using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
{
    public class Transition : BehaviourAPI.StateMachines.StateTransition
    {
        [SerializeReference] Action _action;
        [SerializeReference] Perception perception;
    }
}
