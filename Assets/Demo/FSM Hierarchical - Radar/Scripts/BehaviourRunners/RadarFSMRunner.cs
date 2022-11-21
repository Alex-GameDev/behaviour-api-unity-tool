using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;
using UnityEngine.UI;

public class RadarFSMRunner : BehaviourGraphRunner
{
    [SerializeField] private Vector3 pointToLook;
    [SerializeField] private Text speedText;
    [SerializeField] Light radarLight;

    BehaviourAPI.StateMachines.State _brokenState, _workingState;

    protected override BehaviourGraph CreateGraph()
    {
        var radarFSM = new BehaviourAPI.StateMachines.FSM();

        var fix = new UnityTimePerception(10f);
        var @break = new UnityTimePerception(20f);

        var subFSM = CreateLightSubFSM();
        var workingState = radarFSM.CreateState("working", new EnterSystemAction(subFSM));
        var brokenState = radarFSM.CreateState("broken", new BlinkAction(radarLight, speedText, Color.yellow));

        // La FSM cambia de un estado a otro con el paso del tiempo:
        var @fixed = radarFSM.CreateTransition("fixed", brokenState, workingState, fix);
        var broken = radarFSM.CreateTransition("broken", workingState, brokenState, @break);

        _brokenState = brokenState;
        _workingState = workingState;

        return radarFSM;
    }

    private BehaviourAPI.StateMachines.FSM CreateLightSubFSM()
    {
        var lightSubFSM = new BehaviourAPI.StateMachines.FSM();
        var overSpeedPerception = new RadarPerception(pointToLook, transform, (speed) => speed > 20, speedText);
        var underSpeedPerception = new RadarPerception(pointToLook, transform, (speed) => speed <= 20, speedText);

        var waitingState = lightSubFSM.CreateState("waiting", new LightAction(radarLight, Color.blue));
        var overSpeedState = lightSubFSM.CreateState("over", new LightAction(radarLight, Color.red, 1f));
        var underSpeedState = lightSubFSM.CreateState("under", new LightAction(radarLight, Color.green, 1f));

        // Pasa al estado "over" o "under" si pasa un coche, según la velocidad que lleve
        lightSubFSM.CreateTransition("car over speed", waitingState, overSpeedState, overSpeedPerception);
        lightSubFSM.CreateTransition("car under speed", waitingState, underSpeedState, underSpeedPerception);

        // Vuelve al estado de espera al acabar la acción
        lightSubFSM.CreateFinishStateTransition("over speed to waiting", overSpeedState, waitingState, true, true);
        lightSubFSM.CreateFinishStateTransition("under speed to waiting", underSpeedState, waitingState, true, true);

        return lightSubFSM;
    }

    public BehaviourAPI.StateMachines.State GetBrokenState() => _brokenState;

    public BehaviourAPI.StateMachines.State GetWorkingState() => _workingState;
}
