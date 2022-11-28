using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BehaviourAPI/Demos/Pizza/Ingredient", fileName = "ingredient")]
public class Ingredient : ScriptableObject
{
    public string Name;
    public GameObject prefab;
    public float height = .01f;
}
