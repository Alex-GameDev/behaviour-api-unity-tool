using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.UnityExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;
using UnityAction = BehaviourAPI.UnityExtensions.UnityAction;

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



        public static string RemoveTermination(this string str, string termination)
        {
            if (str.EndsWith(termination))
            {
                str = str.Substring(0, str.Length - termination.Length);
            }
            return str;
        }

        public static string TypeName(this object obj) => obj.GetType().Name;
        public static string ToCodeFormat(this float f) => f.ToString().Replace(',', '.') + "f";
        public static string ToCodeFormat(this bool b) => b.ToString().ToLower();
        public static string ToCodeFormat(this Status s) => "Status." + s.ToString();
        public static string ToCodeFormat(this StatusFlags s) => "StatusFlags." + ((int)s < 0 ? StatusFlags.Active.ToString() : s.ToString());

        public static string DisplayInfo(this StatusFlags statusFlags)
        {
            switch (statusFlags)
            {
                case StatusFlags.None: return "never";
                case StatusFlags.Success: return "when finish with success";
                case StatusFlags.Failure: return "when finish with failure";
                case StatusFlags.Finished: return "when finish";
                case StatusFlags.NotSuccess: return "when not finished with success";
                case StatusFlags.NotFailure: return "when not finished with failure";
                default: return "always";
            }
        }

        public static string GetActionInfo(this Action action)
        {
            switch (action)
            {
                case CustomAction customAction:

                    var actionMethodLines = new List<string>();

                    var code = customAction.start.GetSerializedMethodText();
                    if (code != null) actionMethodLines.Add(code);

                    code = customAction.update.GetSerializedMethodText();
                    if (code != null) actionMethodLines.Add(code);
                    else actionMethodLines.Add("() => Running;");

                    code = customAction.stop.GetSerializedMethodText();
                    if (code != null) actionMethodLines.Add(code);

                    return actionMethodLines.Join("\n");

                case UnityAction unityAction:
                    string info = unityAction.DisplayInfo;

                    var type = unityAction.GetType();
                    var properties = type.GetFields();

                    for (int i = 0; i < properties.Length; i++)
                    {
                        string value = GetPropertyDisplay(properties[i].GetValue(unityAction));
                        info = info.Replace($"${properties[i].Name}", value);
                    }
                    return info;

                case SubgraphAction subgraphAction:
                    if (string.IsNullOrEmpty(subgraphAction.subgraphId))
                    {
                        return "Subgraph: Unasigned";
                    }
                    else
                    {
                        var graph = CustomEditorWindow.instance.System.Data.graphs.Find(g => g.id == subgraphAction.subgraphId);
                        if (graph == null) return "Subgraph: missing subgraph";
                        else return "Subgraph: " + graph.name;
                    }

                default:
                    return null;

            }
        }

        public static string GetPerceptionInfo(this Perception perception)
        {
            switch (perception)
            {
                case CustomPerception customPerception:
                    var perceptionMethodLines = new List<string>();

                    var code = customPerception.init.GetSerializedMethodText();
                    if (code != null) perceptionMethodLines.Add(code);

                    code = customPerception.check.GetSerializedMethodText();
                    if (code != null) perceptionMethodLines.Add(code);
                    else perceptionMethodLines.Add("() => false;");

                    code = customPerception.reset.GetSerializedMethodText();
                    if (code != null) perceptionMethodLines.Add(code);

                    return perceptionMethodLines.Join("\n");


                case UnityPerception unityPerception:

                    string info = unityPerception.DisplayInfo;

                    var type = unityPerception.GetType();
                    var properties = type.GetFields();

                    for (int i = 0; i < properties.Length; i++)
                    {
                        string value = GetPropertyDisplay(properties[i].GetValue(unityPerception));
                        info = info.Replace($"${properties[i].Name}", value);
                    }
                    return info;

                case CompoundPerceptionWrapper compoundPerception:
                    var compoundType = compoundPerception.compoundPerception.GetType();
                    var logicCharacter = compoundType == typeof(AndPerception) ? " && " :
                        compoundType == typeof(OrPerception) ? " || " : " - ";
                    return "(" + compoundPerception.subPerceptions.Select(sub => GetPerceptionInfo(sub.perception)).Join(logicCharacter) + ")";
                default:
                    return null;
            }
        }

        public static string GetSerializedMethodText(this SerializedContextMethod contextMethod)
        {
            if (string.IsNullOrWhiteSpace(contextMethod.methodName)) return null;
            return $"{(string.IsNullOrEmpty(contextMethod.componentName) ? "$runner" : contextMethod.componentName)}.{contextMethod.methodName};";
        }


        private static string GetPropertyDisplay(object property)
        {
            if (property == null) return null;
            switch (property)
            {
                case Color color:
                    var colorTag = $"#{ColorUtility.ToHtmlStringRGB(color)}";
                    return $"<color={colorTag}>color</color>";
                default:
                    return property.ToString();
            }
        }


        public static SerializedProperty AddElement(this SerializedProperty prop)
        {
            int size = prop.arraySize;
            prop.InsertArrayElementAtIndex(size);
            return prop.GetArrayElementAtIndex(size);
        }

        public static void MoveAtFirst<T>(this List<T> list, T element)
        {
            if (list.Remove(element)) list.Insert(0, element);
        }

        #endregion
    }
}
