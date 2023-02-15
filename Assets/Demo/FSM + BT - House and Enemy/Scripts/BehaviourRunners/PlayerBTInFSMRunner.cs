using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Demo
{
    public class PlayerBTInFSMRunner : CodeBehaviourRunner
    {
        protected override BehaviourGraph CreateGraph()
        {
            var fsm = new StateMachines.FSM();

            var bt = CreateFindKeySubBT();

            var doorState = fsm.CreateState("Go to home");
            var keyState = fsm.CreateState("Search key", new SubsystemAction(bt));
            var houseState = fsm.CreateState("Enter the house");
            var runState = fsm.CreateState("Runaway");

            // Si ya tiene la llave, consigue entrar a la casa.
            fsm.CreateTransition("Success to enter", doorState, houseState, new ExecutionStatusPerception(doorState, StatusFlags.Success));

            // Si todavía no tiene la llave, no consigue entrar y va a buscarla.
            fsm.CreateTransition("Fail to enter", doorState, keyState, new ExecutionStatusPerception(doorState, StatusFlags.Failure));

            // Las acciones se interrumpen si el enemigo está cerca.
            fsm.CreateTransition("Interrupt key", keyState, runState);
            fsm.CreateTransition("Interrupt to door", doorState, runState);

            // Cuando consigue la llave, vuelve al estado de abrir la puerta
            fsm.CreateTransition("Finish key search", keyState, doorState, new ExecutionStatusPerception(keyState, StatusFlags.Success));

            RegisterGraph(fsm);
            return fsm;
        }

        private BehaviourTrees.BehaviourTree CreateFindKeySubBT()
        {
            return null;
        }
    }

}