/*****************************************************************************
// File Name :         MGWire.cs
// Author :            Andrea Swihart-DeCoster
// Contributor :       Nick Grinstead
// Creation Date :     05/21/24
//
// Brief Description : Contains the logic and properties for the wire itself
                       and any relevant gameplay logic for how the wire works.
*****************************************************************************/

using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
using PlaceboEntertainment.UI;
using UnityEngine.Rendering.Universal;

public class MGWire : MonoBehaviour
{
    public EWireID WireID;
    [SerializeField] private Color _wireColor;
    [SerializeField] private Material _blueJack;
    [SerializeField] private Material _redJack;
    [SerializeField] private Material _greenJack;
    [SerializeField] private Material _blackJack;

    [SerializeField] private string _interactPromptText = "MOVE";

    [SerializeField] private Transform _wireStartPosition;
    [SerializeField] private Transform _wireEndPosition;
    [SerializeField] private float _distanceFromPlayer;
    [SerializeField] private float _maxLength;
    [SerializeField] private FMODUnity.EventReference wireGrabEvent;
    [SerializeField] private FMODUnity.EventReference wireConnectEvent;

    [SerializeField] private GameObject _avcCable;
    [SerializeField] private GameObject _avcJack;

    [SerializeField] private GameObject _wireJack;

    [SerializeField] float _totalWeight = 10f;

    [SerializeField] float _drag = 1f;
    [SerializeField] float _angularDrag = 1f;

    [SerializeField] bool _usePhysics = false;

    private bool _canConnectToSlot = false;

    private MGWireSlot _currentSlot = null;
    private bool _isCorrectlySlotted = false;

    private bool _isInteracting = false;
    private bool _canInteract = false;
    private Transform _cameraTrans;
    private TabbedMenu _tabbedMenu;
    private bool _minigameStarted = false;

    private Rigidbody _jackRb;

    public enum EWireID
    {
        ONE, TWO, THREE, FOUR
    }

    private void Start()
    {
        if (_wireJack != null)
            _wireJack.GetComponent<Renderer>().material = GetJackColor();

        _tabbedMenu = TabbedMenu.Instance;
        _cameraTrans = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();

        _jackRb = _wireJack.GetComponent<Rigidbody>();
        _jackRb.isKinematic = false;
        _jackRb.mass = _totalWeight;
        _jackRb.drag = _drag;
        _jackRb.angularDrag = _angularDrag;
    }
    
    /// <summary>
    /// Make wire end move to a position in front of the camera if the player
    /// is interacting with this wire
    /// </summary>
    private void FixedUpdate()
    {
        if (_isInteracting)
        {
            Vector3 target = _cameraTrans.position + _cameraTrans.forward * _distanceFromPlayer;
            _wireEndPosition.position = target;
        }
    }

    /// <summary>
    /// Invoked by start minigame event triggered by Robot to enable wire interactions
    /// </summary>
    public void StartMinigame()
    {
        _minigameStarted = true;
        _canInteract = true;
        MGWireState.WireGameWon += EndMinigame;
    }

    /// <summary>
    /// Invoked by minigame won event to disable wire interactions
    /// </summary>
    private void EndMinigame()
    {
        _minigameStarted = false;
        _canInteract = false;
        MGWireState.WireGameWon -= EndMinigame;
    }

    /// <summary>
    /// Called toggle interacting with this wire
    /// </summary>
    /// <param name="player">Player interacting with the wire</param>
    public void Interact()
    {
        // Can't interact with wires unless the minigame has begun
        if (_minigameStarted && _canInteract)
        {
            AudioManager.PlaySound(wireGrabEvent, transform.position);
            
            if (!_isInteracting)
            {
                OnInteract();
            }
            else
            {
                OnDrop();
            }
        }
    }

