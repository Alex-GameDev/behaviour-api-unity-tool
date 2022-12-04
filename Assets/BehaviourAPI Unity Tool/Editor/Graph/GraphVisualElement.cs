using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class GraphVisualElement : VisualElement
    {
        public GraphVisualElement()
        {
            AddGridBackground();
            AddManipulators();
            AddStyles();
        }

        void AddManipulators()
        {
            //this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void AddStyles()
        {
            StyleSheet styleSheet = VisualSettings.GetOrCreateSettings().GraphStylesheet;
            styleSheets.Add(styleSheet);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }

        public override bool canGrabFocus => base.canGrabFocus;

        public override FocusController focusController => base.focusController;

        public override VisualElement contentContainer => base.contentContainer;

        public override void Blur()
        {
            base.Blur();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            return base.ContainsPoint(localPoint);
        }

        public override void HandleEvent(EventBase evt)
        {
            base.HandleEvent(evt);
        }

        public override bool Overlaps(Rect rectangle)
        {
            return base.Overlaps(rectangle);
        }

        protected override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
        {
            return base.DoMeasure(desiredWidth, widthMode, desiredHeight, heightMode);
        }

        protected override void ExecuteDefaultAction(EventBase evt)
        {
            base.ExecuteDefaultAction(evt);
        }

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
        }
    }
}
