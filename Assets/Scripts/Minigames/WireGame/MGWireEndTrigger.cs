/*****************************************************************************
// File Name :         MGWireEndTrigger.cs
// Author :            Andrea Swihart-DeCoster
// Creation Date :     05/23/24
//
// Brief Description : Controls the triggers on the end of the wire.
*****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGWireEndTrigger : MonoBehaviour
{
    private MGWire wire;
    private void Start()
    {
        wire = GetComponentInParent<MGWire>();
    }
    
    /// <summary>
    /// Calls the wire trigger enter functionality if a wire interacts
    /// </summary>
    /// <param name="other">other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        MGWireSlot slot = other.GetComponent<MGWireSlot>();
        if (wire && slot && !slot.IsConnected)
        {
            wire.EndTriggerEnter(slot);
        }
    }

    /// <summary>
    /// Calls the wire trigger exit functionality if a wire exits the trigger
    /// </summary>
    /// <param name="other">other collider</param>
    private void OnTriggerExit(Collider other)
    {
        MGWireSlot slot;

        if (wire && other.TryGetComponent<MGWireSlot>(out slot))
        {
            if (slot.ConnectedWire && slot.ConnectedWire.Equals(wire))
            {
                Debug.Log("End trigger " + wire.WireID);
                wire.EndTriggerExit();
            }
        }
    }
}
