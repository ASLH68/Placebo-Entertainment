/*****************************************************************************
// File Name :         WrenchBehavior.cs
// Author :            Mark Hanson
// Contributors :      Marissa Moser, Nick Grinstead
// Creation Date :     5/27/2024
//
// Brief Description : Any function to do with the wrench will be found here. Wrench swinging, spark interaction, and completion of this segment of the minigame.
*****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PlaceboEntertainment.UI;
using System;

public class WrenchBehavior : MonoBehaviour, IInteractable
{
    /// <summary>
    /// Holds an items description and the "leave" player option for it
    /// </summary>
    [System.Serializable]
    protected struct DescriptionNode
    {
        [SerializeField] private string _description;
        [SerializeField] private string _exitResponse;
        [SerializeField] private NpcEvent _eventToTrigger;
        [SerializeField] private NpcEventTags _eventTag;

        public string Description { get => _description; }
        public string ExitResponse { get => _exitResponse; }
        public NpcEvent EventToTrigger { get => _eventToTrigger; }
        public NpcEventTags EventTag { get => _eventTag; }
    }

    [SerializeField] private NpcEvent _minigameEndEvent;

    [Header("UI Stuff")]
    [SerializeField] private DescriptionNode _itemDescription;
    [SerializeField] private string _interactPromptText = "WRENCH";

    [Header("Wrench overall functions")]
    [SerializeField] private GameObject _sparksMode;
    //private PlayerController _pc;
    private int _sparkSmacked;
    [SerializeField] private int _maxSpark;

    [Header("Wrench within hand functions")]
    [SerializeField] private Animator _animate;
    [SerializeField] private GameObject _wrenchSpark;
    //private bool _swing;
    [Header("Wrench outside hand functions")]
    [SerializeField] private GameObject _rightHand;

    [SerializeField] private FMODUnity.EventReference pickupEvent;
    private bool _withinProx;
    private bool _isEquipped;

    public static Action SparkSmackedAction;

    private TabbedMenu _tabbedMenu;
    private PlayerController _playerController;
    private Interact _playerInteractBehavior;

    void Awake()
    {
        _rightHand = GameObject.FindWithTag("Righty");
        _sparksMode = GameObject.Find("SparksMode");
        GameObject _smackTextObject = GameObject.Find("Spark num");
        GameObject _pc = GameObject.FindWithTag("RightArm");
        _animate = _pc.GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _tabbedMenu = TabbedMenu.Instance;
        _playerController = PlayerController.Instance;
        _playerInteractBehavior = _playerController.GetComponent<Interact>();
        _isEquipped = false;
        SparkSmackedAction += SparkSmacked;
    }

    void FixedUpdate()
    {
        if(_isEquipped == true)
        {
            transform.position = new Vector3(_rightHand.transform.position.x, _rightHand.transform.position.y, _rightHand.transform.position.z);
            transform.rotation = _rightHand.transform.rotation;
        }
    }

    /// <summary>
    /// A coroutine to manage the swinging animation.
    /// </summary>
    /// <returns></returns>
    IEnumerator Swinging()
    {
        GetComponent<Collider>().enabled = true;
        _animate.SetTrigger("_swing");

        yield return new WaitForSeconds(1f);

        GetComponent<Collider>().enabled = false;
    }
    IEnumerator SystematicShutDown()
    {
        yield return new WaitForSeconds(1.1f);
        gameObject.SetActive(false);
    }
    /// <summary>
    /// This function is invoked in SparkInteractBehavior whenever a spark is interacted
    /// with. It keeps track of the number of sparks that have been smacked and ends the game.
    /// </summary>
    private void SparkSmacked()
    {
        _sparkSmacked++;
        StartCoroutine(Swinging());

        if (_sparkSmacked >= _maxSpark)
        {
            _sparksMode.SetActive(false);
            StartCoroutine(SystematicShutDown());

            //game ends here?
            _minigameEndEvent.TriggerEvent(NpcEventTags.Coward);
            print("game end");
        }
    }

    /// <summary>
    /// Invoked by dialogue button to stop showing the item's descriptiond
    /// </summary>
    public void CloseItemDescription()
    {
        _tabbedMenu.ToggleDialogue(false);
        _playerController.LockCharacter(false);
        _playerInteractBehavior.StartDetectingInteractions();

        if (_itemDescription.EventToTrigger != null)
        {
            _itemDescription.EventToTrigger.TriggerEvent(_itemDescription.EventTag);
        }
    }

    /// <summary>
    /// This function is called when the player interacts with the wrench.
    /// </summary>
    /// <param name="player"></param>
    public void Interact(GameObject player)
    {
        _playerController.LockCharacter(true);
        _playerInteractBehavior.StopDetectingInteractions();
        _tabbedMenu.DisplayDialogue("", _itemDescription.Description, null);
        _tabbedMenu.ToggleDialogue(true);
        _tabbedMenu.ClearDialogueOptions();
        _tabbedMenu.DisplayDialogueOption(_itemDescription.ExitResponse, click: () => { CloseItemDescription(); });

        PickUpWrench();
    }

    /// <summary>
    /// Function to move the wrench object to the player's hand.
    /// </summary>
    public void PickUpWrench()
    {
        if (_isEquipped == false)
        {
            //_animate.SetTrigger("pickedUp");
            AudioManager.PlaySound(pickupEvent, transform.position);
            _isEquipped = true;
            GetComponent<Collider>().enabled = false;
            transform.position = _rightHand.transform.position;
            transform.rotation = _rightHand.transform.rotation;
            transform.parent = _rightHand.transform;
        }
    }

    /// <summary>
    /// Shows UI prompt for wrench
    /// </summary>
    public void DisplayInteractUI()
    {
        TabbedMenu.Instance.ToggleInteractPrompt(true, _interactPromptText);
    }

    /// <summary>
    /// Hides UI prompt for wrench
    /// </summary>
    public void HideInteractUI()
    {
        TabbedMenu.Instance.ToggleInteractPrompt(false);
    }

    private void OnDisable()
    {
        SparkSmackedAction -= SparkSmacked;
    }
}
