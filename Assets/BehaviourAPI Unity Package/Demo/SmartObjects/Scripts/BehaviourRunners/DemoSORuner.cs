using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;

public class DemoSORuner : CodeBehaviourRunner
{
    private SmartAgent _agent;


    protected override BehaviourGraph CreateGraph()
    {
        var bt = new BehaviourTree();

        var randomRequestAction = new RandomRequestAction(_agent);
        var leaf = bt.CreateLeafNode(randomRequestAction);
        var root = bt.CreateDecorator<LoopNode>(leaf);
        bt.SetRootNode(root);

        RegisterGraph(bt, "main");
        return bt;
    }

    protected override void OnAwake()
    {
        _agent = GetComponent<SmartAgent>();
        base.OnAwake();
    }
}
