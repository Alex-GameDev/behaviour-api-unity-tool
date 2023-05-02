using System;
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    public abstract class CodeElement
    {
        public abstract void GenerateCode(CodeWriter writer, CodeGenerationOptions options);
    }

    public abstract class CodeMember : CodeElement
    {
    }

    public class CodeFieldMember : CodeMember
    {
        private bool isPublic;
        private bool isSerializeField;

        public string name;
        public Type type;

        public CodeExpression InitExpression;

        public CodeFieldMember(string name, Type type, bool isPublic = false, bool isSerializeField = true)
        {
            this.name = name;
            this.type = type;
            this.isPublic = isPublic;
            this.isSerializeField = isSerializeField;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            if (isSerializeField) writer.Append("[SerializeField] ");
            writer.Append(isPublic ? "public " : "private ");
            writer.Append(type.GetTypeName() + " " + name);
            if (InitExpression != null) InitExpression.GenerateCode(writer, options);
            writer.AppendLine(";");
        }
    }

    public class CodeMethodMember : CodeMember
    {
        public string Name;
        public Type returnType;

        public List<CodeParameter> Parameters = new List<CodeParameter>();
        public CodeMethodMember(string name, Type returnType)
        {
            Name = name;
            this.returnType = returnType;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.AppendLine("");

            var typeName = returnType == null ? "void" : returnType == typeof(float) ? "float" : returnType.Name;

            writer.Append($"private {typeName} {Name}(");

            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i].GenerateCode(writer, options);
                if (i != Parameters.Count - 1) writer.Append(", ");
            }

            writer.AppendLine(")");
            writer.AppendLine("{");
            writer.IdentationLevel++;
            writer.AppendLine("throw new System.NotImplementedException();");
            writer.IdentationLevel--;
            writer.AppendLine("}");
        }
    }

    public class CodeParameter : CodeElement
    {
        public Type type;
        public string name;

        public CodeParameter(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.Append(type.Name + " " + name);
        }
    }


    public abstract class CodeStatement : CodeElement
    {
    }

    public class CodeVariableDeclarationStatement : CodeStatement
    {
        public Type Type;
        public string Identifier;
        public CodeExpression RightExpression;

        public CodeVariableDeclarationStatement(Type type, string identifier)
        {
            Type = type;
            Identifier = identifier;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            var typeName = options.useVarKeyword ? "var" : Type.Name;
            writer.Append($"{typeName} {Identifier} = ");
            if (RightExpression != null)
                RightExpression.GenerateCode(writer, options);
            else
                writer.Append("null");
            writer.AppendLine(";");
        }
    }

    public class CodeAssignationStatement : CodeStatement
    {
        public CodeExpression LeftExpression;
        public CodeExpression RightExpression;
        public override void GenerateCode(CodeWriter codeWritter, CodeGenerationOptions options)
        {
            LeftExpression.GenerateCode(codeWritter, options);
            codeWritter.Append(" = ");
            RightExpression.GenerateCode(codeWritter, options);
            codeWritter.AppendLine(";");
        }
    }

    public class CodeCustomStatement : CodeStatement
    {
        string statement;

        public CodeCustomStatement(string statement)
        {
            this.statement = statement;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.AppendLine(statement);
        }
    }

    public abstract class CodeExpression : CodeElement
    {
    }

    public class CodeMethodReferenceExpression : CodeExpression
    {
        public string invokerIdentifier;
        public string memberName;

        public CodeMethodReferenceExpression(string memberName)
        {
            this.memberName = memberName;
        }

        public CodeMethodReferenceExpression(string invokerIdentifier, string memberName)
        {
            this.invokerIdentifier = invokerIdentifier;
            this.memberName = memberName;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            if (invokerIdentifier != null) writer.Append(invokerIdentifier + ".");
            writer.Append(memberName);
        }
    }

    public class CodeMMethodInvokeExpression : CodeExpression
    {
        public CodeMethodReferenceExpression methodReferenceExpression;
        public List<CodeExpression> parameters = new List<CodeExpression>();

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            methodReferenceExpression.GenerateCode(writer, options);
            writer.Append("(");
            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].GenerateCode(writer, options);
                if (i != parameters.Count - 1) writer.Append(", ");
            }
            writer.Append(")");
        }

        public void Add(CodeExpression expression)
        {
            parameters.Add(expression);
        }
    }

    public class CodeNodeCreationMethodExpression : CodeMMethodInvokeExpression
    {
        public string nodeName;

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            methodReferenceExpression.GenerateCode(writer, options);
            writer.Append("(");
            if (options.includeNames && !string.IsNullOrEmpty(nodeName))
            {
                writer.Append('\"' + nodeName + '\"');
                if (parameters.Count > 0) writer.Append(", ");
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].GenerateCode(writer, options);
                if (i != parameters.Count - 1) writer.Append(", ");
            }
            writer.Append(")");
        }
    }

    public class CodeObjectCreationExpression : CodeExpression
    {
        public Type Type;

        public List<CodeExpression> parameters = new List<CodeExpression>();

        public CodeObjectCreationExpression(Type type)
        {
            Type = type;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.Append($"new {Type.GetTypeName()}(");

            for (int i = 0; i < parameters.Count; i++)
            {
                parameters[i].GenerateCode(writer, options);
                if (i != parameters.Count - 1) writer.Append(", ");
            }

            writer.Append(")");
        }

        public void Add(CodeExpression parameter)
        {
            parameters.Add(parameter);
        }
    }

    public class CodeCustomExpression : CodeExpression
    {
        public string Expression;

        public CodeCustomExpression(string expression)
        {
            Expression = expression;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.Append(Expression);
        }
    }

    public class CodeNamedExpression : CodeExpression
    {
        public string name;
        public CodeExpression expression;

        public CodeNamedExpression(string name, CodeExpression expression)
        {
            this.name = name;
            this.expression = expression;
        }

        public override void GenerateCode(CodeWriter writer, CodeGenerationOptions options)
        {
            writer.Append(name + ": ");
            expression.GenerateCode(writer, options);
        }
    }
}
