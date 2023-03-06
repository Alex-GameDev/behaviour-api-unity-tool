using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{   
    public static class VisualElementExtensions
    {
        public static bool IsHorizontal(this PortOrientation portOrientation) => portOrientation == PortOrientation.Left || portOrientation == PortOrientation.Right;

        public static Orientation ToOrientation(this PortOrientation portOrientation)
        {
            if (portOrientation.IsHorizontal()) return Orientation.Horizontal;
            else return Orientation.Vertical;
        }

        public static Vector2 ToVector(this PortOrientation portOrientation)
        {
            if (portOrientation == PortOrientation.Top) return Vector2.up;
            if (portOrientation == PortOrientation.Bottom) return Vector2.down;
            if (portOrientation == PortOrientation.Left) return Vector2.left;
            if (portOrientation == PortOrientation.Right) return Vector2.right;
            else return Vector2.zero;
        }

        public static FlexDirection ToFlexDirection(this PortOrientation portOrientation)
        {
            if (portOrientation == PortOrientation.Top) return FlexDirection.ColumnReverse;
            if (portOrientation == PortOrientation.Bottom) return FlexDirection.Column;
            if (portOrientation == PortOrientation.Left) return FlexDirection.Row;
            if (portOrientation == PortOrientation.Right) return FlexDirection.RowReverse;
            else return FlexDirection.Column;
        }
    }
}
