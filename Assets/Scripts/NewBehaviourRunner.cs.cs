using System;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityExtensions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Demos;

public class NewBehaviourRunner : CodeBehaviourRunner
{
	
	
	protected override BehaviourGraph CreateGraph()
	{
		FSM main = new FSM();
		
		ChangeSpeedAction State_1_action = new ChangeSpeedAction();
		State_1_action.baseSpeed = 0f;
		State_1_action.minAddedSpeed = 10f;
		State_1_action.maxAddedSpeed = 30f;
		State State_1 = main.CreateState(State_1_action);
		
		ChangeSpeedAction State_2_action = new ChangeSpeedAction();
		State_2_action.baseSpeed = 10f;
		State_2_action.minAddedSpeed = 10f;
		State_2_action.maxAddedSpeed = 30f;
		State State_2 = main.CreateState(State_2_action);
		
		StateTransition speed_down = main.CreateTransition(State_2, State_1);
		
		StateTransition speed_up = main.CreateTransition(State_1, State_2);
		
		return main;
	}
}
