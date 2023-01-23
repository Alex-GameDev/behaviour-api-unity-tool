using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public static class ScriptGeneration
    {
        public static void GenerateScript(string path, string name, BehaviourSystemAsset asset)
        {
            //var scriptPath = EditorUtility.SaveFilePanel("Select a folder to save the script", path, $"{name}.cs", "CS");
            var scriptPath = $"{path}{name}.cs";
            if(!string.IsNullOrEmpty(scriptPath))
            {
                Object obj = CreateScript(scriptPath, asset);
                AssetDatabase.Refresh();
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }

        static Object CreateScript(string path, BehaviourSystemAsset asset)
        {
            string folderPath = path.Substring(0, path.LastIndexOf("/") + 1);
            string scriptName = path.Substring(path.LastIndexOf("/") + 1).Replace(".cs", "");

            ScriptTemplate scriptTemplate = new ScriptTemplate(scriptName, nameof(CodeBehaviourRunner));

            // Add main using directives
            scriptTemplate.AddUsingDirective("BehaviourAPI.Core");
            scriptTemplate.AddUsingDirective("BehaviourAPI.Unity.Runtime");
            scriptTemplate.AddUsingDirective("BehaviourAPI.StateMachines");
            scriptTemplate.AddUsingDirective("BehaviourAPI.BehaviourTrees");
            scriptTemplate.AddUsingDirective("BehaviourAPI.UtilitySystems");

            scriptTemplate.OpenMethodDeclaration("CreateGraph", nameof(BehaviourGraph), "protected override");

            // Add the class

            var rootName = "";

            for(int i = 0; i < asset.Graphs.Count; i++)
            {
                var graph = asset.Graphs[i].Graph;

                if(graph == null)
                {
                    Debug.LogWarning($"Graph {i} is empty.");
                    scriptTemplate.AddLine($"//Graph {i} is empty.");
                    continue;
                }

                var converter = GraphConverter.FindConverter(graph);
                var graphName = converter.AddCreateGraphLine(asset.Graphs[i], scriptTemplate);

                if (i == 0)
                {
                    rootName = graphName;
                }
            }

            //for (int i = 0; i < asset.Graphs.Count; i++)
            //{
            //    var graph = asset.Graphs[i].Graph;
            //    var converter = GraphConverter.FindConverter(graph);
            //    converter.ConvertAssetToCode(asset.Graphs[i], scriptTemplate);
            //}

            if (!string.IsNullOrEmpty(rootName))
            {
                scriptTemplate.AddLine("");
                scriptTemplate.AddLine($"return {rootName};");
            }

            scriptTemplate.CloseMethodDeclaration();

            var content = scriptTemplate.ToString();

            UTF8Encoding encoding = new UTF8Encoding(true, false);
            StreamWriter writer = new StreamWriter(Path.GetFullPath(path), false, encoding);
            writer.Write(content);
            writer.Close();

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }
    }
}
