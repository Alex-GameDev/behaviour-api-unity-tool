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
        public List<EditorHierarchyNode> Childs;

        public EditorHierarchyNode(string name, Type type, List<EditorHierarchyNode> childs)
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
