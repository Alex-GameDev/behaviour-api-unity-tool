using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipePaper : MonoBehaviour
{
    [SerializeField] Text _recipeNameText;
    [SerializeField] Text _ingredientText;

    Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetRecipe(Recipe recipe)
    {
        _recipeNameText.text = recipe.RecipeName;
        _ingredientText.text = "";
        recipe.ingredients.ForEach(ingredient => _ingredientText.text += $"{ingredient.Name}\n");
    }

    public void Show() => _animator.SetTrigger("Reset");
    public void Hide() => _animator.ResetTrigger("Reset");

    public void Clear()
    {
        _recipeNameText.text = "";
        _ingredientText.text = "";
    }
}
