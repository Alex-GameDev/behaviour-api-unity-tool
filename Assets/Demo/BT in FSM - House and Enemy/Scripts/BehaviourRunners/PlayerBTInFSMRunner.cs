using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.Unity.Demo
{
    public class PlayerBTInFSMRunner : BehaviourGraphRunner
    {
        protected override BehaviourGraph CreateGraph()
        {
            var fsm = new StateMachines.FSM();

            var bt = CreateFindKeySubBT();

            var doorState = fsm.CreateState("Go to home");
            var keyState = fsm.CreateState("Search key", new EnterSystemAction(bt));
            var houseState = fsm.CreateState("Enter the house");
            var runState = fsm.CreateState("Runaway");

            // Si ya tiene la llave, consigue entrar a la casa.
            fsm.CreateFinishStateTransition("Success to enter", doorState, houseState, true, false);

            // Si todavía no tiene la llave, no consigue entrar y va a buscarla.
            fsm.CreateFinishStateTransition("Fail to enter", doorState, keyState, false, true);

            // Las acciones se interrumpen si el enemigo está cerca.
            fsm.CreateTransition("Interrupt key", keyState, runState);
            fsm.CreateTransition("Interrupt to door", doorState, runState);

            // Cuando consigue la llave, vuelve al estado de abrir la puerta
            fsm.CreateFinishStateTransition("Finish key search", keyState, doorState, true, false);

            return fsm;
        }

        private BehaviourTrees.BehaviourTree CreateFindKeySubBT()
        {
            return null;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}