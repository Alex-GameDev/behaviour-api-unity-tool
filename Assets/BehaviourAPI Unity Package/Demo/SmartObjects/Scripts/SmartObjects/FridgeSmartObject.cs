using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.Demos;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.SmartObjects
{
    public class FridgeSmartObject : DirectSmartObject
    {
        [SerializeField] float useTime = 3f;
        [SerializeField] Transform fridgeDoor;
        float lieTime;

        protected override Action GetUseAction(SmartAgent agent)
        {
            return new FunctionalAction(StartUse, () => OnUpdate(agent), StopUse);
        }

        void StartUse()
        {
            Debug.Log("_______________________________");
            lieTime = Time.time;
            fridgeDoor.rotation = Quaternion.Euler(0, -90, 0);
        }

        void StopUse()
        {
            fridgeDoor.rotation = Quaternion.identity;
        }

        Status OnUpdate(SmartAgent smartAgent)
        {
            if (Time.time > lieTime + useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}

