using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Demos
{
    public class BoyBTEditorRunner : EditorBehaviourRunner
    {
        [SerializeField] AudioClip doorOpenClip;
        [SerializeField] AudioClip keyFoundClip;
        [SerializeField] AudioClip explosionClip;
        [SerializeField] GameObject explosionFX;
        Door _door;
        AudioSource _audioSource;

        private bool _hasKey, _keyFound;
        NavMeshAgent _meshAgent;

        protected override void OnAwake()
        {
            _door = FindObjectOfType<Door>();
            _audioSource = GetComponent<AudioSource>();
            _meshAgent = GetComponent<NavMeshAgent>();
            base.OnAwake();
        }

        protected override void ModifyGraphs()
        {
            var doorPos = new Vector3(_door.transform.position.x, transform.position.y, _door.transform.position.z);
            BuildedGraph.FindNode<LeafNode>("go to door").Action = new WalkAction(doorPos, 5f);
            BuildedGraph.FindNode<LeafNode>("return to door").Action = new WalkAction(doorPos, 5f);
        }

        public void SmashDoor()
        {
            GameObject explosion = Instantiate(explosionFX, _door.transform);
            _audioSource.clip = explosionClip;
            _audioSource.Play();
            Destroy(explosion, 3);
        }

        public Status EndWithSuccess() => Status.Success;

        public Status DoorStatus()
        {
            return !_door.IsClosed ? Status.Success : Status.Failure;
        }

        public void OpenDoor()
        {
            if (!_door.IsClosed)
            {
                _audioSource.clip = doorOpenClip;
                _audioSource.Play();
            }
        }

        public void EnterTheHouse()
        {
            Destroy(gameObject, 2);
        }

        public void FindKey()
        {
            GameObject key = GameObject.FindGameObjectWithTag("Key");

            if (key != null)
            {
                _keyFound = true;
                _meshAgent.destination = new Vector3(key.transform.position.x, transform.position.y, key.transform.position.z);
            }
        }

        public Status IsKeyObtained()
        {
            if (_hasKey)
            {
                return Status.Success;
            }
            else if (!_keyFound)
            {
                return Status.Failure;
            }
            else
            {
                return Status.Running;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Key")
            {
                _audioSource.clip = keyFoundClip;
                _audioSource.Play();
                _hasKey = true;
                _door.IsClosed = false;

                other.gameObject.SetActive(false);
            }
        }
    }

}