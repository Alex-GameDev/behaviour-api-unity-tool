using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

public class FishingBoyBTVisualRunner : VisualBehaviourRunner
{
    [SerializeField] GameObject _fishPrefab, _bootPrefab;
    [SerializeField] Transform _fishDropTarget, _bootDropTarget, _baitTarget;
    [SerializeField] FishingRod _rod;
    bool _fishCatched;
    GameObject _currentCapture;

    [CustomMethod]
    public void DropCaptureInWater() => DropCapture(_bootPrefab, _bootDropTarget, true);

    [CustomMethod]
    public void StoreCaptureInBasket() => DropCapture(_fishPrefab, _fishDropTarget, false);

    [CustomMethod]
    public void StartCatch()
    {
        _rod.PickUp();

        var catchId = Random.Range(0, 2);
        _fishCatched = catchId == 0;

        var prefab = catchId == 0 ? _fishPrefab : _bootPrefab;

        _currentCapture = Instantiate(prefab, _baitTarget);
        _currentCapture.GetComponent<Rigidbody>().useGravity = false;
    }

    [CustomMethod]
    public void StartThrow() =>  _rod.Throw();

    [CustomMethod]
    public bool IsFishCatched() => _fishCatched;

    [CustomMethod]
    public Status CompleteOnSuccess() => Status.Success;

    [CustomMethod]
    public Status RodThrownStatus() => _rod.IsThrown() ? Status.Success : Status.Running;

    [CustomMethod]
    public Status RodPickedStatus() => _rod.IsPickedUp() ? Status.Success : Status.Running;

    void DropCapture(GameObject capturePrefab, Transform target, bool destroyAfter)
    {
        Destroy(_currentCapture);
        var drop = Instantiate(capturePrefab, target.position, target.rotation);
        drop.transform.localScale = target.localScale;
        if (destroyAfter) Destroy(drop, 2f);
    }
}
