/******************************************************************
*    Author: Marissa Moser
*    Contributors: 
*    Date Created: June 18, 2024
*    Description: Contains the base functionality for levers. Each station that
*    requires a lever can use a script that derives from this. Levers start in the
*    down/false state.

*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlaceboEntertainment.UI;

public class LeverInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactPromptText = "LEVER";
    [SerializeField] private FMODUnity.EventReference leverEvent;
    [SerializeField] private GameObject _handle;
    public bool _isOn { get; private set; }

    /// <summary>
    /// Flips the lever when interacted with.
    /// </summary>
    /// <param name="player"></param>
    public virtual void Interact(GameObject player)
    {
        //print("interact");
        if (!_isOn)
        {
            SetLever(true);
        }
        else
        {
            SetLever(false);
        }
    }

    /// <summary>
    ///  Displays the specific UI prompt
    /// </summary>
    public virtual void DisplayInteractUI()
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
    /// Sets the lever to the state that is passed in as the parameter.
    /// </summary>
    /// <param name="input"></param>
    public virtual void SetLever(bool input)
    {
        AudioManager.PlaySound(leverEvent, transform.position);
        //set to true if not already
        if(input && !_isOn)
        {
            _handle.transform.Rotate(-120, 0, 0, Space.Self);
            _isOn = true;
        }
        //set to false if not already
        else if(!input && _isOn)
        {
            _handle.transform.Rotate(120, 0, 0, Space.Self);
            _isOn = false;
        }
    }
}
