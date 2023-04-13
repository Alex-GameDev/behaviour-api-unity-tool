using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.UnityExtensions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.UtilitySystems;
using BehaviourAPI.StateMachines;

namespace graphs
{
	public class NewBehaviourRunner : CodeBehaviourRunner
	{
		[SerializeField] private AnimationCurve variablecurve;
		
		protected override BehaviourGraph CreateGraph()
		{
			BehaviourTree main = new BehaviourTree();
			UtilitySystem RecipeUS = new UtilitySystem(1.3f);
			FSM MakepizzaFSM = new FSM();
			
			Action recipeaction = new SubsystemAction(RecipeUS, false, false);
			LeafNode recipe = main.CreateLeafNode(recipeaction);
			Action createaction = new SubsystemAction(MakepizzaFSM, false, false);
			LeafNode create = main.CreateLeafNode(createaction);
			Action bakeaction = new FunctionalAction(BakePizza, pizzaBaked, BakedActionCompleted);
			LeafNode bake = main.CreateLeafNode(bakeaction);
			SequencerNode seq = main.CreateComposite<SequencerNode>(false, recipe, create, bake);
			IteratorNode iter = main.CreateDecorator<IteratorNode>(seq);
			iter.Iterations = -1;
			
			VariableFactor Pizzafactor = RecipeUS.CreateVariable(PizzaFactor, 0f, 10f);
			VariableFactor Peperonifactor = RecipeUS.CreateVariable(PeperoniFactor, 0f, 4f);
			WeightedFusionFactor variable_1 = RecipeUS.CreateFusion<WeightedFusionFactor>(Pizzafactor, Peperonifactor);
			variable_1.Weights = new float[] { 0.6f, 0.4f};
			Action hamandcheeseaction = new FunctionalAction(CreateRecipe_0, RecipeCreated, CreateRecipeCompleted);
			UtilityAction hamandcheese = RecipeUS.CreateAction(variable_1, hamandcheeseaction, true);
			ExponentialCurveFactor ExponentialCurveFactor = RecipeUS.CreateCurve<ExponentialCurveFactor>(Pizzafactor);
			ExponentialCurveFactor.Exponent = 0.7f;
			ExponentialCurveFactor.DespX = 0f;
			ExponentialCurveFactor.DespY = 0f;
			Action hawaiianaction = new FunctionalAction(CreateRecipe_1, RecipeCreated, CreateRecipeCompleted);
			UtilityAction hawaiian = RecipeUS.CreateAction(ExponentialCurveFactor, hawaiianaction, true);
			UnityCurveFactor variable = RecipeUS.CreateCurve<UnityCurveFactor>(Pizzafactor);
			variable.curve = variablecurve;
			Action vegetarianaction = new FunctionalAction(CreateRecipe_2, RecipeCreated, CreateRecipeCompleted);
			UtilityAction vegetarian = RecipeUS.CreateAction(variable, vegetarianaction, true);
			PointedCurveFactor variable_2 = RecipeUS.CreateCurve<PointedCurveFactor>(null);
			variable_2.Points = new List<CurvePoint>() {new CurvePoint(0f, 0.8f), new CurvePoint(0.5f, 0.2f), new CurvePoint(1f, 0.5f)};
			
			Action Stateaction = new FunctionalAction(PutMass, WaitToPutIngredient);
			State State = MakepizzaFSM.CreateState(Stateaction);
			Action State1action = new FunctionalAction(PutTomato, WaitToPutIngredient);
			State State_1 = MakepizzaFSM.CreateState(State1action);
			Action State2action = new FunctionalAction(PutNextTopping, CheckToppings);
			State State_2 = MakepizzaFSM.CreateState(State2action);
			ExitTransition ExitTransition = MakepizzaFSM.CreateExitTransition(State_2, Status.Success, statusFlags: StatusFlags.Success);
			StateTransition StateTransition = MakepizzaFSM.CreateTransition(State, State_1, statusFlags: StatusFlags.Finished);
			StateTransition StateTransition_1 = MakepizzaFSM.CreateTransition(State_1, State_2, statusFlags: StatusFlags.Finished);
			StateTransition StateTransition_2 = MakepizzaFSM.CreateTransition(State_2, State_2, statusFlags: StatusFlags.Failure);
			
			return main;
		}
		
		private void BakePizza()
		{
			throw new System.NotImplementedException();
		}
		
		private Status pizzaBaked()
		{
			throw new System.NotImplementedException();
		}
		
		private void BakedActionCompleted()
		{
			throw new System.NotImplementedException();
		}
		
		private float PizzaFactor()
		{
			throw new System.NotImplementedException();
		}
		
		private float PeperoniFactor()
		{
			throw new System.NotImplementedException();
		}
		
		private void CreateRecipe_0()
		{
			throw new System.NotImplementedException();
		}
		
		private Status RecipeCreated()
		{
			throw new System.NotImplementedException();
		}
		
		private void CreateRecipeCompleted()
		{
			throw new System.NotImplementedException();
		}
		
		private void CreateRecipe_1()
		{
			throw new System.NotImplementedException();
		}
		
		private void CreateRecipe_2()
		{
			throw new System.NotImplementedException();
		}
		
		private void PutMass()
		{
			throw new System.NotImplementedException();
		}
		
		private Status WaitToPutIngredient()
		{
			throw new System.NotImplementedException();
		}
		
		private void PutTomato()
		{
			throw new System.NotImplementedException();
		}
		
		private void PutNextTopping()
		{
			throw new System.NotImplementedException();
		}
		
		private Status CheckToppings()
		{
			throw new System.NotImplementedException();
		}
	}
}
