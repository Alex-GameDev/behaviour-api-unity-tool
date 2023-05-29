using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    /// <summary>
    /// Stores a behaviour system data in a asset file.
    /// </summary>
    [CreateAssetMenu(fileName = "newBehaviourSystem", menuName = "BehaviourAPI/BehaviourSystem", order = 0)]
    public class BehaviourSystem : ScriptableObject, IBehaviourSystem
    {

        [SerializeField] SystemData data;
        public SystemData Data => data;
        public Object ObjectReference => this;

        /// <summary>
        /// Get a runtime copy of the behaviour system stored in this asset
        /// </summary>
        /// <returns>A copy of <see cref="Data"/></returns>
        public SystemData GetBehaviourSystemData()
        {
            string json = JsonUtility.ToJson(this);
            BehaviourSystem copy = CreateInstance<BehaviourSystem>();
            JsonUtility.FromJsonOverwrite(json, copy);
            return copy.Data;
        }
    }
}