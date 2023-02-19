using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class EditorHierarchyNode
    {
        public string name;
        public Type Type;
        public IEnumerable<EditorHierarchyNode> Childs;

        public EditorHierarchyNode(string name, Type type, IEnumerable<EditorHierarchyNode> childs)
        {
            this.name = name;
            Type = type;
            Childs = childs;
        }

        public EditorHierarchyNode(string name, Type type)
        {
            this.name = name;
            Type = type;
            Childs = new List<EditorHierarchyNode>();
        }
    }
}
