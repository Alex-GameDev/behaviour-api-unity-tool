using BehaviourAPI.Unity.Runtime;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class ScriptTemplate
    {
        List<string> includes = new List<string>();    
        List<string> properties = new List<string>();
        List<string> graphDeclarations = new List<string>();
        List<string> code = new List<string>();

        string currentCodeLine;
        bool lastElementWasAParameter;

        string m_currentGraphName;
        string m_className;
        string[] m_inheritedClassNames;

        Dictionary<object, string> m_variableNamingMap = new Dictionary<object, string>();
        HashSet<string> m_variableNames = new HashSet<string>();

        int identation = 0;

        public ScriptTemplate(string className, params string[] inheritedClassNames)
        {
            m_className = className;
            m_inheritedClassNames = inheritedClassNames;
        }

        public string FindVariableName(object obj)
        {
            if(obj == null) return null;
            else return m_variableNamingMap.GetValueOrDefault(obj);
        }

        /// <summary>
        /// Returns the structured content of the template
        /// </summary>
        public override string ToString()
        {
            var includeLines = string.Join("\n", includes.Select(l => $"using {l};"));
            var propertyLines = string.Join("\n\t", properties);
            var codeLines = string.Join("\n\t", code);

            var codeText = $"{includeLines}\n\npublic class {m_className}";
            if (m_inheritedClassNames.Length > 0) codeText += $" : {string.Join(", ", m_inheritedClassNames)}";
            codeText += $"\n{{\n\t{propertyLines}\n\n\t{codeLines}\n}}";
            return codeText;
        }

        /// <summary>
        /// Add an include line with the given namespace
        /// </summary>
        public void AddUsingDirective(string include)
        {
            if (!includes.Contains(include))
            {
                includes.Add(include);
            }
        }

        /// <summary>
        /// Add a custom line after commit the last line.
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(string line)
        {
            if (!string.IsNullOrEmpty(currentCodeLine))
            {
                CommitCurrentLine();
            }
            code.Add(string.Concat(Enumerable.Repeat("\t", identation)) + line);
        }

        /// <summary>
        /// Add a variable to the variable map and change its name to a valid unique identificator.
        /// Returns true if the variable didn't exist yet.
        private bool AddVariable(object obj, ref string varName)
        {
            varName = varName.ToValidIdentificatorName();

            if(m_variableNamingMap.TryGetValue(obj, out string existingName))
            {
                varName = existingName;
                return false;
            }

            if(!m_variableNames.Add(varName))
            {
                var i = 2;
                var fixedName = $"{varName}_{i}";
                while(!m_variableNames.Add(fixedName))
                {
                    i++;
                    fixedName = $"{varName}_{i}";
                }
                varName = fixedName;
            }

            m_variableNamingMap.Add(obj, varName);

            
            return true;
        }

        #region -------------------------- Instructions --------------------------

        /// <summary>
        /// Add a variable declaration in the code.
        /// If the property is reference type, add a line in the property declaration section and returns the variable name. 
        /// If the property is value type, returns its value in code format.
        /// </summary>
        public string AddPropertyLine(Type varType, string varName, object obj, bool isPublic = false, bool isSerialized = true)
        {
            if(varType.IsEnum)
            {
                return $"{varType.Name}.{obj}";
            }
            else if(varType.IsValueType)
            {
                return ToCode(obj);
            }
            else
            {
                string line = "";
                if (AddVariable(obj, ref varName))
                {
                    line = $"{(isSerialized ? "[SerializeField]" : "")} {(isPublic ? "public " : "private")} {varType.Name} {varName};";
                    properties.Add(line);
                }
                return varName;
            }         
        }

        /// <summary>
        /// Add a line in the code section, inside a method. If the property exists yet, add a reassignation.
        /// </summary>
        public string AddVariableInstantiationLine(string typeName, string varName, object obj, params string[] args)
        {
            return AddVariableDeclarationLine(typeName, varName, obj, $"new {typeName}({string.Join(", ", args)})");
        }

        public string AddVariableDeclarationLine(string typeName, string varName, object obj, string methodCall)
        {
            string line;
            if (AddVariable(obj, ref varName))
            {
                line = $"{typeName} {varName}";
            }
            else
            {
                line = $"{varName}";
            }

            line += $" = {methodCall};";
            AddLine(line);
            return varName;
        }

        public void OpenMethodDeclaration(string methodName, string returnType = "void", string modifiers = "public", params string[] parameters)
        {
            AddLine($"{modifiers} {returnType} {methodName}({string.Join(", ", parameters)})");
            OpenBrackets();
        }

        public void CloseMethodDeclaration()
        {
            CloseBrackets();
        }

        public void OpenCreateNodeLine(string variableName, string method)
        {
            if (currentCodeLine.Length > 0) CommitCurrentLine();

            currentCodeLine = $"var {variableName} = {m_currentGraphName}.{method}(";
        }

        public void AddParameter(string parameter)
        {
            if (lastElementWasAParameter) currentCodeLine += ", ";
            currentCodeLine += parameter;
            lastElementWasAParameter = true;
        }

        public void CloseCreateNodeLine(bool finishInstruction = true)
        {
            currentCodeLine += ")";
            if(finishInstruction)
            {
                currentCodeLine += ";";
                CommitCurrentLine();
            }
        }     

        #endregion             
         

        private void OpenBrackets()
        {
            AddLine("{");
            identation++;
        }

        private void CloseBrackets()
        {
            identation--;
            AddLine("}");
        }

        private void CommitCurrentLine()
        {
            code.Add(currentCodeLine);
            currentCodeLine = "";
        }

        string ToCode(object obj)
        {
            if (obj is int i) return i.ToString();
            else if (obj is float f) return f.ToCodeFormat();
            else if (obj is string s) return $"\"{s}\"";
            else if (obj is bool b) return b.ToCodeFormat();
            else if (obj is char c) return $"\'{c}\'";
            else if (obj is Vector2 v2) return $"new Vector2({ToCode(v2.x)}, {ToCode(v2.y)})";
            else if (obj is Vector3 v3) return $"new Vector3({ToCode(v3.x)}, {ToCode(v3.y)}, {ToCode(v3.z)})";
            else return default;
        }
    }
}
