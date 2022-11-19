using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{

    #region variables

    [SerializeField] private GameObject testBoy;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private KeyRotation key;

    #endregion variables

    public void Restart()
    {
        if (key.toggle.isOn) key.gameObject.SetActive(true);

        GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");
        if (oldPlayer != null)
        {
            Destroy(oldPlayer);
        }

        Instantiate(testBoy, spawnPoint.position, spawnPoint.rotation);
    }
}