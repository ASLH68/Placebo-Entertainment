/******************************************************************
*    Author: Marissa Moser
*    Contributors: 
*    Date Created: May 22, 2024
*    Description: Contains the functionality for the door when it is interacted with.

*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlaceboEntertainment.UI;

public class DoorBehavior : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactPromptText = "DOOR";
    [SerializeField] private string _lockedInteractPromptText = "LOCKED DOOR";

    private bool _isOpened = false;
    [SerializeField] private bool _isLocked;
    private Animator _anim;

    [SerializeField] private Material _material;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private FMODUnity.EventReference doorEvent;
    private Color _color;
    private float _intensity;

    [Header("Fuse Hotfix")] 
    [SerializeField] private NpcEvent _fuseEndOfGame;
    [SerializeField] private bool _inFuse;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _material = _renderer.GetComponent<Renderer>().material;
    }

    /// <summary>
    /// Called from player's Interact script. Contains functionality for when
    /// a door is interacted with. Can be updated to also unlock the door.
    /// </summary>
    /// <param name="player"></param>
    public void Interact(GameObject player)
    {
        if (!_isLocked)
        {
            OpenDoor();
            if (_inFuse)
            {
                _fuseEndOfGame.TriggerEvent(NpcEventTags.Game);
            }
        }
    }

    /// <summary>
    /// Displays the specific UI prompt if the door is locked or unlocked
    /// </summary>
    public void DisplayInteractUI()
    {
        if(!_isLocked && !_isOpened)
        {
            TabbedMenu.Instance.ToggleInteractPrompt(true, _interactPromptText);
        }
        else if(!_isOpened)
        {
            TabbedMenu.Instance.ToggleInteractPrompt(true, _lockedInteractPromptText);
        }

    }

    /// <summary>
    /// Hides the specific UI prompt
    /// </summary>
    public void HideInteractUI()
    {
        TabbedMenu.Instance.ToggleInteractPrompt(false);
    }

    /// <summary>
    /// Contains the functionality to open a door
    /// </summary>
    private void OpenDoor()
    {
        _isOpened = true;
        _anim.SetTrigger("_openDoor");
        AudioManager.PlaySound(doorEvent, transform.position, null);
        GetComponent<BoxCollider>().enabled = false;
        //disabe UI - should I just remove the interactable script from the door?
        HideInteractUI();
    }

    /// <summary>
    /// A public function that can be called from the NPC that will unlock the
    /// door. Can be altered to also open the door when this happens.
    /// </summary>
    public void UnlockDoor()
    {
        _isLocked = false;
        ChangeColor("Blue");

        //if unlocking door should open the door:
        //OpenDoor();
    }

    public void ChangeColor(string myColor)
    {
        switch(myColor)
        {
            case "Red": 
                _color = Color.red;
                _intensity = 2.0f;
                _material.SetColor("_EmissionColor", _color * _intensity);
                break;

            case "Blue":
                _color = Color.cyan;
                _intensity = 2.0f;
                _material.SetColor("_EmissionColor", _color * _intensity);
                break;
        }
    }

}
