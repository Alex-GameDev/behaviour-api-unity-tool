using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Demos
{
    public class PlayerFSMRunner : CodeBehaviourRunner
    {
        #region variables

        [SerializeField] private float minDistanceToChicken = 5;
        [SerializeField] private Transform chicken;
        [SerializeField] private Transform restartPoint;

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
            var moving = fsm.CreateState("Moving", new MoveToMousePosAction());
            var flee = fsm.CreateState("Flee", new FleeAction(chicken, 2.5f, 15f, 5f));

            // Las transiciones que pasan al estado "moving" se activan con percepciones Push.
            var idleToMoving = fsm.CreateTransition("idle to moving", idle, moving, statusFlags: StatusFlags.None);
            var movingToMoving = fsm.CreateTransition("moving to moving", moving, moving, statusFlags: StatusFlags.None);
            _click = new PushPerception(movingToMoving, idleToMoving);

            // La transición que pasan al estado "idle" se lanzan cuando la acción del estado anterior termine.
            fsm.CreateTransition("moving to idle", moving, idle, statusFlags: StatusFlags.Finished);
            fsm.CreateTransition("runaway to idle", flee, idle, statusFlags: StatusFlags.Finished);

            // Las transiciones que pasan al estado "flee" se activan con la percepción "chicken near"
            fsm.CreateTransition("idle to runaway", idle, flee, chickenNear);
            fsm.CreateTransition("moving to runaway", moving, flee, chickenNear);

            RegisterGraph(fsm);
            return fsm;
        }

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

        public void Restart()
        {
            transform.position = restartPoint.position;
        }
    }
}