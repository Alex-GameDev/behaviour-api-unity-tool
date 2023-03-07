﻿using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using BehaviourAPI.UnityExtensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChickenFSMRunner : CodeBehaviourRunner
{
    #region variables

    [SerializeField] Transform _target;
    [SerializeField] Collider _visionCollider;
    NavMeshAgent _agent;

    #endregion variables

    protected override void OnAwake()
    {
        _agent = GetComponent<NavMeshAgent>();
        base.OnAwake();
    }

    protected override BehaviourGraph CreateGraph()
    {
        var fsm = new BehaviourAPI.StateMachines.FSM();

        // Percepciones pull
        var chickenNear = new ConditionPerception(CheckWatchTarget);
        var timeToStartMoving = new UnityTimePerception(3f);

        // Estados
        var idle = fsm.CreateState("Idle");
        var moving = fsm.CreateState("Moving", new PatrolAction(3f, 13f));
        var chasing = fsm.CreateState("Chasing", new ChaseAction(_target, 5f, 10f, 2.5f));

        // Las transiciones que pasan al estado moving se lanzan con un temporizador:
        var idleToMoving = fsm.CreateTransition("idle to moving", idle, moving, timeToStartMoving);

        // Las transiciones que pasan al estado "idle" se lanzan cuando la acción "moving" o "chase" termine.
        fsm.CreateTransition("moving to idle", moving, idle, statusFlags: StatusFlags.Finished);
        fsm.CreateTransition("runaway to idle", chasing, idle, statusFlags: StatusFlags.Finished);

        // Las transiciones que pasan al estado "chasing" se activan con la percepción "watchPlayer".
        fsm.CreateTransition("idle to runaway", idle, chasing, chickenNear);
        fsm.CreateTransition("moving to runaway", moving, chasing, chickenNear);

        RegisterGraph(fsm);
        return fsm;
    }

    private bool CheckWatchTarget()
    {
        if (_visionCollider.bounds.Contains(_target.position))
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            Ray ray = new Ray(transform.position + transform.up, direction * 20);

            bool watchPlayer = Physics.Raycast(ray, out RaycastHit hit, 20) && hit.collider.gameObject.transform == _target;

            return watchPlayer;
        }
        return false;
    }
}