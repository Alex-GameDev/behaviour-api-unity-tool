using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Framework;

public class BoyBTVisualRunner : VisualBehaviourRunner
{
    Door _door;
    [SerializeField] AudioClip doorOpenClip;
    [SerializeField] AudioClip keyFoundClip;
    [SerializeField] AudioClip explosionClip;

    [Header("VFX")]
    [SerializeField] GameObject explosionFX;

    private bool _hasKey, _keyFound;
    AudioSource _audioSource;
    NavMeshAgent meshAgent;

    protected override void OnAwake()
    {
        _door = FindObjectOfType<Door>();
        _audioSource = GetComponent<AudioSource>();
        meshAgent = GetComponent<NavMeshAgent>();
        base.OnAwake();
    }

    [CustomMethod]
    public void SmashDoor()
    {
        Debug.Log("Exploding");
        GameObject explosion = Instantiate(explosionFX, _door.transform);
        _audioSource.clip = explosionClip;
        _audioSource.Play();
        Destroy(explosion, 3);
    }

    [CustomMethod]
    public void OpenDoor()
    {
        if (!_door.IsClosed)
        {
            Debug.Log("Door is open");
            _audioSource.clip = doorOpenClip;
            _audioSource.Play();
        }
        else
            Debug.Log("Door is closed");
    }

    [CustomMethod]
    public void EnterTheHouse()
    {
        Debug.Log("Entering the house");
        Destroy(gameObject, 2);
    }

    [CustomMethod]
    public void FindKey()
    {
        GameObject key = GameObject.FindGameObjectWithTag("Key");

        if (key != null)
        {
            _keyFound = true;
            Debug.Log("Key found");
            meshAgent.destination = new Vector3(key.transform.position.x, transform.position.y, key.transform.position.z);
        }

    }

    [CustomMethod]
    public Status IsKeyObtained()
    {
        if (_hasKey)
        {
            Debug.Log("Key obtained");
            return Status.Success;
        }
        else if (!_keyFound)
        {
            Debug.Log("Key didn't found");
            return Status.Failure;
        }
        else
        {
            Debug.Log("Moving to key");
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
