namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    public struct CodeGenerationOptions
    {
        public string scriptNamespace;
        public string scriptClassName;

        public bool includeNames;
        public bool useVarKeyword;

        public bool registerGraphsInDebugger;
        public bool createTasksInline;

        public bool openBracketsInSameLine;
    }
}
