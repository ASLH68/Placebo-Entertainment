/******************************************************************
*    Author: Nick Grinstead
*    Contributors: Elijah Vroman
*    Date Created: 6/9/24
*    Description: NPC class containing logic for the Angel NPC.
*                 Currently set up for the first playable, will need to be updated
*                 for future milestones with more features.
*******************************************************************/

using PlaceboEntertainment.UI;
using Unity.VisualScripting;
using UnityEngine;

public class AngelNpc : BaseNpc
{
    [SerializeField] private NpcEvent _removeTimerEvent;

    private bool _robotGameComplete = false;
    private bool _cowardGameComplete = false;
    private bool _isPostMinigameState = false;

    public override void CheckForStateChange()
    {
        if (_currentState == NpcStates.DefaultIdle)
        {
            EnterMinigameReady();
        }
        else if (_currentState == NpcStates.PlayingMinigame)
        {
            EnterPostMinigame();
        }
    }

    protected override void EnterPostMinigame()
    {
        base.EnterPostMinigame();

        //Interact();

        _removeTimerEvent.TriggerEvent(NpcEventTags.Angel);

        _isPostMinigameState = true;
    }

    /// <summary>
    /// Temporary wingame as of 6/26
    /// </summary>
    public void WinGame()
    {
        if (TabbedMenu.TimeSpanSinceStart.TotalSeconds <= 60f * 8 && !SteamAchievements.Instance.IsUnityNull())
        {
            SteamAchievements.Instance.UnlockSteamAchievement("SPEED_RUN");
        }
        
        _tabbedMenu.ToggleWin(true);
    }

    /// <summary>
    /// Chooses different dialogue based on if the player has done both minigames
    /// or not
    /// </summary>
    /// <param name="node">Dialogue node being examined</param>
    /// <returns>String dialogue to display</returns>
    protected override string ChooseDialogueFromNode(DialogueNode node)
    {
        PlayRandomTalkingAnim(node);
        if (_cowardGameComplete && _robotGameComplete)
        {
            return node.Dialogue[1];
        }
        else
        {
            return base.ChooseDialogueFromNode(node);
        }
    }

    protected override int ChooseDialoguePath(PlayerResponse option)
    {
        if (_isPostMinigameState && option.NextResponseIndex.Length == 1)
        {
            _animator.SetTrigger("FinalChoice");
            return 0;
        }
        if (_hasBypassItem)
        {
            _animator.SetTrigger("Healed");
            return option.NextResponseIndex[1];
        }
        else if(!_hasBypassItem && option.NextResponseIndex.Length > 0)
        {
            return option.NextResponseIndex[0];
        }
        // Checks for bypass
        else if (_hasBypassItem && _currentState != NpcStates.PostMinigame)
        {
            _shouldEndDialogue = true;
            Invoke(nameof(EnterPostMinigame), 0.2f);
            return 0;
        }
        // Don't have minigame bypass
        else
        {
            return base.ChooseDialoguePath(option);
        }
    }

    public override void Interact(int responseIndex = 0)
    {
        // Overwrote this to add special functionality to play an anim a the 2nd to last dialogue node.
        base.Interact(responseIndex);

        if (_isPostMinigameState && _currentDialogueIndex == _stateDialogueTrees.GetStateData(_currentState).Length - 1)
        {
            _animator.SetTrigger("FinalChoice");
        }
    }
}
