using BehaviourAPI.StateMachines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRadar
{
    public State GetBrokenState();
    public State GetWorkingState();
}
