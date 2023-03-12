using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PizzaBoyEditorRunner : EditorBehaviourRunner
{
    [SerializeField] Ingredient _pizzaMass;
    [SerializeField] Ingredient _tomato;
    [SerializeField] List<Recipe> _allRecipes;
    [SerializeField] float _timeToAddIngredient;
    [SerializeField] Transform _pizzaTransform, _table, _oven, _handsHandler, _tableHandler;

    [SerializeField] Pizza _pizza;
    [SerializeField] RecipePaper _recipePaper;

    NavMeshAgent _agent;

    // utility variables
    int _pizzasCreated = 0;
    int _peperoniUsed = 0;

    Recipe _currentRecipe;
    int _currentIngredient = 0;
    float _lastIngredientAddedTime = 0f;

    protected override void OnAwake()
    {
        _agent = GetComponent<NavMeshAgent>();
        base.OnAwake();
    }

    // Escribe la receta
    public void CreateRecipe_0() => CreateRecipe(0);

    public void CreateRecipe_1() => CreateRecipe(1);

    public void CreateRecipe_2() => CreateRecipe(2);
    public void CreateRecipe(int id)
    {
        _pizza.SetHandler(_tableHandler);
        _agent.SetDestination(_table.position);
        _pizzasCreated += 1;
        if (id == 0) _peperoniUsed += 1;
        _currentRecipe = _allRecipes[id];
        _currentIngredient = 0;
        _recipePaper.SetRecipe(_currentRecipe);
        _recipePaper.Show();
    }

    // Espera a que el personaje se haya colocado en la mesa
    public Status RecipeCreated() => (Vector3.Distance(transform.position, _table.position) < 0.3f).ToStatus(Status.Running);

    // Cuando termina la acci�n:
    public void CreateRecipeCompleted()
    {
        transform.LookAt(_pizzaTransform);
        _recipePaper.Hide();
    }

    // A�ade un elemento a la pizza
    public void PutMass() => PutIngredient(_pizzaMass);

    public void PutTomato() => PutIngredient(_tomato);

    public void PutIngredient(Ingredient ingredient)
    {
        _pizza.AddIngredient(ingredient);
        _lastIngredientAddedTime = Time.time;
    }

    // Espera un tiempo
    public Status WaitToPutIngredient()
    {
        Status st = (Time.time > _lastIngredientAddedTime + _timeToAddIngredient).ToStatus(Status.Running);
        return st;
    }

    // A�ade el siguiente ingrediente de la receta
    public void PutNextTopping()
    {
        _lastIngredientAddedTime = Time.time;
        PutIngredient(_currentRecipe.ingredients[_currentIngredient]);
        _currentIngredient++;
    }

    // Espera un tiempo y devuelve success si ya ha puesto todos los ingredientes.
    public Status CheckToppings()
    {
        if (Time.time > _lastIngredientAddedTime + _timeToAddIngredient)
        {
            return (_currentIngredient == _currentRecipe.ingredients.Count).ToStatus();
        }
        else
            return Status.Running;
    }

    // Acci�n de hornear la pizza
    public void BakePizza()
    {
        _pizza.SetHandler(_handsHandler);
        _agent.SetDestination(_oven.position);
    }

    // Espera a que el personaje llegue al horno
    public Status pizzaBaked() => (Vector3.Distance(transform.position, _oven.position) < 0.5f).ToStatus(Status.Running);

    // Cuando la acci�n de hornear la pizza acaba, se borra la receta y se destruye la pizza
    public void BakedActionCompleted()
    {
        _pizza.Clear();
        _recipePaper.Clear();
    }

    public float PizzaFactor() => _pizzasCreated % 10;

    public float PeperoniFactor() => _peperoniUsed % 4;

}
