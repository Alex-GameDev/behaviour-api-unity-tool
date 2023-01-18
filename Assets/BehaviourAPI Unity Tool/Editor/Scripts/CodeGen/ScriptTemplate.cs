using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace BehaviourAPI.Unity.Editor
{
    public class ScriptTemplate
    {
        List<string> includes = new List<string>();
        List<string> lines = new List<string>();

        int identationLevel = 0;

        const string identationChar = "\t";

        public void AddInclude(string include)
        {
            if (!lines.Contains(include)) lines.Add(include);
        }

        public string GetContent()
        {
            var includeLines = string.Join("\n", includes.Select(l => $"using {l};"));
            var codeLines = string.Join("\n", lines);
            return includeLines + '\n' + codeLines;
        }

        public void AddEmptyLine() => AddLine("");

        public void AddCommentLine(string comment) => AddLine($"//{comment}");

        public void AddClassBegin(string className, string modifiers = "public",
            params string[] inheritedClassNames)
        {
            var st = $"{modifiers} class {className}";
            if (inheritedClassNames.Length > 0)
            {
                st += $" : {string.Join(", ", inheritedClassNames)}";
            }
            AddLine(st);
            AddOpenBrackedLine();
        }

        public void AddLine(string line)
        {
            lines.Add($"{string.Concat(Enumerable.Repeat(identationChar, identationLevel))}{line}");
        }

        private void AddOpenBrackedLine()
        {
            AddLine("{");
            identationLevel++;
        }

        private void AddClosedBrackedLine()
        {
            identationLevel--;
            AddLine("}");
        }

        public void AddClassEnd() => AddClosedBrackedLine();

        public void AddMethodBegin(string methodName,
            string modifiers = "public",
            string returnType = "void")
        {
            var st = $"{modifiers} {returnType} {methodName}()";
            AddLine(st);
            AddOpenBrackedLine();
        }

        public void AddMethodClose() => AddClosedBrackedLine();

        public void AddUsingDirective(string namespaceName) => includes.Add(namespaceName);

    }

}
