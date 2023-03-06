using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class EdgeControl : UnityEditor.Experimental.GraphView.EdgeControl
    {
        public EdgeView edgeView;

        public VisualElement edgeTag;
        public Label edgeNumberLabel;

        public EdgeControl()
        {
            edgeTag = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(BehaviourAPISettings.instance.EditorLayoutsPath + "edgetag.uxml").Instantiate();
            edgeTag.style.position = Position.Absolute;
            edgeTag.style.left = new StyleLength(new Length(50, LengthUnit.Percent));
            edgeTag.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
            edgeNumberLabel = edgeTag.Q<Label>("edge-number-label");
            Add(edgeTag);
        }

        protected override void ComputeControlPoints()
        {
            var inputDir = Vector2.zero;
            var outputDir = Vector2.zero;

            if (edgeView.input is PortView inputPortView)
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

            if (delta < minDelta)
            {
                delta = minDelta;
            }

            if (delta > 30f) delta = 30f;

            controlPoints[1] = controlPoints[0] + delta * outputDir;
            controlPoints[2] = controlPoints[3] + delta * inputDir;
        }

        public void UpdateIndex(int id)
        {
            edgeNumberLabel.text = id.ToString();
            if(id == 0)
            {
                edgeNumberLabel.Disable();
            }
            else
            {
                edgeNumberLabel.Enable();
            }
        }
    }
}
