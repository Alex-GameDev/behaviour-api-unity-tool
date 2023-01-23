using BehaviourAPI.Unity.Runtime;

using System.Collections.Generic;
using System.Linq;

using System.Text.RegularExpressions;
using UnityEngine.UIElements;


namespace BehaviourAPI.Unity.Editor
{
    public static class Extensions
    {
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

        public static void Disable(this VisualElement visualElement) => visualElement.style.display = DisplayStyle.None;
        public static void Enable(this VisualElement visualElement) => visualElement.style.display = DisplayStyle.Flex;
        public static void Hide(this VisualElement visualElement) => visualElement.style.visibility = Visibility.Hidden;
        public static void Show(this VisualElement visualElement) => visualElement.style.visibility = Visibility.Visible;

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
    }
}
