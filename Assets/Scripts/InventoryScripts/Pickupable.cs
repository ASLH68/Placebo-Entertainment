/******************************************************************
*    Author: Elijah Vroman
*    Contributors: Elijah Vroman, Nick Grinstead, Marissa Moser
*    Date Created: 5/21/24
*    Description: PUT THIS ON ANY GAMEOBJECT TO BE INVENTORIED
*    If what this script is attached to is hit by an inventory 
*    system, it is added to that inventory.
*******************************************************************/

using FMODUnity;
using PlaceboEntertainment.UI;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Pickupable : MonoBehaviour, IInteractable
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

    [SerializeField] private DescriptionNode _itemDescription;

    [SerializeField] private float PickUpRadius;
    [SerializeField] private InventoryItemData myData;
    private SphereCollider myCollider;
    private TabbedMenu _tabbedMenu;
    private PlayerController _playerController;
    private Interact _playerInteractBehavior;
    private bool _isInteractive;
    [SerializeField] private bool _IsInteractiveOnStart;
    [SerializeField] private FMODUnity.EventReference pickupEvent;

    private void Awake()
    {
        myCollider = GetComponent<SphereCollider>();
        myCollider.isTrigger = false;
        //Changed above to false so that the player can run into the gameobject

        if (myData == null)
        {
            Debug.Log("An item in the scene is missing a scriptable object");
        }
        if (PickUpRadius <= 0)
        {
            PickUpRadius = myCollider.radius;
        }
    }

    private void Start()
    {
        _tabbedMenu = TabbedMenu.Instance;
        _playerController = PlayerController.Instance;
        _playerInteractBehavior = _playerController.GetComponent<Interact>();

        //On a delay because player inventory is not always loaded before this script
        Invoke("ValidateInWorld", 5);

        if (_IsInteractiveOnStart)
        {
            MakeInteractive();
        }
    }

    /// <summary>
    /// removes this object from the world if it is already in the players inventory.
    /// Should only be used for the translation book. 
    /// </summary>
    private void ValidateInWorld()
    {
        GameObject player = _playerController.gameObject;
        InventoryHolder inventoryHolder = player.GetComponent<InventoryHolder>();
        if (inventoryHolder.InventorySystem.ContainsItem(myData, out _))
        {
            Destroy(gameObject);
        }
    }

    public void DisplayInteractUI()
    {
        if (_isInteractive)
        {
            _tabbedMenu.ToggleInteractPrompt(true, "The " + myData.DisplayName);
        }
    }

    public void HideInteractUI()
    {
        if (_isInteractive)
        {
            _tabbedMenu.ToggleInteractPrompt(false);
        }
    }

    /// <summary>
    /// This function is called by the event system to make the pickupable item
    /// interactive. 
    /// </summary>
    public void MakeInteractive()
    {
        _isInteractive = true;
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
    /// Shows item description before adding item to inventory
    /// </summary>
    /// <param name="player">The player interacting</param>
    public void Interact(GameObject player)
    {
        if (this == null) { return; }

        if(_isInteractive)
        {
            AudioManager.PlaySoundUnManaged(pickupEvent, gameObject);
            _playerController.LockCharacter(true);
            _playerInteractBehavior.StopDetectingInteractions();
            _tabbedMenu.DisplayDialogue("", _itemDescription.Description, null);
            _tabbedMenu.ToggleDialogue(true);
            _tabbedMenu.ClearDialogueOptions();
            _tabbedMenu.DisplayDialogueOption(_itemDescription.ExitResponse, click: () => { CloseItemDescription(); });

            InventoryHolder inventoryHolder = player.GetComponent<InventoryHolder>();

            
            if (inventoryHolder != null)
            {
                //if item is not already in inventory, add the item to the inventory
                if (!inventoryHolder.InventorySystem.ContainsItem(myData, out _))
                {
                    inventoryHolder.InventorySystem.AddToInventory(myData, 1, out _);
                }
                
                PlayerController.Animator.SetTrigger("Interact");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("InventoryHolder component not found on player.");
            }
        }
    }
    /* COMMENTED THIS SO ITEMS MUST BE INTERACTED WITH. KEEPING JUST IN CASE
    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.transform.GetComponent<InventoryHolder>();
        if (inventory.InventorySystem.AddToInventory(myData, 1, out _))
        {
            //IFF successful, destroy gameobject

            Destroy(gameObject);
        }
    }*/
}
