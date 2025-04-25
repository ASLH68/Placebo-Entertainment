/******************************************************************
*    Author: Elijah Vroman
*    Contributors: Elijah Vroman, Alec Pizziferro
*    Date Created: 5/30/24?
*    Description: This monobehavior will be present in the scene to 
*    control when the scene resets. 
*******************************************************************/

using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LoopController : MonoBehaviour
{
    private List<Timer> _runningTimersAtStart = new();
    private List<Timer> _pausedTimersAtStart= new();
    [SerializeField] private float _endScreenDelay = 3f;

    private static bool _looped = false;

    private void Start()
    {
        foreach(TimerStruct timer in TimerManager.Instance._timers)
        {
            if(timer.timer.IsRunning())
            {
                _runningTimersAtStart.Add(timer.timer);
            }
            else if (!timer.timer.IsRunning())
            {
                _pausedTimersAtStart.Add(timer.timer);  
            }
        }
        //_loopTimer = TimerManager.Instance.CreateTimer(LoopTimerName, _loopTimerTime + _endScreenDelay, _temporaryLoop, _temporaryTag);
        //_loopTimer.TimesUp += HandleLoopTimerEnd;
        LoadSave();
        
        if(_looped)
        {
            PlayerController.Animator.SetTrigger("Reset");
        }
    }
    /// <summary>
    /// Handler for the event
    /// </summary>
    private void HandleLoopTimerEnd()
    {
        ResetLoop();
    }
    /// <summary>
    /// Saving, loading the new scene, loading saved data
    /// </summary>
    public void ResetLoop()
    {
        foreach(Timer timer in  _runningTimersAtStart)
        {
            timer.ResetTimer();
            timer.StartTimer();
        }
        foreach (Timer timer in _pausedTimersAtStart)
        {
            timer.ResetTimer();
        }
        SaveLoadManager.Instance.SaveGameToSaveFile();
        StartCoroutine(DelayLoadOfScene());

        //Place achievement "Die" here
        if (!SteamAchievements.Instance.IsUnityNull())
        {
            SteamAchievements.Instance.UnlockSteamAchievement("DIE");
        }
    }

    IEnumerator DelayLoadOfScene()
    {
        yield return new WaitForSeconds(_endScreenDelay);
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
        _looped = true;
    }
    
    /// <summary>
    /// Called to load the game
    /// </summary>
    private void LoadSave()
    {
        SaveLoadManager.Instance.LoadGameFromSaveFile();
    }
}
