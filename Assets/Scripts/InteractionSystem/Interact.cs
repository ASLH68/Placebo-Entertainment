/******************************************************************
*    Author: Marissa Moser
*    Contributors: 
*    Date Created: May 24, 2024
*    Description: 
*       Contains a function called when the player presses E. This function will:
            Use a raycast to check if there is something in from of them
            to interact with and if it is interactable. If so, call the
            Interact(player) function on that game object

*******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using PlaceboEntertainment.UI;

public class Interact : MonoBehaviour
{
    PlayerControls _playerControls;
    private InputAction _interact;
    private Camera _camera;

    [SerializeField] private GameObject _targetGameObj;
    private IInteractable _interactable;
    private bool _canInteract;

    //raycast variables
    private RaycastHit _colliderHit;
    [SerializeField] private float _maxInteractDistance;
    [SerializeField] LayerMask _layerToIgnore;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.BasicControls.Enable();
        _interact = _playerControls.FindAction("Interact");
        _interact.started += InteractPressed;
        _interact.canceled += InteractReleased;

        _camera = Camera.main;

        StartDetectingInteractions();
    }

    /// <summary>
    /// Called when Interact input is started. Calls Interact() on the detected
    /// interactable game object
    /// </summary>
    private void InteractPressed(InputAction.CallbackContext ctx)
    {
        if(_interactable != null && !TabbedMenu.Instance.DialogueVisible)
        {
            _interactable.Interact(gameObject);
        }
    }

    /// <summary>
    /// Starts the Detect Interactable coroutine
    /// </summary>
    public void StartDetectingInteractions()
    {
        _canInteract = true;
        StartCoroutine(DetectInteractable());
    }

    /// <summary>
    /// Ends the Detect Interactable coroutine
    /// </summary>
    public void StopDetectingInteractions()
    {
        _canInteract = false;
    }

    /// <summary>
    /// A coroutine that detects if there is an interactable object in front of
    /// the player using a raycast. This coroutine can be stopped with the public 
    /// Start/StopDetectingInteraction function
    /// </summary>
    /// <returns></returns>
    private IEnumerator DetectInteractable()
    {
        while(_canInteract)
        {
            //Casts Raycast in the center of the screen
            Ray r = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(r, out _colliderHit, _maxInteractDistance, ~_layerToIgnore))
            {
                _targetGameObj = _colliderHit.transform.gameObject;

                //sets the _interactable variable for the InteractPressed function
                if (_targetGameObj.TryGetComponent(out IInteractable interactable))
                {
                    _interactable = _targetGameObj.GetComponent<IInteractable>();
                    _interactable.DisplayInteractUI();
                }
                else if (_interactable != null)
                {
                    _interactable.HideInteractUI();
                   _interactable = null;
                }
            }
            //resets the variables if the player backs away from interactable
            else if (_interactable != null)
            {
                _targetGameObj = null;
                _interactable.HideInteractUI();
                _interactable = null;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Called when Interact input is canceled. Calls CancelInteract() on the
    /// detected interactable game object.
    /// </summary>
    /// <param name="obj"></param>
    private void InteractReleased(InputAction.CallbackContext obj)
    {
        if (_interactable != null)
        {
            _interactable.CancelInteract();
        }
    }

    private void OnDisable()
    {
        _interact.started -= InteractPressed;
        _interact.canceled -= InteractReleased;
    }
}
