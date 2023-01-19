using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
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

            ScriptTemplate scriptTemplate = new ScriptTemplate();
            scriptTemplate.AddUsingDirective("BehaviourAPI.Core");
            scriptTemplate.AddUsingDirective("BehaviourAPI.Unity.Runtime");
            scriptTemplate.AddUsingDirective("BehaviourAPI.StateMachines");
            scriptTemplate.AddUsingDirective("BehaviourAPI.BehaviourTrees");
            scriptTemplate.AddUsingDirective("BehaviourAPI.UtilitySystems");

            scriptTemplate.AddClassBegin(scriptName, "public", nameof(CodeBehaviourRunner));
            scriptTemplate.AddMethodBegin("CreateGraph", "protected override", returnType: nameof(BehaviourGraph));
            var rootName = "";
            asset.Graphs.ForEach(graph =>
            {
                var converter = GraphConverter.FindRenderer(graph.Graph);
                converter.ConvertAssetToCode(graph, scriptTemplate);

                if(asset.RootGraph == graph) rootName = graph.Name;

            });
            if(asset.RootGraph != null) scriptTemplate.AddLine($"return {rootName};");

            scriptTemplate.AddMethodClose();
            scriptTemplate.AddClassEnd();            

            var content = scriptTemplate.GetContent();

            UTF8Encoding encoding = new UTF8Encoding(true, false);
            StreamWriter writer = new StreamWriter(Path.GetFullPath(path), false, encoding);
            writer.Write(content);
            writer.Close();

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }
    }
}
