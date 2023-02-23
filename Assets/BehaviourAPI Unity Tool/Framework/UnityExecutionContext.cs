using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Framework
{
    public class UnityExecutionContext : ExecutionContext
    {
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        public NavMeshAgent NavMeshAgent { get; private set; }

        public Rigidbody Rigidbody { get; private set; }
        public Rigidbody2D Rigidbody2D { get; private set; }

        public Collider Collider { get; private set; }
        public Collider2D Collider2D { get; private set; }

        public CharacterController CharacterController { get; private set; }

        public UnityExecutionContext(GameObject gameObject)
        {
            GameObject = gameObject;
            if(gameObject != null)
            {
                Transform = gameObject.transform;
                NavMeshAgent = gameObject.GetComponent<NavMeshAgent>();
                Rigidbody = gameObject.GetComponent<Rigidbody>();
                Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                Collider = gameObject.GetComponent<Collider>();
                Collider2D = gameObject.GetComponent<Collider2D>();
                CharacterController = gameObject.GetComponent<CharacterController>();
            }
            else
            {
                Debug.LogWarning("Context was created with a null gameobject");
            }
        }
    }
}