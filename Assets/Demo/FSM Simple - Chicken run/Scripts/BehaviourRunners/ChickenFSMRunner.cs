using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;
using UnityEngine.AI;

public class ChickenFSMRunner : BehaviourGraphRunner
{
    #region variables

    [SerializeField] Transform _target;
    [SerializeField] Collider _visionCollider;
    NavMeshAgent _agent;

    BehaviourAPI.StateMachines.FSM fsm;

    #endregion variables

    protected override void OnAwake()
    {
        _agent = GetComponent<NavMeshAgent>();
        base.OnAwake();
    }

    protected override BehaviourGraph CreateGraph()
    {
        fsm = new BehaviourAPI.StateMachines.FSM();

        // Percepciones pull
        var chickenNear = new ConditionPerception(CheckWatchTarget);
        var timeToStartMoving = new UnityTimePerception(3f);

        // Estados
        var idle = fsm.CreateState("Idle");
        var moving = fsm.CreateState("Moving", new PatrolAction(_agent, 3f, 13f));
        var chasing = fsm.CreateState("Chasing", new ChaseAction(_agent, _target, 5f, 10f, 2.5f));

        // Las transiciones que pasan al estado moving se lanzan con un temporizador:
        var idleToMoving = fsm.CreateTransition("idle to moving", idle, moving, timeToStartMoving);

        // Las transiciones que pasan al estado "idle" se lanzan cuando la acción "moving" o "chase" termine.
        fsm.CreateFinishStateTransition("moving to idle", moving, idle, true, true);
        fsm.CreateFinishStateTransition("runaway to idle", chasing, idle, true, true);

        // Las transiciones que pasan al estado "chasing" se activan con la percepción "watchPlayer".
        fsm.CreateTransition("idle to runaway", idle, chasing, chickenNear);
        fsm.CreateTransition("moving to runaway", moving, chasing, chickenNear);

        return fsm;
    }

    private bool CheckWatchTarget()
    {
        if (_visionCollider.bounds.Contains(_target.position))
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            Ray ray = new Ray(transform.position + transform.up, direction * 20);

            bool watchPlayer = Physics.Raycast(ray, out RaycastHit hit, 20) && hit.collider.gameObject.transform == _target;

            if (watchPlayer) Debug.Log("Player watched");

            return watchPlayer;
        }
        return false;
    }
}