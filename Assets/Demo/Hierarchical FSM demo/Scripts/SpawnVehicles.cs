using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnVehicles : MonoBehaviour
{
    #region variables

    [SerializeField] private List<GameObject> _vehicles = new List<GameObject>();

    #endregion variables

    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating("SpawnVehicle", 2, 4);
    }

    private void SpawnVehicle()
    {
        int vehicleIndex = Random.Range(0, _vehicles.Count);
        Instantiate(_vehicles[vehicleIndex], transform.position, transform.rotation);
    }
}