/*****************************************************************************
// File Name :         MGWireSlot.cs
// Author :            Andrea Swihart-DeCoster
// Creation Date :     05/21/24
//
// Brief Description : Controls the logic for the wire attachment slot.
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MGWireSlot : MonoBehaviour
{
    public static Action CorrectWire;

    [SerializeField] private MGWire.EWireID _matchingWire;
    [SerializeField] private Color _slotColor;
    [HideInInspector] public bool IsConnected;
    [HideInInspector] public bool IsCorrectWire;

    [SerializeField] private GameObject _jackPosition;
    

    [Header("VFX Stuff")]
    [SerializeField] private ParticleSystem _connectSpark;
    [SerializeField] private ParticleSystem _disconnectedSparks;

    [HideInInspector] public MGWire ConnectedWire;
    
    /// <summary>
    /// Setting color of wire slot
    /// </summary>
    private void Start()
    {
        //Adds To Slot List in Wire State Script
        GameObject robo = GameObject.FindWithTag("WireManager");
        robo.GetComponent<MGWireState>().ListOfWireSlots.Add(this);
    }

    /// <summary>
    /// Checks to see if the wire plaaced in the slot was the corrent wire
    /// </summary>
    /// <param name="wire"></param>
    public bool CheckWire(MGWire wire)
    {
        Assert.IsNotNull(wire, "Make sure the object passed in is a " +
            "valid wire");
        
        ConnectedWire = wire;

        if(wire.WireID == _matchingWire)
        {
            IsCorrectWire = true;
            CorrectWire?.Invoke();
            _disconnectedSparks.Stop();
            _connectSpark.Play();
            return true;
        }

        return false;
    }

    public void ConnectJackToSlot(GameObject jack)
    {
        PrimeTween.Tween.Position(jack.transform, endValue: _jackPosition.transform.position, duration: 1, PrimeTween.Ease.InOutSine);
        PrimeTween.Tween.Rotation(jack.transform, endValue: _jackPosition.transform.rotation, duration: 1, PrimeTween.Ease.InOutSine);
    }

    public void RemoveWire()
    {
        IsCorrectWire = false;
        IsConnected = false;
        
        Debug.Log(ConnectedWire.name + " removed");
        ConnectedWire = null;

        if (!_disconnectedSparks.isPlaying)
        {
            _disconnectedSparks.Play();
        }
    }
}
