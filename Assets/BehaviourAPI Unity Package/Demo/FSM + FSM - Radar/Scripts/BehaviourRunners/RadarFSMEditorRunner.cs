using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarFSMEditorRunner : EditorBehaviourRunner, IRadar
{
    [SerializeField] private Vector3 pointToLook;
    [SerializeField] private Text speedText;

    public State GetBrokenState()
    {
        return FindGraph("Main").FindNode<State>("broken");
    }

    public State GetWorkingState()
    {
        return FindGraph("Main").FindNode<State>("working");
    }

    [CustomMethod]
    public bool CheckRadarForOverSpeed()
    {
        return CheckRadar((speed) => speed > 20);
    }

    [CustomMethod]
    public bool CheckRadarForUnderSpeed()
    {
        return CheckRadar((speed) => speed <= 20);
    }

    bool CheckRadar(Func<float, bool> speecCheckFunction)
    {
        Ray ray = new Ray(transform.position, -transform.TransformPoint(pointToLook));

        if (Physics.Raycast(ray, out RaycastHit hit, 50) && hit.collider.tag == "Car")
        {
            var carSpeed = hit.collider.gameObject.GetComponent<ICar>().GetSpeed();

            bool trigger = speecCheckFunction?.Invoke(carSpeed) ?? false;
            if (trigger)
            {
                speedText.text = $"{Mathf.RoundToInt(carSpeed) + 100}";
            }
            return trigger;
        }
        return false;
    }
}
