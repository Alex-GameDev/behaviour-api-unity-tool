using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Framework;
    using StateMachines;
    using ExitTransition = Framework.Adaptations.ExitTransition;
    using ProbabilisticState = Framework.Adaptations.ProbabilisticState;
    using State = Framework.Adaptations.State;
    using StateTransition = Framework.Adaptations.StateTransition;

    [CustomGraphCodeGenerator(typeof(FSM))]
    public class StateMachineCodeGenerator : GraphCodeGenerator
    {
        private static readonly string k_StateMethod = "CreateState";
        private static readonly string k_ProbabilisticStateMethod = "CreateProbabilisticState";
        private static readonly string k_StateTransitionMethod = "CreateTransition";
        private static readonly string k_ExitTransitionMethod = "CreateExitTransition";

        public override void GenerateGraphDeclaration(GraphData graphData, CodeTemplate template)
        {
            GraphIdentificator = template.GetSystemElementIdentificator(graphData.id);
            var type = graphData.graph.GetType();
            var graphStatement = new CodeVariableDeclarationStatement()
            {
                Type = type,
                Identificator = GraphIdentificator,
                RightExpression = new CodeObjectCreationExpression(type)
            };

            template.AddNamespace("BehaviourAPI.StateMachines");
            template.AddStatement(graphStatement, true);

            foreach (var nodeData in graphData.nodes)
            {
                GenerateNodeCode(nodeData, template);
            }
        }

        private void GenerateNodeCode(NodeData nodeData, CodeTemplate template)
        {
            if (nodeData == null) return;
            if (IsGenerated(nodeData.id)) return;

            switch (nodeData.node)
            {
                case State state: GenerateStateCode(state, nodeData, template); break;
                case ProbabilisticState pState: GenerateProbabilisticStateCode(pState, nodeData, template); break;
                case StateTransition stateTransition: GenerateStateTransitionCode(stateTransition, nodeData, template); break;
                case ExitTransition exitTransition: GenerateExitTransitionCode(exitTransition, nodeData, template); break;
            }
            MarkGenerated(nodeData.id);
        }


        private void GenerateStateCode(State state, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_StateMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.GetActionExpression(state.ActionReference));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = typeof(State),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateProbabilisticStateCode(ProbabilisticState state, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_ProbabilisticStateMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.GetActionExpression(state.ActionReference));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = typeof(ProbabilisticState),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateStateTransitionCode(StateTransition stateTransition, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_StateTransitionMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of parents is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }

            bool lastArgumentAdded = true;

            if (stateTransition.ActionReference != null)
            {
                initMethod.Add(template.GetActionExpression(stateTransition.ActionReference));
            }
            else
            {
                lastArgumentAdded = false;
            }

            if (stateTransition.PerceptionReference != null)
            {
                initMethod.Add(template.GetPerceptionExpression(stateTransition.PerceptionReference), lastArgumentAdded ? null : "action");
            }
            else
            {
                lastArgumentAdded = false;
            }

            if (stateTransition.StatusFlags != Core.StatusFlags.Active)
            {
                initMethod.Add(template.CreateGenericExpression(stateTransition.StatusFlags.ToCodeFormat()), lastArgumentAdded ? null : "statusFlags");
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = typeof(StateTransition),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateExitTransitionCode(ExitTransition exitTransition, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_ExitTransitionMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of parents is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }

            initMethod.Add(template.CreateGenericExpression(exitTransition.ExitStatus.ToCodeFormat()));
            bool lastArgumentAdded = true;

            if (exitTransition.ActionReference != null)
            {
                initMethod.Add(template.GetActionExpression(exitTransition.ActionReference));
            }
            else
            {
                lastArgumentAdded = false;
            }

            if (exitTransition.PerceptionReference != null)
            {
                initMethod.Add(template.GetPerceptionExpression(exitTransition.PerceptionReference), lastArgumentAdded ? null : "action");
            }
            else
            {
                lastArgumentAdded = false;
            }

            if (exitTransition.StatusFlags != Core.StatusFlags.Active)
            {
                initMethod.Add(template.CreateGenericExpression(exitTransition.StatusFlags.ToCodeFormat()), lastArgumentAdded ? null : "statusFlags");
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = typeof(StateTransition),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }
    }
}
