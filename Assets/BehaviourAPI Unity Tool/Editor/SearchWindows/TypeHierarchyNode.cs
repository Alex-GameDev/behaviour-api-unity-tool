using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public struct HierarchicalTypeNode
    {
        public Type Type;
        public List<HierarchicalTypeNode> Childs;

        public HierarchicalTypeNode(Type rootType, IEnumerable<Type> derivedTypes)
        {
            Type = rootType;
            Childs = derivedTypes.Where(t => t.BaseType == rootType).ToList()
                .Select(subType => new HierarchicalTypeNode(subType,
                derivedTypes.Where(t => t.IsSubclassOf(subType) && t != subType))).ToList();
        }
    }
}
