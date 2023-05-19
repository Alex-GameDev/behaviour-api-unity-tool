using BehaviourAPI.Unity.SmartObjects;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    /// <summary> 
    /// Request action that find a random Smart Object using the smart object manager. 
    /// </summary>
    public class RandomRequestAction : UnityRequestAction
    {
        public RandomRequestAction()
        {
        }

        public RandomRequestAction(SmartAgent agent) : base(agent)
        {
        }

        protected override SmartObject GetSmartObject(SmartAgent agent)
        {
            int random = Random.Range(0, SmartObjectManager.Instance.RegisteredObjects.Count);
            return SmartObjectManager.Instance.RegisteredObjects[random];
        }
    }
}
