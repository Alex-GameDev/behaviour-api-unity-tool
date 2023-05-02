using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;

public class RadarFSMEditorRunner : EditorBehaviourRunner, IRadar
{
    RadarDisplay _radarDisplay;

    protected override void OnAwake()
    {
        _radarDisplay = GetComponent<RadarDisplay>();
        base.OnAwake();
    }


    public State GetBrokenState()
    {
        return FindGraph("Main").FindNode<State>("broken");
    }

    public State GetWorkingState()
    {
        return FindGraph("Main").FindNode<State>("working");
    }

    public bool CheckRadarForOverSpeed()
    {
        return _radarDisplay.CheckRadar((speed) => speed > 20);
    }

    public bool CheckRadarForUnderSpeed()
    {
        return _radarDisplay.CheckRadar((speed) => speed <= 20);
    }
}
