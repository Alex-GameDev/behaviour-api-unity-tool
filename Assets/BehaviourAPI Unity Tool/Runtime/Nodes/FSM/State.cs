using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
{
    public class State : BehaviourAPI.StateMachines.State
    {
        [SerializeReference] Action _action;
    }
}
