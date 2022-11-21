using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;
using UnityEngine.AI;

public class PlayerFSMRunner : BehaviourGraphRunner
{
    #region variables

    [SerializeField] private float minDistanceToChicken = 5;
    [SerializeField] private Transform chicken;

    private NavMeshAgent meshAgent;
    private BehaviourAPI.Core.Perceptions.PushPerception _click;

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
        var idleToMoving = fsm.CreateTransition("idle to moving", idle, moving);
        var movingToMoving = fsm.CreateTransition("moving to moving", moving, moving);
        _click = new BehaviourAPI.Core.Perceptions.PushPerception(idleToMoving, movingToMoving);

        // La transición que pasan al estado "idle" se lanzan cuando la acción de "moving" o "flee" termine.
        fsm.CreateFinishStateTransition("moving to idle", moving, idle, true, true);
        fsm.CreateFinishStateTransition("runaway to idle", flee, idle, true, true);

        // Las transiciones que pasan al estado "flee" se activan con la percepción "chicken near"
        fsm.CreateTransition("idle to runaway", idle, flee, chickenNear);
        fsm.CreateTransition("moving to runaway", moving, flee, chickenNear);

        return fsm;
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _click.Fire();
        }
        base.OnUpdate();
    }

    private bool CheckDistanceToChicken()
    {
        return Vector3.Distance(transform.position, chicken.transform.position) < minDistanceToChicken;
    }

}