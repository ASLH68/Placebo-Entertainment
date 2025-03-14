/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: June 24, 2024
*    Description: Contains the functionality for dials. Dials start in
*    the Up position and rotate 90 degrees clockwise when interacted with.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlaceboEntertainment.UI;

public class DialInteraction : MonoBehaviour, IInteractable
{
    public enum DialDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public DialDirection _direction { get; private set; }

    [SerializeField] private string _interactPromptText = "DIAL";
    [SerializeField] private FMODUnity.EventReference dialEvent;

    /// <summary>
    /// Sets dial direction
    /// </summary>
    private void Awake()
    {
        _direction = DialDirection.Up;
    }

    /// <summary>
    /// Rotates the dial when interacted with.
    /// </summary>
    /// <param name="player"></param>
    public virtual void Interact(GameObject player)
    {
        int newDirection = (int) _direction + 1;
        newDirection %= 4;
        _direction = (DialDirection) newDirection;

        transform.Rotate(new Vector3(-90, 0, 0));
        AudioManager.PlaySound(dialEvent, transform.position);
    }

    /// <summary>
    ///  Displays the specific UI prompt
    /// </summary>
    public void DisplayInteractUI()
    {
        TabbedMenu.Instance.ToggleInteractPrompt(true, _interactPromptText);
    }

    /// <summary>
    /// Hides the specific UI prompt
    /// </summary>
    public void HideInteractUI()
    {
        TabbedMenu.Instance.ToggleInteractPrompt(false);
    }

    /// <summary>
    /// Returns the dial to its up position
    /// </summary>
    public void ResetDial()
    {
        if (_direction != DialDirection.Up)
        {
            int difference = 4 - (int) _direction;
            transform.Rotate(new Vector3(-90 * difference, 0, 0));
            _direction = DialDirection.Up;
        }
    }
}
