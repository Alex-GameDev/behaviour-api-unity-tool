using System.Collections.Generic;
using System.Linq;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    public class IdentificatorProvider
    {
        private static readonly string[] k_Keywords = new[]
        {
            "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "double", "float", "decimal",
            "string", "char", "void", "object", "typeof", "sizeof", "null", "true", "false", "if", "else", "while", "for", "foreach", "do", "switch",
            "case", "default", "lock", "try", "throw", "catch", "finally", "goto", "break", "continue", "return", "public", "private", "internal",
            "protected", "static", "readonly", "sealed", "const", "fixed", "stackalloc", "volatile", "new", "override", "abstract", "virtual",
            "event", "extern", "ref", "out", "in", "is", "as", "params", "__arglist", "__makeref", "__reftype", "__refvalue", "this", "base",
            "namespace", "using", "class", "struct", "interface", "enum", "delegate", "checked", "unchecked", "unsafe", "operator", "implicit", "explicit"
        };

        HashSet<string> m_UsedIdentificators = new HashSet<string>();
        HashSet<string> m_KeywordSet = k_Keywords.ToHashSet();

        public string GenerateIdentificator(string defaultName)
        {
            string str = defaultName.RemoveWhitespaces();
            str = str.Replace('-', '_');
            str = str.RemovePunctuationsAndSymbols();

            if (string.IsNullOrEmpty(str)) str = "variable";
            else if (char.IsDigit(str[0])) str = "_" + str;
            else if (m_KeywordSet.Contains(str)) str = "@" + str;

            int i = 1;
            string fixedName = str;
            while (m_UsedIdentificators.Contains(fixedName))
            {
                fixedName = str + "_" + i;
                i++;
            }
            m_UsedIdentificators.Add(fixedName);
            return fixedName;
        }
    }
}
