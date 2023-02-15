﻿using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerFSMRunner : CodeBehaviourRunner
{
    #region variables

    [SerializeField] private float minDistanceToChicken = 5;
    [SerializeField] private Transform chicken;

    private NavMeshAgent meshAgent;
    private PushPerception _click;

    #endregion variables

    protected override void OnAwake()
    {
        meshAgent = GetComponent<NavMeshAgent>();
        base.OnAwake();
    }

    protected override BehaviourGraph CreateGraph()
    {
        var fsm = new BehaviourAPI.StateMachines.FSM();

        // Percepciones
        var chickenNear = new ConditionPerception(CheckDistanceToChicken);

        // Estados
        var idle = fsm.CreateState("Idle");
        var moving = fsm.CreateState("Moving", new MoveToMousePosAction(meshAgent, 3.5f));
        var flee = fsm.CreateState("Flee", new FleeAction(meshAgent, 7f, 13f, 3f));

        // Las transiciones que pasan al estado "moving" se activan con percepciones Push.
        var idleToMoving = fsm.CreateTransition("idle to moving", idle, moving, action: new FunctionalAction(() => Debug.Log("MOVE")), statusFlags: StatusFlags.None);
        var movingToMoving = fsm.CreateTransition("moving to moving", moving, moving, action: new FunctionalAction(() => Debug.Log("MOVE AGAIN")), statusFlags: StatusFlags.None);
        _click = new PushPerception(idleToMoving, movingToMoving);

        // La transición que pasan al estado "idle" se lanzan cuando la acción de "moving" o "flee" termine.
        fsm.CreateTransition("moving to idle", moving, idle, statusFlags: StatusFlags.Finished);
        fsm.CreateTransition("runaway to idle", flee, idle, statusFlags: StatusFlags.Finished);

        // Las transiciones que pasan al estado "flee" se activan con la percepción "chicken near"
        fsm.CreateTransition("idle to runaway", idle, flee, chickenNear);
        fsm.CreateTransition("moving to runaway", moving, flee, chickenNear);

        RegisterGraph(fsm);
        return fsm;
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Fire");
            _click.Fire();
        }
        base.OnUpdate();
    }

    private bool CheckDistanceToChicken()
    {
        return Vector3.Distance(transform.position, chicken.transform.position) < minDistanceToChicken;
    }

}