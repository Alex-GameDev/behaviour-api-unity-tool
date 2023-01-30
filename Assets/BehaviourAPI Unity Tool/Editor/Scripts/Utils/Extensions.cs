using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


namespace BehaviourAPI.Unity.Editor
{
    public static class Extensions
    {
        #region -------------------------------- Strings and regex --------------------------------

        private static readonly Regex k_Whitespace = new Regex(@"\s+");

        private static readonly string[] k_Keywords = new[]
        {
            "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "double", "float", "decimal",
            "string", "char", "void", "object", "typeof", "sizeof", "null", "true", "false", "if", "else", "while", "for", "foreach", "do", "switch",
            "case", "default", "lock", "try", "throw", "catch", "finally", "goto", "break", "continue", "return", "public", "private", "internal",
            "protected", "static", "readonly", "sealed", "const", "fixed", "stackalloc", "volatile", "new", "override", "abstract", "virtual",
            "event", "extern", "ref", "out", "in", "is", "as", "params", "__arglist", "__makeref", "__reftype", "__refvalue", "this", "base",
            "namespace", "using", "class", "struct", "interface", "enum", "delegate", "checked", "unchecked", "unsafe", "operator", "implicit", "explicit"
        };

        private static readonly char[] k_NumberChars = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static string CamelCaseToSpaced(this string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1").Trim();
        }

        public static string RemoveWhitespaces(this string str)
        {
            return k_Whitespace.Replace(str, "");
        }

        public static string RemovePunctuationsAndSymbols(this string str)
        {
            return string.Concat(str.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)));
        }

        public static string Join(this IEnumerable<string> strings, string separator = ", ") => string.Join(separator, strings);

        public static string ToValidIdentificatorName(this string str)
        {

            str = str.RemoveWhitespaces();
            str = str.Replace('-', '_');
            str = str.RemovePunctuationsAndSymbols();

            if (string.IsNullOrEmpty(str)) return "variable";

            if (char.IsDigit(str[0])) str = "_" + str;

            if (k_Keywords.Contains(str)) str = "@" + str;

            return str;
        }

        public static string TypeName(this object obj) => obj.GetType().Name;
        public static string ToCodeFormat(this float f) => f.ToString().Replace(',', '.') + "f";
        public static string ToCodeFormat(this bool b) => b.ToString().ToLower();
        public static string ToCodeFormat(this Status s) => "Status." + s.ToString();
        public static string ToCodeFormat(this StatusFlags s) => "StatusFlags." + s.ToString();
        public static string GetPath(this NodeLayout layout)
        {
            if (layout == NodeLayout.Cyclic) return BehaviourAPISettings.instance.EditorElementPath + "/Nodes/CG Node.uxml";
            else if (layout == NodeLayout.Layered) return BehaviourAPISettings.instance.EditorElementPath + "/Nodes/DAG Node.uxml";
            else return BehaviourAPISettings.instance.EditorElementPath + "/Nodes/Tree Node.uxml";
        }


        #endregion

        #region -------------------------------- Visual elements --------------------------------

        public static void Disable(this VisualElement visualElement) => visualElement.style.display = DisplayStyle.None;
        public static void Enable(this VisualElement visualElement) => visualElement.style.display = DisplayStyle.Flex;
        public static void Hide(this VisualElement visualElement) => visualElement.style.visibility = Visibility.Hidden;
        public static void Show(this VisualElement visualElement) => visualElement.style.visibility = Visibility.Visible;

        public static Color ToColor(this Status status)
        {
            if (status == Status.Success) return Color.green;
            if (status == Status.Failure) return Color.red;
            if (status == Status.Running) return Color.yellow;
            return Color.gray;
        }

        public static void ChangeBorderColor(this VisualElement visualElement, Color color)
        {
            visualElement.style.borderBottomColor = color;
            visualElement.style.borderTopColor = color;
            visualElement.style.borderLeftColor = color;
            visualElement.style.borderRightColor = color;
        }

        public static void AddGroup(this List<SearchTreeEntry> searchTreeEntries, string title, int level)
        {
            searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(title), level));
        }

        public static void AddEntry(this List<SearchTreeEntry> searchTreeEntries, string title, int level, object userData)
        {
            searchTreeEntries.Add(new SearchTreeEntry(new GUIContent("     " + title)) { level = level, userData = userData});
        }

        public static DropdownMenuAction.Status ToMenuStatus(this bool b) => b ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;


        public static void Align(this Port port, PortOrientation orientation, Direction dir)
        {
            var middleValue = new StyleLength(new Length(50, LengthUnit.Percent));
            if (orientation == PortOrientation.None) return;

            port.style.position = Position.Absolute;
            if (orientation == PortOrientation.Top)
            {
                port.style.top = 0;
                if (dir == Direction.Input) port.style.right = middleValue;
                else port.style.left = middleValue;
            }
            else if (orientation == PortOrientation.Right)
            {
                port.style.right = 0;
                if (dir == Direction.Input) port.style.bottom = middleValue;
                else port.style.top = middleValue;
            }
            else if (orientation == PortOrientation.Bottom)
            {
                port.style.bottom = 0;
                if (dir == Direction.Input) port.style.left = middleValue;
                else port.style.right = middleValue;
            }
            else
            {
                port.style.left = 0;
                if (dir == Direction.Input) port.style.top = middleValue;
                else port.style.bottom = middleValue;
            }
        }
        #endregion
    }
}
