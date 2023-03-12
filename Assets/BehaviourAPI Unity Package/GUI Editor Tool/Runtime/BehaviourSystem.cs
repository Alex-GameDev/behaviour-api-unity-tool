using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    using Framework;
    /// <summary>
    /// Stores a behaviour system data in a asset file
    /// </summary>
    [CreateAssetMenu(fileName = "newBehaviourSystem", menuName = "BehaviourAPI/BehaviourSystem", order = 0)]
    public class BehaviourSystem : ScriptableObject, IBehaviourSystem
    {
        [SerializeField] SystemData data = new SystemData();

        public SystemData Data => data;
        public Object ObjectReference => this;
    }
}