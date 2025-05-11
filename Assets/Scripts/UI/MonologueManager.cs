/******************************************************************
*    Author: Nick Grinstead
*    Contributors: 
*    Date Created: 7/17/24
*    Description: This script will hold all of the player's internal monologues.
*       It will be attached to an object with an event listener that can trigger
*       specific monologues.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlaceboEntertainment.UI;
using Unity.VisualScripting;

public class MonologueManager : MonoBehaviour
{
    /// <summary>
    /// Holds an items description and the "leave" player option for it
    /// </summary>
    [System.Serializable]
    protected struct MonologueNode
    {
        [SerializeField] private string _monologueLabel;
        [SerializeField] private string _monologueText;
        [SerializeField] private string _exitResponse;
        [SerializeField] private NpcEvent _eventToTrigger;
        [SerializeField] private NpcEventTags _eventTag;

        public string MonologueText{ get => _monologueText; }
        public string ExitResponse { get => _exitResponse; }
        public NpcEvent EventToTrigger { get => _eventToTrigger; }
        public NpcEventTags EventTag { get => _eventTag; }
    }

    [SerializeField] private string _playerName;
    [SerializeField] private MonologueNode[] _monologueNodes;
    private MonologueNode _currentNode;

    private TabbedMenu _tabbedMenu;
    private PlayerController _playerController;
    private Interact _playerInteractBehavior;

    private void Start()
    {
        _tabbedMenu = TabbedMenu.Instance;
        _playerController = PlayerController.Instance;
        _playerInteractBehavior = _playerController.GetComponent<Interact>();

        // Triggering start of game monologue
        TriggerMonologue(0);
    }

    /// <summary>
    /// Invoked by events to display a specific player monologue
    /// </summary>
    /// <param name="monologueIndex"></param>
    public void TriggerMonologue(int monologueIndex)
    {
        if (monologueIndex >= 0 && monologueIndex < _monologueNodes.Length)
        {
            //For the timers to stop when monologuing
            foreach (TimerStruct timerStruct in TimerManager.Instance._timers.ToList())
            {
                timerStruct.timer.StopTimer();
            }
            _currentNode = _monologueNodes[monologueIndex];
            _playerController.LockCharacter(true);
            _playerInteractBehavior.StopDetectingInteractions();
            _tabbedMenu.DisplayDialogue(_playerName, _currentNode.MonologueText, null);
            _tabbedMenu.ToggleDialogue(true);
            _tabbedMenu.ClearDialogueOptions();
            _tabbedMenu.DisplayDialogueOption(_currentNode.ExitResponse, click: () => { ExitMonologue(); });
        }
    }

    /// <summary>
    /// Invoked by dialogue button to stop showing the current monologue
    /// </summary>
    public void ExitMonologue()
    {
        //For the timers to start when monologuing has stopped
        foreach (TimerStruct timerStruct in TimerManager.Instance._timers.ToList())
        {
            timerStruct.timer.StartTimer();
        }
        
        //Put achievement "Find what is beyond the marion" here
        
        _tabbedMenu.ToggleDialogue(false);
        _playerController.LockCharacter(false);
        _playerInteractBehavior.StartDetectingInteractions();

        if (!_currentNode.Equals(default(MonologueNode)) && !_currentNode.EventToTrigger.IsUnityNull())
        { 
            _currentNode.EventToTrigger.TriggerEvent(_currentNode.EventTag);
        }

        _currentNode = default(MonologueNode);
    }
}
