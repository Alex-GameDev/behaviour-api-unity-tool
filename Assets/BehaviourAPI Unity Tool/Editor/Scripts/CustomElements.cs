using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class PortView : Port
    {
        public PortOrientation Orientation;
        protected PortView(PortOrientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation.ToOrientation(), portDirection, portCapacity, type)
        {
            Orientation = portOrientation;
            Decorate();
        }

        public void Decorate()
        {
            bool isOutput = direction == Direction.Output;

            var fixer = new VisualElement();
            fixer.StretchToParentSize();
            Add(fixer);

            if (Orientation == PortOrientation.Top && isOutput || Orientation == PortOrientation.Bottom && !isOutput) 
            { 
                m_ConnectorBox.style.borderTopLeftRadius = 0f; 
                m_ConnectorBox.style.borderTopRightRadius = 0f; 
            }
            else if (Orientation == PortOrientation.Left && isOutput || Orientation == PortOrientation.Right && !isOutput) 
            { 
                m_ConnectorBox.style.borderTopRightRadius = 0f; 
                m_ConnectorBox.style.borderBottomRightRadius = 0f; 
            }
            else if (Orientation == PortOrientation.Right && isOutput || Orientation == PortOrientation.Left && !isOutput) 
            { 
                m_ConnectorBox.style.borderTopLeftRadius = 0f; 
                m_ConnectorBox.style.borderBottomLeftRadius = 0f; 
            }
            else if (Orientation == PortOrientation.Bottom && isOutput || Orientation == PortOrientation.Top && !isOutput) 
            { 
                m_ConnectorBox.style.borderBottomLeftRadius = 0f;
                m_ConnectorBox.style.borderBottomRightRadius = 0f; 
            }
        }

        public static PortView Create(PortOrientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
        {
            DefaultEdgeConnectorListener listener = new DefaultEdgeConnectorListener();
            PortView port = new PortView(portOrientation, portDirection, portCapacity, type)
            {
                m_EdgeConnector = new EdgeConnector<EdgeView>(listener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;        
        }

        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private GraphViewChange m_GraphViewChange;

            private List<Edge> m_EdgesToCreate;

            private List<GraphElement> m_EdgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                m_EdgesToCreate = new List<Edge>();
                m_EdgesToDelete = new List<GraphElement>();
                m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                m_EdgesToCreate.Clear();
                m_EdgesToCreate.Add(edge);
                m_EdgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                {
                    foreach (Edge connection in edge.input.connections)
                    {
                        if (connection != edge)
                        {
                            m_EdgesToDelete.Add(connection);
                        }
                    }
                }

                if (edge.output.capacity == Capacity.Single)
                {
                    foreach (Edge connection2 in edge.output.connections)
                    {
                        if (connection2 != edge)
                        {
                            m_EdgesToDelete.Add(connection2);
                        }
                    }
                }

                if (m_EdgesToDelete.Count > 0)
                {
                    graphView.DeleteElements(m_EdgesToDelete);
                }

                List<Edge> edgesToCreate = m_EdgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
                }

                foreach (Edge item in edgesToCreate)
                {
                    graphView.AddElement(item);
                    edge.input.Connect(item);
                    edge.output.Connect(item);
                }
            }
        }
    }

    public class EdgeControl : UnityEditor.Experimental.GraphView.EdgeControl
    {
        public EdgeView edgeView;
        protected override void ComputeControlPoints()
        {
            var inputDir = Vector2.zero;
            var outputDir = Vector2.zero;

            if(edgeView.input is PortView inputPortView)
            {
                inputDir = inputPortView.Orientation.ToVector();
            }

            if (edgeView.output is PortView outputPortView)
            {
                outputDir = outputPortView.Orientation.ToVector();
            }


            base.ComputeControlPoints();
            var minDelta = 16f;
            var delta = (controlPoints[3] - controlPoints[0]).magnitude * .25f;
            
            if(delta < minDelta)
            {
                delta = minDelta;
            }

            if (delta > 30f) delta = 30f;

            controlPoints[1] = controlPoints[0] + delta * outputDir;
            controlPoints[2] = controlPoints[3] + delta * inputDir;
        }
    }

    public class EdgeView : Edge
    {
        protected override UnityEditor.Experimental.GraphView.EdgeControl CreateEdgeControl()
        {
            return new EdgeControl()
            {
                capRadius = 4.0f,
                interceptWidth = 6.0f,
                edgeView = this
            };
        }
    }

    public enum PortOrientation
    {
        None = 0,
        Top = 1,
        Right = 2,
        Bottom = 3,
        Left = 4     
    }

    public static class VisualElementExtensions
    {
        public static bool IsHorizontal(this PortOrientation portOrientation) => portOrientation == PortOrientation.Left || portOrientation == PortOrientation.Right;

        public static Orientation ToOrientation(this PortOrientation portOrientation)
        {
            if (portOrientation.IsHorizontal()) return Orientation.Horizontal;
            else return Orientation.Vertical;
        }

        public static Vector2 ToVector(this PortOrientation portOrientation)
        {
            if (portOrientation == PortOrientation.Top) return Vector2.up;
            if (portOrientation == PortOrientation.Bottom) return Vector2.down;
            if (portOrientation == PortOrientation.Left) return Vector2.left;
            if (portOrientation == PortOrientation.Right) return Vector2.right;
            else return Vector2.zero;
        }

        public static FlexDirection ToFlexDirection(this PortOrientation portOrientation)
        {
            if (portOrientation == PortOrientation.Top) return FlexDirection.ColumnReverse;
            if (portOrientation == PortOrientation.Bottom) return FlexDirection.Column;
            if (portOrientation == PortOrientation.Left) return FlexDirection.Row;
            if (portOrientation == PortOrientation.Right) return FlexDirection.RowReverse;
            else return FlexDirection.Column;
        }
    }
}
