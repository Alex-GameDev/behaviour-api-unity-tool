using BehaviourAPI.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] string varName;

    [ContextMenu("Test var name")]
    public void TestVarName() => Debug.Log(varName.ToValidIdentificatorName());
}
