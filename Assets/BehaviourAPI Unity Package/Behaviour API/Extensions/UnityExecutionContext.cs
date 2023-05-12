using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.UnityExtensions
{
    using Core;
    /// <summary>
    /// The execution context in unity with references to the gameObject and the main components.
    /// </summary>
    public class UnityExecutionContext : ExecutionContext
    {
        /// <summary>
        /// The script that executes the graph with this context.
        /// </summary>
        public Component RunnerComponent { get; private set; }

        /// <summary>
        /// The gameobject of the agent.
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// The transform of the agent.
        /// </summary>
        public Transform Transform { get; private set; }

        /// <summary>
        /// The NavMewshAgent component.
        /// </summary>
        public NavMeshAgent NavMeshAgent { get; private set; }

        /// <summary>
        /// The Rigidbody component.
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }

        /// <summary>
        /// The rigidbody2D component.
        /// </summary>
        public Rigidbody2D Rigidbody2D { get; private set; }

        /// <summary>
        /// The collider component.
        /// </summary>
        public Collider Collider { get; private set; }

        /// <summary>
        /// The collider 2D component.
        /// </summary>
        public Collider2D Collider2D { get; private set; }

        /// <summary>
        /// The characterController component.
        /// </summary>
        public CharacterController CharacterController { get; private set; }


        public IAgentMovement Movement { get; private set; }

        /// <summary>
        /// Create a new uniy execution context with a runner script component. Use this constructor
        /// to access methods in the runner component with custom actions or perceptions.
        /// </summary>
        /// <param name="runnerComponent">The runner component.</param>
        public UnityExecutionContext(Component runnerComponent) : this(runnerComponent.gameObject)
        {
            RunnerComponent = runnerComponent;
        }

        /// <summary>
        /// Create a new uniy execution context with a gameobject.
        /// </summary>
        /// <param name="gameObject"></param>
        public UnityExecutionContext(GameObject gameObject)
        {
            GameObject = gameObject;
            if (gameObject != null)
            {
                Transform = gameObject.transform;
                NavMeshAgent = gameObject.GetComponent<NavMeshAgent>();
                Rigidbody = gameObject.GetComponent<Rigidbody>();
                Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                Collider = gameObject.GetComponent<Collider>();
                Collider2D = gameObject.GetComponent<Collider2D>();
                CharacterController = gameObject.GetComponent<CharacterController>();
                Movement = gameObject.GetComponent<IAgentMovement>();
            }
            else
            {
                Debug.LogWarning("Context was created with a null gameobject");
            }
        }
    }
}