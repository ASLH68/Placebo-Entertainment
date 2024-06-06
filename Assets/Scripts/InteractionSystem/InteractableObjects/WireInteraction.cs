/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 5/29/24
*    Description: Wrapper for MGWire's interactions. Attaches to EndPos
*    so the interactable script can move with wire's only collider.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireInteraction : MonoBehaviour, IInteractable
{
    private MGWire _wire;

    /// <summary>
    /// Assigning MGWire reference
    /// </summary>
    private void Awake()
    {
        _wire = GetComponentInParent<MGWire>();
    }

    public void DisplayInteractUI()
    {
        _wire.DisplayInteractUI();
    }

    public void HideInteractUI()
    {
        _wire.HideInteractUI();
    }

    public void Interact(GameObject player)
    {
        _wire.Interact();
    }
}
