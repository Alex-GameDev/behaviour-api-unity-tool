using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

namespace BehaviourAPI.Unity.Demo
{
    public class FishingBoyBTRunner : CodeBehaviourRunner
    {
        [SerializeField] GameObject _fishPrefab, _bootPrefab;
        [SerializeField] Transform _fishDropTarget, _bootDropTarget, _baitTarget;
        [SerializeField] FishingRod _rod;
        bool _fishCatched;
        GameObject _currentCapture;

        protected override BehaviourGraph CreateGraph(HashSet<BehaviourGraph> registeredGraphs)
        {
            var fishPerception = new ConditionPerception(() => _fishCatched);

            var bt = new BehaviourTree();

            var throwTheRod = bt.CreateLeafNode("Throw rod", new FunctionalAction(StartThrow, () => _rod.IsThrown() ? Status.Success : Status.Running));
            var catchSomething = bt.CreateLeafNode("Catch sonmething", new FunctionalAction(StartCatch, () => _rod.IsPickedUp() ? Status.Success : Status.Running));

            var returnToWater = bt.CreateLeafNode("Return to water", new FunctionalAction(DropCaptureInWater, () => Status.Success));
            var storeInBasket = bt.CreateLeafNode("Store in basket", new FunctionalAction(StoreCaptureInBasket, () => Status.Success));

            var timer = bt.CreateDecorator<UnityTimerDecorator>("Timer", catchSomething).SetTime(3f);
            var check = bt.CreateDecorator<ConditionNode>("Cond", storeInBasket).SetPerception(fishPerception);
            var sel = bt.CreateComposite<SelectorNode>("sel", false, check, returnToWater);
            var seq = bt.CreateComposite<SequencerNode>("seq", false, throwTheRod, timer, sel);

            var loop = bt.CreateDecorator<IteratorNode>("loop", seq).SetIterations(-1);

            bt.SetRootNode(loop);

            registeredGraphs.Add(bt);
            return bt;
        }

        void StartThrow() => _rod.Throw();

        void StartCatch()
        {
            _rod.PickUp();

            var catchId = Random.Range(0, 2);
            _fishCatched = catchId == 0;

            var prefab = catchId == 0 ? _fishPrefab : _bootPrefab;

            _currentCapture = Instantiate(prefab, _baitTarget);
            _currentCapture.GetComponent<Rigidbody>().useGravity = false;
        }

        void DropCaptureInWater() => DropCapture(_bootPrefab, _bootDropTarget, true);

        void StoreCaptureInBasket() => DropCapture(_fishPrefab, _fishDropTarget, false);

        void DropCapture(GameObject capturePrefab, Transform target, bool destroyAfter)
        {
            Destroy(_currentCapture);
            var drop = Instantiate(capturePrefab, target.position, target.rotation);
            drop.transform.localScale = target.localScale;
            if (destroyAfter) Destroy(drop, 2f);
        }
    }
}
