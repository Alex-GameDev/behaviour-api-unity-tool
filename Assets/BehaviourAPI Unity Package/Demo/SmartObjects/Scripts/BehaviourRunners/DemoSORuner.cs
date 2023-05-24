using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UnityExtensions;
using UnityEngine.AI;

public class DemoSORuner : BehaviourRunner
{
    private SmartAgent _agent;

    BSRuntimeDebugger _debugger;

    protected override void Init()
    {
        _agent = GetComponent<SmartAgent>();
        _debugger = GetComponent<BSRuntimeDebugger>();
        base.Init();
    }

    protected override BehaviourGraph CreateGraph()
    {
        var bt = new BehaviourTree();

        var randomRequestAction = new RandomRequestAction(_agent);
        var leaf = bt.CreateLeafNode(randomRequestAction);
        var root = bt.CreateDecorator<LoopNode>(leaf);
        bt.SetRootNode(root);

        _debugger.RegisterGraph(bt, "main");
        return bt;
    }
}
