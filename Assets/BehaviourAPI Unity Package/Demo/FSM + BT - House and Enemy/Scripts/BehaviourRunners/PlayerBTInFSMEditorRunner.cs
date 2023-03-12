using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using BehaviourAPI.UnityExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBTInFSMEditorRunner : EditorBehaviourRunner
{
    [SerializeField] AudioClip keyFoundClip;

    Transform _enemyTransform;
    Transform _door;
    AudioSource _audioSource;

    Vector3 _doorPos;
    Vector3 _keyPos;

    bool _hasKey;

    protected override void OnAwake()
    {
        _enemyTransform = GameObject.FindGameObjectWithTag("Enemy").transform;
        _door = GameObject.FindGameObjectWithTag("Door").transform;
        _doorPos = new Vector3(_door.position.x, transform.position.y, _door.position.z);
        _keyPos = GameObject.FindGameObjectWithTag("Key").transform.position;
        _audioSource = GetComponent<AudioSource>();
        base.OnAwake();
    }

    protected override void ModifyGraphs()
    {
        var mainGraph = FindGraph("main");
        var subgraph = FindGraph("key subtree");

        mainGraph.FindNode<State>("go to home").SetAction(new WalkAction(_doorPos, 5f));
        mainGraph.FindNode<State>("enter house").SetAction(new FunctionalAction(EnterTheHouse));

        //FindPerception<DistancePerception>("distance to enemy").OtherTransform = _enemyTransform;

        subgraph.FindNode<ConditionNode>("has no key").SetPerception(new ConditionPerception(() => !_hasKey));
        subgraph.FindNode<LeafNode>("walk to key").SetAction(new WalkAction(_keyPos, 5f));
        subgraph.FindNode<LeafNode>("return to door").SetAction(new WalkAction(_doorPos, 5f));
    }

    private void EnterTheHouse()
    {
        Destroy(gameObject, 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Key")
        {
            _audioSource.clip = keyFoundClip;
            _audioSource.Play();
            _hasKey = true;
            other.gameObject.SetActive(false);
        }
    }

}
