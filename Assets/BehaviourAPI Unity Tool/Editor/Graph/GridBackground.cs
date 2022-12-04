using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GridBackground : ImmediateModeElement
    {
        static readonly float s_DefaultSpacing = 50f;
        static readonly int s_DefaultThickLines = 10;
        static readonly Color s_DefaultLineColor = new Color(0f, 0f, 0f, 0.18f);
        static readonly Color s_DefaultThickLineColor = new Color(0f, 0f, 0f, 0.38f);
        static readonly Color s_DefaultGridBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1.0f);

        VisualElement _container;

        protected override void ImmediateRepaint()
        {
            _container = parent.contentContainer;
            var clientRect = _container.layout;
            var containerScale = new Vector3(_container.transform.matrix.GetColumn(0).magnitude,
                _container.transform.matrix.GetColumn(1).magnitude,
                _container.transform.matrix.GetColumn(2).magnitude);
            var containerTranslation = _container.transform.matrix.GetColumn(3);
            var containerPosition = _container.layout;

            GL.Begin(GL.QUADS);
            GL.Color(s_DefaultGridBackgroundColor);
            GL.Vertex(new Vector3(clientRect.x, clientRect.y));
            GL.Vertex(new Vector3(clientRect.xMax, clientRect.y));
            GL.Vertex(new Vector3(clientRect.xMax, clientRect.yMax));
            GL.Vertex(new Vector3(clientRect.x, clientRect.yMax));
            GL.End();

            Vector3 from = new Vector3(clientRect.x, clientRect.y, 0.0f);
            Vector3 to = new Vector3(clientRect.x, clientRect.height, 0.0f);

            var tx = Matrix4x4.TRS(containerTranslation, Quaternion.identity, Vector3.one);
            from = tx.MultiplyPoint(from);
            to = tx.MultiplyPoint(to);

            from.x += (containerPosition.x * containerScale.x);
            from.y += (containerPosition.y * containerScale.y);
            to.x += (containerPosition.x * containerScale.x);
            to.y += (containerPosition.y * containerScale.y);

            while(from.x < clientRect.width)
            {
                from.x += s_DefaultSpacing * containerScale.x;
                to.x += s_DefaultSpacing * containerScale.x;

                GL.Begin(GL.LINES);
                GL.Color(s_DefaultLineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();
            }

            from = new Vector3(clientRect.x, clientRect.y, 0.0f);
            to = new Vector3(clientRect.x + clientRect.width, clientRect.y, 0.0f);

            from.x += (containerPosition.x * containerScale.x);
            from.y += (containerPosition.y * containerScale.y);
            to.x += (containerPosition.x * containerScale.x);
            to.y += (containerPosition.y * containerScale.y);

            from = tx.MultiplyPoint(from);
            to = tx.MultiplyPoint(to);

            from.y = to.y = (from.y % (s_DefaultSpacing * (containerScale.y)) - (s_DefaultSpacing * (containerScale.y)));
            from.x = clientRect.x;
            to.x = clientRect.width;

            while (from.y < clientRect.height)
            {
                from.y += s_DefaultSpacing * containerScale.y;
                to.y += s_DefaultSpacing * containerScale.y;

                GL.Begin(GL.LINES);
                GL.Color(s_DefaultLineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();
            }
        }

        Vector3 Clip(Rect clipRect, Vector3 _in)
        {
            if (_in.x < clipRect.xMin)
                _in.x = clipRect.xMin;
            if (_in.x > clipRect.xMax)
                _in.x = clipRect.xMax;

            if (_in.y < clipRect.yMin)
                _in.y = clipRect.yMin;
            if (_in.y > clipRect.yMax)
                _in.y = clipRect.yMax;

            return _in;
        }
    }
}
