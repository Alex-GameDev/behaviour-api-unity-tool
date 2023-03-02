using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    public bool IsClosed { get; set; }

    private void Start()
    {
        IsClosed = toggle.isOn;
    }

    public void OnReset()
    {
        IsClosed = toggle.isOn;
    }
}
