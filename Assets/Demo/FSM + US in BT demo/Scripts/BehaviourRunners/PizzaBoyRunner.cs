using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Utils;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UtilitySystems;
using UnityEngine;
using UnityEngine.AI;

public class PizzaBoyRunner : BehaviourGraphRunner
{
    [SerializeField] Ingredient _pizzaMass;
    [SerializeField] Ingredient _tomato;
    [SerializeField] List<Recipe> _allRecipes;
    [SerializeField] float _timeToAddIngredient;
    [SerializeField] Transform _pizzaTransform, _table, _oven;

    [SerializeField] Pizza _pizza;
    [SerializeField] RecipePaper _recipePaper;

    NavMeshAgent _agent;

    // utility variables
    int _pizzasCreated = 0;
    int _peperoniUsed = 0;


    Recipe _currentRecipe;
    int _currentIngredient = 0;
    float _lastIngredientAddedTime = 0f;


    protected override BehaviourGraph CreateGraph()
    {
        var bt = new BehaviourAPI.BehaviourTrees.BehaviourTree();
        var us = CreateLookRecipeUtilitySystem();
        var fsm = CreateMakePizzaFSM();

        var recipeAction = bt.CreateLeafNode("recipe action", new EnterSystemAction(us));
        var makePizzaAction = bt.CreateLeafNode("make pizza action", new EnterSystemAction(fsm));
        var bakeAction = bt.CreateLeafNode("bake pizza" /* TODO: Accion*/);

        var seq = bt.CreateComposite<SequencerNode>("pizza seq", false, recipeAction, makePizzaAction, bakeAction);
        var root = bt.CreateDecorator<IteratorNode>("loop", seq).SetIterations(-1);
        bt.SetStartNode(root);

        return bt;
    }


    private BehaviourAPI.StateMachines.FSM CreateMakePizzaFSM()
    {
        var fsm = new BehaviourAPI.StateMachines.FSM();

        var massState = fsm.CreateState("mass", new FunctionalAction(() => PutIngredient(_pizzaMass), WaitToPutIngredient));
        var tomatoState = fsm.CreateState("tomato", new FunctionalAction(() => PutIngredient(_tomato), WaitToPutIngredient));
        var toppingState = fsm.CreateState("topping", new FunctionalAction(PutNextTopping, CheckToppings));
        var completeState = fsm.CreateState("completed", new ExitSystemAction(fsm, Status.Success));

        fsm.CreateFinishStateTransition("mass putted", massState, tomatoState, true, true);
        fsm.CreateFinishStateTransition("tommato putted", tomatoState, toppingState, true, true);
        fsm.CreateFinishStateTransition("next topping", toppingState, toppingState, false, true);
        fsm.CreateFinishStateTransition("pizza completed", toppingState, completeState, true, false);

        return fsm;
    }

    BehaviourAPI.UtilitySystems.UtilitySystem CreateLookRecipeUtilitySystem()
    {
        var us = new BehaviourAPI.UtilitySystems.UtilitySystem();

        var pizzafactor = us.CreateVariableFactor("pizzas", () => _pizzasCreated, 10, 0);
        var pepperoniFactor = us.CreateVariableFactor("peperoni", () => _pizzasCreated, 4, 0);

        var peperoniSumFactor = us.CreateFusionFactor<WeightedFusionFactor>("a", pizzafactor, pepperoniFactor)
            .SetWeights(0.6f, 0.4f);

        var pointList = new List<System.Numerics.Vector2>();
        pointList.Add(new System.Numerics.Vector2(0.0f, 1f));
        pointList.Add(new System.Numerics.Vector2(0.2f, 0.5f));
        pointList.Add(new System.Numerics.Vector2(0.4f, 0.1f));
        pointList.Add(new System.Numerics.Vector2(0.6f, 0.4f));
        pointList.Add(new System.Numerics.Vector2(0.8f, 0.2f));
        pointList.Add(new System.Numerics.Vector2(1.0f, 0.0f));
        var vegetarianFactor = us.CreateFunctionFactor<PointedFunction>("vegetarian", pizzafactor).SetPoints(pointList);

        var hawaiianFactor = us.CreateFunctionFactor<ExponentialFunction>("hawaiian", pizzafactor).SetExponent(.7f);

        var peperoniAction = us.CreateUtilityAction("choose ham and cheese", peperoniSumFactor,
            new FunctionalAction(() => CreateRecipe(0), RecipeCreated, CreateRecipeCompleted), finishOnComplete: true);
        var vegetarianAction = us.CreateUtilityAction("choose vegetarian", vegetarianFactor,
            new FunctionalAction(() => CreateRecipe(1), RecipeCreated, CreateRecipeCompleted), finishOnComplete: true);
        var hawaiianAction = us.CreateUtilityAction("choose hawaiian", hawaiianFactor,
            new FunctionalAction(() => CreateRecipe(2), RecipeCreated, CreateRecipeCompleted), finishOnComplete: true);

        return us;
    }

    // Escribe la receta
    void CreateRecipe(int id)
    {
        _pizzasCreated += 1;
        if (id == 0) _peperoniUsed += 1;
        _currentRecipe = _allRecipes[id];
        _currentIngredient = 0;
        _recipePaper.SetRecipe(_currentRecipe);
        _recipePaper.Show();
    }

    // Espera a que el personaje se haya colocado en la mesa
    Status RecipeCreated() => (Vector3.Distance(transform.position, _table.position) < 0.1f).ToStatus(Status.Running);

    // Cuando termina la acción:
    void CreateRecipeCompleted()
    {
        transform.LookAt(_pizzaTransform);
        _recipePaper.Hide();
    }



    // Añade un elemento a la pizza
    void PutIngredient(Ingredient ingredient) => _pizza.AddIngredient(ingredient);

    // Espera un tiempo
    Status WaitToPutIngredient()
    {
        return (Time.deltaTime > _lastIngredientAddedTime + _timeToAddIngredient).ToStatus(Status.Running);
    }

    // Añade el siguiente ingrediente de la receta
    void PutNextTopping()
    {
        PutIngredient(_currentRecipe.ingredients[_currentIngredient]);
        _currentIngredient++;
    }

    // Espera un tiempo y devuelve success si ya ha puesto todos los ingredientes.
    Status CheckToppings()
    {
        if (Time.deltaTime > _lastIngredientAddedTime + _timeToAddIngredient)
        {
            return (_currentIngredient == _currentRecipe.ingredients.Count).ToStatus();
        }
        else
            return Status.Running;
    }

    // Acción de hornear la pizza
    void BakePizza()
    {
        _pizza.SetHandler(transform);
        _agent.SetDestination(_oven.position);
    }

    // Espera a que el personaje llegue al horno
    Status pizzaBaked() => (Vector3.Distance(transform.position, _oven.position) < 0.1f).ToStatus(Status.Running);

    // Cuando la acción de hornear la pizza acaba, se borra la receta y se destruye la pizza
    void BakedActionCompleted()
    {
        _pizza.Clear();
        _recipePaper.Clear();
    }
}
