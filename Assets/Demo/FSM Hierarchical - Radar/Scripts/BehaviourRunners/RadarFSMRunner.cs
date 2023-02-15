using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using UnityEngine;
using UnityEngine.UI;

public class RadarFSMRunner : CodeBehaviourRunner, IRadar
{
    [SerializeField] private Vector3 pointToLook;
    [SerializeField] private Text speedText;
    [SerializeField] Light radarLight;

    State _brokenState, _workingState;

    protected override BehaviourGraph CreateGraph()
    {
        var radarFSM = new FSM();

        var fix = new UnityTimePerception(10f);
        var @break = new UnityTimePerception(20f);

        var subFSM = CreateLightSubFSM();
        var workingState = radarFSM.CreateState("working state", new SubsystemAction(subFSM));
        var brokenState = radarFSM.CreateState("broken state", new BlinkAction(radarLight, speedText, Color.yellow));

        // La FSM cambia de un estado a otro con el paso del tiempo:
        var @fixed = radarFSM.CreateTransition("fixed", brokenState, workingState, fix);
        var broken = radarFSM.CreateTransition("broken", workingState, brokenState, @break);

        _brokenState = brokenState;
        _workingState = workingState;

        RegisterGraph(radarFSM);
        RegisterGraph(subFSM);

        return radarFSM;
    }

    private FSM CreateLightSubFSM()
    {
        var lightSubFSM = new FSM();
        var overSpeedPerception = new RadarPerception(pointToLook, transform, (speed) => speed > 20, speedText);
        var underSpeedPerception = new RadarPerception(pointToLook, transform, (speed) => speed <= 20, speedText);

        var waitingState = lightSubFSM.CreateState("waiting", new LightAction(radarLight, Color.blue));
        var overSpeedState = lightSubFSM.CreateState("over", new LightAction(radarLight, Color.red, 1f));
        var underSpeedState = lightSubFSM.CreateState("under", new LightAction(radarLight, Color.green, 1f));

        // Pasa al estado "over" o "under" si pasa un coche, según la velocidad que lleve
        lightSubFSM.CreateTransition("car over speed", waitingState, overSpeedState, overSpeedPerception);
        lightSubFSM.CreateTransition("car under speed", waitingState, underSpeedState, underSpeedPerception);

        // Vuelve al estado de espera al acabar la acción
        lightSubFSM.CreateTransition("over speed to waiting", overSpeedState, waitingState, statusFlags: StatusFlags.Finished);
        lightSubFSM.CreateTransition("under speed to waiting", underSpeedState, waitingState, statusFlags: StatusFlags.Finished);

        return lightSubFSM;
    }

    public State GetBrokenState() => _brokenState;

    public State GetWorkingState() => _workingState;

}
