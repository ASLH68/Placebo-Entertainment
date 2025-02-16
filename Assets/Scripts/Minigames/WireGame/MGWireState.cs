/*****************************************************************************
// File Name :         MGWireState.cs
// Author :            Andrea Swihart-DeCoster
// Creation Date :     05/21/24
//
// Brief Description : Controls the state of the wire minigame.
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGWireState : MonoBehaviour
{
    public static Action WireGameWon;
    [SerializeField] private NpcEvent _gameWonEvent;

    [SerializeField] private int _maxAttachments; // = 3;
    private int _currentAttachments = 0;

    public List<MGWireSlot> ListOfWireSlots;

    [SerializeField] private Renderer screenToChange;
    [SerializeField] private Material fixedScreenMaterial;

    private void OnEnable()
    {
        MGWireSlot.CorrectWire += AttachedWire;
    }

    private void OnDisable()
    {
        MGWireSlot.CorrectWire -= AttachedWire;
    }

    /// <summary>
    /// Called when a wire was placed in the correct slot. Checks to see if
    /// the end conditions for winning have been met.
    /// </summary>
    private void AttachedWire()
    {
        //counts how many wires are connected
        foreach(MGWireSlot wireSlot in ListOfWireSlots)
        {
            if(wireSlot.IsConnected)
            {
                ++_currentAttachments;
            }
        }
        
        //checks if enough wires are connected to end the game
        if (_currentAttachments >= _maxAttachments)
        {
            EndWireGame();
        }

        //resets count
        _currentAttachments = 0;
    }

    /// <summary>
    /// Called when the wire game has been successfully won.
    /// </summary>
    private void EndWireGame()
    {
        print("Wire game won");
        screenToChange.material = fixedScreenMaterial;
        _gameWonEvent.TriggerEvent(NpcEventTags.Robot);
        WireGameWon?.Invoke();
    }
}
