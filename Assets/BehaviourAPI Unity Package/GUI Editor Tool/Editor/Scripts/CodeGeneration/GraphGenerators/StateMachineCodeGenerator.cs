using UnityEngine;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    using BehaviourAPI.Core;
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
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
            var graphStatement = new CodeVariableDeclarationStatement(type, GraphIdentificator);
            graphStatement.RightExpression = new CodeObjectCreationExpression(type);

            template.AddNamespace("BehaviourAPI.StateMachines");
            template.AddStatement(graphStatement, true);

            foreach (var nodeData in graphData.nodes)
            {
                GenerateNodeCode(nodeData, template);
            }
        }

        private void GenerateNodeCode(NodeData data, CodeTemplate template)
        {
            if (data == null) return;
            if (IsGenerated(data.id)) return;

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement(data.node.GetType(), template.GetSystemElementIdentificator(data.id));

            switch (data.node)
            {
                case State state:
                    nodeDeclaration.RightExpression = GenerateStateCode(state, data, template); break;
                case ProbabilisticState pState:
                    nodeDeclaration.RightExpression = GenerateProbabilisticStateCode(pState, data, template); break;
                case StateTransition stateTransition:
                    nodeDeclaration.RightExpression = GenerateStateTransitionCode(stateTransition, data, template); break;
                case ExitTransition exitTransition:
                    nodeDeclaration.RightExpression = GenerateExitTransitionCode(exitTransition, data, template); break;
            }
            template.AddStatement(nodeDeclaration);
            MarkGenerated(data.id);
        }


        private CodeNodeCreationMethodExpression GenerateStateCode(State state, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_StateMethod);

            initMethod.Add(template.GetActionExpression(state.ActionReference, template.GetSystemElementIdentificator(data.id) + "_action"));

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateProbabilisticStateCode(ProbabilisticState state, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_ProbabilisticStateMethod);

            initMethod.Add(template.GetActionExpression(state.ActionReference, template.GetSystemElementIdentificator(data.id) + "_action"));

            var probNum = Mathf.Max(data.childIds.Count, state.probabilities.Count);

            for (int i = 0; i < probNum; i++)
            {
                var id = template.GetSystemElementIdentificator(data.childIds[i]);
                if (id != null && state.probabilities[i] > 0)
                {
                    var nodeId = template.GetSystemElementIdentificator(data.id);
                    CodeCustomStatement probAssignation = new CodeCustomStatement($"{nodeId}.SetProbability({state.probabilities[i]});");
                    template.AddStatement(probAssignation);
                }
            }

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateStateTransitionCode(StateTransition stateTransition, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_StateTransitionMethod);

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of parents is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }

            CreateTransitionArguments(initMethod, template.GetSystemElementIdentificator(data.id), stateTransition.ActionReference, stateTransition.PerceptionReference, stateTransition.StatusFlags, template);

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateExitTransitionCode(ExitTransition exitTransition, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_ExitTransitionMethod);

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of parents is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }

            initMethod.Add(template.CreateGenericExpression(exitTransition.ExitStatus.ToCodeFormat()));

            CreateTransitionArguments(initMethod, template.GetSystemElementIdentificator(data.id), exitTransition.ActionReference, exitTransition.PerceptionReference, exitTransition.StatusFlags, template);

            return initMethod;
        }

        protected void CreateTransitionArguments(CodeNodeCreationMethodExpression expression, string identifier, Action action, Perception perception, StatusFlags flags, CodeTemplate template)
        {
            bool lastArgumentAdded = true;

            if (action != null)
            {
                expression.Add(template.GetActionExpression(action, identifier + "_action"));
            }
            else
            {
                lastArgumentAdded = false;
            }

            if (perception != null)
            {
                expression.Add(new CodeNamedExpression(lastArgumentAdded ? null : "action", template.GetPerceptionExpression(perception, identifier + "_perception")));
            }
            else
            {
                lastArgumentAdded = false;
            }
            if (flags != Core.StatusFlags.Active)
            {
                expression.Add(new CodeNamedExpression(lastArgumentAdded ? null : "statusFlags", template.CreateGenericExpression(flags.ToCodeFormat())));
            }
        }
    }
}