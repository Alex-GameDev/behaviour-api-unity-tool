using System;
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Editor
{
    #region --------------------------------------- Statements ---------------------------------------

    public abstract class CodeStatement
    {
        public abstract void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options);
    }


    public class CodeVariableDeclarationStatement : CodeStatement
    {
        public Type Type;
        public string Identificator;
        public CodeArgumentExpression RightExpression;

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            var typeName = options.useVarKeyword ? "var" : Type.Name;
            codeWritter.Append($"{typeName} {Identificator} = ");
            RightExpression.GenerateCode(codeWritter, options);
            codeWritter.AppendLine(";");
        }
    }

    public class CodeVariableReassignationStatement : CodeStatement
    {
        public CodeArgumentExpression LeftExpression;
        public CodeArgumentExpression RightExpression;

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            LeftExpression.GenerateCode(codeWritter, options);
            codeWritter.Append(" = ");
            RightExpression.GenerateCode(codeWritter, options);
            codeWritter.AppendLine(";");
        }
    }

    public class CustomStatement : CodeStatement
    {
        string code;

        public CustomStatement(string code)
        {
            this.code = code;
        }

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            codeWritter.AppendLine(code);
        }
    }

    public class CodeMethodMember
    {
        public string Name;
        public Type returnType;
        public List<CodeParameterExpression> parameterExpressions = new List<CodeParameterExpression>();

        public void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            codeWritter.AppendLine("");
            var typeName = returnType == null ? "void" : returnType == typeof(float) ? "float" : returnType.Name;

            codeWritter.Append($"private {typeName} {Name}(");
            if (parameterExpressions != null)
            {
                for (int i = 0; i < parameterExpressions.Count; i++)
                {
                    codeWritter.Append(parameterExpressions[i].type.Name + " " + parameterExpressions[i].name + "_" + i + 1);
                    if (i != parameterExpressions.Count - 1)
                    {
                        codeWritter.Append(", ");
                    }
                }
            }
            codeWritter.AppendLine(")");
            codeWritter.AppendLine("{");
            codeWritter.IdentationLevel++;
            codeWritter.AppendLine("throw new NotImplementedException();");
            codeWritter.IdentationLevel--;
            codeWritter.AppendLine("}");
        }
    }

    public class CodeFieldMember
    {
        public string name;
        public Type type;

        public void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            codeWritter.AppendLine($"[SerializeField] private {type.Name} {name};");
        }
    }

    public class CodeParameterExpression
    {
        public Type type;
        public string name;
    }
    #endregion

    #region --------------------------------------- Expressions ---------------------------------------
    public abstract class CodeArgumentExpression
    {
        public abstract void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options);
    }

    public abstract class CodeArgumentHandlerExpression : CodeArgumentExpression
    {
        List<CodeArgumentExpression> m_Parameters = new List<CodeArgumentExpression>();
        List<string> m_ParameterNames = new List<string>();

        public void Add(CodeArgumentExpression argumentExpression, string optionalParamName = null)
        {
            m_Parameters.Add(argumentExpression);
            m_ParameterNames.Add(optionalParamName);
        }

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            for (int i = 0; i < m_Parameters.Count; i++)
            {
                if (m_ParameterNames[i] != null) codeWritter.Append(m_ParameterNames[i] + ": ");
                m_Parameters[i].GenerateCode(codeWritter, options);
                if (i != m_Parameters.Count - 1)
                {
                    codeWritter.Append(", ");
                }
            }
        }
    }

    public class CodeObjectCreationExpression : CodeArgumentHandlerExpression
    {
        public Type Type;

        public CodeObjectCreationExpression(Type type)
        {
            Type = type;
        }

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            codeWritter.Append($"new {Type.Name}(");
            base.GenerateCode(codeWritter, options);
            codeWritter.Append(")");
        }
    }

    public class CodeMethodInvocationExpression : CodeArgumentHandlerExpression
    {
        public CodeMethodReferenceExpression methodReferenceExpression;

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            if (methodReferenceExpression == null) return;
            methodReferenceExpression.GenerateCode(codeWritter, options);
            codeWritter.Append("(");
            base.GenerateCode(codeWritter, options);
            codeWritter.Append(")");
        }
    }

    public class CodeMethodReferenceExpression : CodeArgumentExpression
    {
        public string methodName;
        public string invokerIdentificator;

        public CodeMethodReferenceExpression(string methodName)
        {
            this.methodName = methodName;
        }

        public CodeMethodReferenceExpression(string invokerIdentificator, string methodName)
        {
            this.methodName = methodName;
            this.invokerIdentificator = invokerIdentificator;
        }

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            if (invokerIdentificator != null) codeWritter.Append(invokerIdentificator + ".");
            codeWritter.Append(methodName);
        }
    }

    public class CodeSimpleExpression : CodeArgumentExpression
    {
        public string code;

        public CodeSimpleExpression(string code)
        {
            this.code = code;
        }

        public override void GenerateCode(CodeWritter codeWritter, CodeGenerationOptions options)
        {
            codeWritter.Append(code);
        }
    }

    #endregion

}