    /// <summary>
    /// Called when the player interacts with the wire. Changes the wire
    /// to be kinematic so the player has direct control over it.
    /// </summary>
    private void OnInteract()
    {
        _isInteracting = true;
        _jackRb.isKinematic = true;
        _jackRb.freezeRotation = true;

        if (_currentSlot)
        {
            StartCoroutine(RotateJack());
        }
    }

    /// <summary>
    /// Rotates the jack when the player picks it up
    /// </summary>
    /// <returns>Waits a fraction of a second</returns>
    IEnumerator RotateJack()
    {
        float lerpValue = 0;
        float newRotation;

        while (lerpValue < 1)
        {
            newRotation = Mathf.Lerp(transform.rotation.z, 90f, lerpValue);
            _jackRb.transform.eulerAngles = new Vector3(0, 0, newRotation);
            lerpValue += 0.01f;

            yield return new WaitForSeconds(0.01f);
        }
    }

    /// <summary>
    /// When the player lets go of the wire, kinematic is turned on and
    /// attempts to place the wire in a slot
    /// </summary>
    private void OnDrop()
    {
        _isInteracting = false;

        _jackRb.isKinematic = false;
        _jackRb.freezeRotation = false;

    }

    /// <summary>
    /// Called when the trigger on the end of the wire is entered.
    /// Enables connection with the slot
    /// </summary>
    /// <param name="slot"></param>
    public void EndTriggerEnter(MGWireSlot slot)
    {
        if (slot && !slot.ConnectedWire)
        {
            _canConnectToSlot = true;
            _currentSlot = slot;
            PlaceWire(slot);
        }
    }
    
    /// <summary>
    /// Called when the player moves the end of the wire outside of a slots 
    /// trigger
    /// </summary>
    public void EndTriggerExit()
    {
        if (_currentSlot && _currentSlot.ConnectedWire && _currentSlot.ConnectedWire.Equals(this))
        {
            _currentSlot.RemoveWire();
        }
        _canConnectToSlot = false;
        _currentSlot = null;
    }

    /// <summary>
    /// Called after the player drops the wire. This attempts to connect it
    /// to a slot, otherwise kinematics are disabled and it responds to
    /// physics.
    /// </summary>
    private void PlaceWire(MGWireSlot slot)
    {
        if (slot && _canConnectToSlot && _currentSlot && !_currentSlot.IsConnected)
        {
            _isInteracting = false;
            _jackRb.isKinematic = true;
            StopAllCoroutines();

            _currentSlot.ConnectJackToSlot(_wireJack);

            _currentSlot.IsConnected = true;
            _isCorrectlySlotted = _currentSlot.CheckWire(this);

            // Prevents the moving of wires that are already in the right place
            if (_isCorrectlySlotted)
            {
                AudioManager.PlaySound(wireConnectEvent, transform.position);
            }
        }
        else if (!_canConnectToSlot)
        {
            _jackRb.isKinematic = false;
            _jackRb.freezeRotation = false;
        }
    }

    /// <summary>
    /// Returns the color of the jack. 
    /// Each jack color correlates to where it will be slotted into.
    /// <summary>
    private Material GetJackColor()
    {
        Material mat;
        switch (WireID)
        {
            case EWireID.ONE:

                mat = _blueJack;
                break;
            case EWireID.TWO:
                mat = _greenJack;
                break;
            case EWireID.THREE:
                mat = _redJack;
                break;
            default:
                mat = _blackJack;
                break;
        }

        return mat;
    }
    
    /// <summary>
    /// For interaction system to toggle interact prompt on
    /// </summary>
    public void DisplayInteractUI()
    {
        if (_tabbedMenu != null && _canInteract)
        {
            _tabbedMenu.ToggleInteractPrompt(true, _interactPromptText);
        }
    }

    /// <summary>
    /// For interaction system to toggle interact prompt off
    /// </summary>
    public void HideInteractUI()
    {
        if (_tabbedMenu != null)
        {
            _tabbedMenu.ToggleInteractPrompt(false);
        }
    }
}