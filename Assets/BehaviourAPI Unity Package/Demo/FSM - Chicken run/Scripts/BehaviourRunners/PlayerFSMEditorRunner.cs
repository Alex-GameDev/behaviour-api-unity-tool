using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

public class PlayerFSMEditorRunner : EditorBehaviourRunner
{
    [SerializeField] private float minDistanceToChicken = 5;
    [SerializeField] private Transform chicken;

    private PushPerception _click;

    protected override void OnAwake()
    {
        base.OnAwake();
        _click = FindPushPerception("click");
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

    public bool CheckDistanceToChicken()
    {
        return Vector3.Distance(transform.position, chicken.transform.position) < minDistanceToChicken;
    }
}
