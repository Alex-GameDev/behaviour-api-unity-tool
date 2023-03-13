using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using Codice.Utils;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    public class ScriptTemplate
    {
        bool useFullTypeDeclarationName;
        HashSet<string> namespaces = new HashSet<string>();

        List<string> properties = new List<string>();
        List<string> code = new List<string>();

        List<string> methods = new List<string>();

        string currentCodeLine;
        bool lastElementWasAParameter;

        string m_currentGraphName;
        string m_className;
        string[] m_inheritedClassNames;

        Dictionary<object, string> m_variableNamingMap = new Dictionary<object, string>();
        HashSet<string> m_variableNames = new HashSet<string>();

        Dictionary<string, NodeData> m_nodeMap = new Dictionary<string, NodeData>();
        Dictionary<string, GraphData> m_graphMap = new Dictionary<string, GraphData>();

        int identation = 0;

        public ScriptTemplate(string className, bool useFullNameVar, params string[] inheritedClassNames)
        {
            useFullTypeDeclarationName = useFullNameVar;
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
            var includeLines = string.Join("\n", namespaces.Select(l => $"using {l};"));
            var propertyLines = string.Join("\n\t", properties);
            var codeLines = string.Join("\n\t", code);
            var methodLines = string.Join("\n", methods);

            var codeText = $"{includeLines}\n\npublic class {m_className}";
            if (m_inheritedClassNames.Length > 0) codeText += $" : {string.Join(", ", m_inheritedClassNames)}";
            codeText += $"\n{{\n\t{propertyLines}\n\n\t{codeLines}\n";
            codeText += "\n" + methodLines + "\n}";            
            return codeText;
        }

        /// <summary>
        /// Add an include line with the given namespace
        /// </summary>
        public void AddUsingDirective(string include)
        {
             if(!string.IsNullOrEmpty(include)) namespaces.Add(include);
        }

        /// <summary>
        /// Remove an include line with the given namespace
        /// </summary>
        public void RemoveUsingDirective(string include) => namespaces.Remove(include);

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
        /// If the object is null, get a valid id but dont and it to the map.
        /// Returns true if the variable didn't exist yet.
        private bool AddVariable(object obj, ref string varName)
        {
            if(m_variableNamingMap.TryGetValue(obj, out string existingName))
            {
                varName = existingName;
                return false;
            }
            else
            {
                varName = GetValidIdentificatorName(varName);
            }

            if(obj.ToString() != "null")
            {
                m_variableNamingMap.Add(obj, varName);
            }           
            
            return true;
        }

        private string GetValidIdentificatorName(string varName)
        {
            varName = varName.ToValidIdentificatorName();
            if (m_variableNames.Contains(varName))
            {
                var i = 2;
                var fixedName = $"{varName}_{i}";
                while (m_variableNames.Contains(fixedName))
                {
                    i++;
                    fixedName = $"{varName}_{i}";
                }
                varName = fixedName;
            }
            m_variableNames.Add(varName);
            return varName;
        }

        #region -------------------------- Instructions --------------------------

        /// <summary>
        /// Add a variable declaration in the code.
        /// If the property is reference type, add a line in the property declaration section and returns the variable name. 
        /// If the property is value type, returns its value in code format.
        /// [SerializeField] VARTYPE VARNAME;  -> VARNAME
        /// </summary>
        public string AddVariableDeclaration(Type varType, string varName, object obj, bool isPublic = false, bool isSerialized = true)
        {
            if (varType.IsEnum)
            {
                return $"{varType.Name}.{obj}";
            }
            else if(varType.IsValueType)
            {
                return ToCode(obj);
            }
            else
            {
                if(varType == typeof(string))
                {
                    return $"\"{obj}\"";
                }

                var ns = varType.Namespace;
                AddUsingDirective(ns);

                if (AddVariable(obj, ref varName))
                {
                    var line = $"{(isSerialized ? "[SerializeField]" : "")} {(isPublic ? "public " : "")} {varType.Name} {varName};";
                    properties.Add(line);
                }
                return varName;
            }         
        }

        /// <summary>
        /// Add a line in the code section, inside a method. If the property exists yet, add a reassignation.
        /// TYPENAME VARNAME = new TYPE
        /// </summary>
        public string AddVariableInstantiationLine(Type type, string varName, object obj, params object[] args)
        {
            var typeName = type.Name;
            var argsCode = args.Select(arg => AddVariableDeclaration(arg.GetType(), $"{varName}_arg", arg));
            return AddVariableDeclarationLine(type, varName, obj, $"new {typeName}({string.Join(", ", argsCode)})");
        }

        /// <summary>
        /// Add a line in the code section, inside a method. If the property exists yet, add a reassignation.
        /// TYPENAME VARNAME = new TYPE
        /// </summary>
        public string AddVariableInstantiationLine(Type type, string varName, object obj, IEnumerable<object> args)
        {
            var typeName = type.Name;
            var argsCode = args.Select(arg => AddVariableDeclaration(arg.GetType(), $"{varName}_arg", arg));
            return AddVariableDeclarationLine(type, varName, obj, $"new {typeName}({string.Join(", ", argsCode)})");
        }

        /// <summary>
        /// 
        /// TYPENAME VARNAME = METHOD;
        /// </summary>

        public string AddVariableDeclarationLine(Type type, string varName, object obj, string methodCall)
        {
            var ns = type.Namespace;
            AddUsingDirective(ns);

            var typeName = useFullTypeDeclarationName ? type.Name : "var";

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
            else if (obj is bool b) return b.ToCodeFormat();
            else if (obj is char c) return $"\'{c}\'";
            else if (obj is Vector2 v2) return $"new Vector2({ToCode(v2.x)}, {ToCode(v2.y)})";
            else if (obj is Vector3 v3) return $"new Vector3({ToCode(v3.x)}, {ToCode(v3.y)}, {ToCode(v3.z)})";
            else return default;
        }

        internal NodeData FindNode(string id)
        {
            return m_nodeMap.GetValueOrDefault(id);
        }

        internal object AddComponentVariable(Type componentType)
        {
            throw new NotImplementedException();
        }

        internal string AddEmptyMethod(string methodName, Type returnType)
        {
            string m = "\tpublic " + (returnType?.Name ?? "void") + " " + methodName + "()\n\t{\n\t\tthrow new NotImplementedException();\n\t}\n";
            methods.Add(m);
            return methodName;
        }

        internal string FindGraphVarName(string subgraphId)
        {
            var graphData = m_graphMap.GetValueOrDefault(subgraphId);
            if(graphData == null) return null;
            return m_variableNamingMap.GetValueOrDefault(graphData);
        }

        internal void RegisterGraph(GraphData graphAsset)
        {
            m_graphMap[graphAsset.id] = graphAsset;
            graphAsset.nodes.ForEach(n => m_nodeMap[n.id] = n);
        }
    }
}
