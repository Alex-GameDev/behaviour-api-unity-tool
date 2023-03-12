using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Stores a behaviour system data in a asset file
    /// </summary>
    [CreateAssetMenu(fileName = "newBehaviourSystem", menuName = "BehaviourAPI/BehaviourSystem", order = 0)]
    public class BehaviourSystem : ScriptableObject, IBehaviourSystem
    {
        [SerializeField] SystemData data;


        public SystemData Data => data;
        public Object ObjectReference => this;
    }
}