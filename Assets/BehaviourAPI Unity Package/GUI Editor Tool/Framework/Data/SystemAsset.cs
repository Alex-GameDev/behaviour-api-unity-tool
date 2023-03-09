using UnityEngine;

namespace BehaviourAPI.UnityTool.Framework
{
    [CreateAssetMenu(menuName = "SystemAsset")]
    public class SystemAsset : ScriptableObject
    {
        [HideInInspector][SerializeField] public BehaviourSystemData data;
    }
}

