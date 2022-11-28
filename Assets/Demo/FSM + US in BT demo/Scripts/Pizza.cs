using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza : MonoBehaviour
{
    float _height;

    public void AddIngredient(Ingredient ingredient)
    {
        var ingredientItem = Instantiate(ingredient.prefab, transform);
        ingredientItem.transform.Translate(Vector3.up * _height);
        _height += ingredient.height;
    }

    public void SetHandler(Transform transform)
    {
        transform.parent = transform;
        transform.localPosition = Vector3.zero;
    }

    public void Clear()
    {
        int childs = transform.childCount;
        for (int i = childs - 1; i > 0; i--)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
        _height = 0f;
    }
}
