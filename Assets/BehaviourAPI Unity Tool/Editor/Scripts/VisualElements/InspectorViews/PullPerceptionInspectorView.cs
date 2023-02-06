using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    public class PullPerceptionInspectorView : ListInspectorView<PerceptionAsset>
    {
        BehaviourSystemAsset _systemAsset;
        public NodeSearchWindow nodeSearchWindow { get; set; }
        public PerceptionSearchWindow perceptionSearchWindow { get; set; }

        public Action<PerceptionAsset> PerceptionCreated, PerceptionRemoved;

        public PullPerceptionInspectorView(BehaviourSystemAsset systemAsset, NodeSearchWindow searchWindow, PerceptionSearchWindow pSearchWindow) : base("Perceptions", Side.Right)
        {
            _systemAsset = systemAsset;
            nodeSearchWindow = searchWindow;
            perceptionSearchWindow = pSearchWindow;
        }

        public override void AddElement()
        {
            if (_systemAsset == null) return;

            perceptionSearchWindow.Open(AddElement);
        }

        void AddElement(Type type)
        {
            var asset = _systemAsset.CreatePerception("new pushperception", type);
            RefreshList();
            PerceptionCreated?.Invoke(asset);
        }

        protected override List<PerceptionAsset> GetList()
        {
            if (_systemAsset == null) return new List<PerceptionAsset>();
            return _systemAsset.Perceptions;
        }

        protected override void RemoveElement(PerceptionAsset asset)
        {
            _systemAsset.RemovePerception(asset);
            PerceptionRemoved?.Invoke(asset);
        }

        public void SetSystem(BehaviourSystemAsset systemAsset)
        {
            _systemAsset = systemAsset;
            ResetList();
            UpdateInspector(null);

        }
    }
}
