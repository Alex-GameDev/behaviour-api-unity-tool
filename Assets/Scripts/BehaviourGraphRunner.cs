using BehaviourAPI.Core;
using UnityEngine;

public abstract class BehaviourGraphRunner : MonoBehaviour
{
    BehaviourGraph _graph;

    void Awake() => OnAwake();

    void Start() => OnStart();

    void Update() => OnUpdate();

    protected abstract BehaviourGraph CreateGraph();
    protected virtual void OnAwake() => _graph = CreateGraph();
    protected virtual void OnStart() => _graph.Start();
    protected virtual void OnUpdate() => _graph.Update();
}
