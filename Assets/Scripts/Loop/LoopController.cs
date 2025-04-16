/******************************************************************
*    Author: Elijah Vroman
*    Contributors: Elijah Vroman, Alec Pizziferro
*    Date Created: 5/30/24?
*    Description: This monobehavior will be present in the scene to 
*    control when the scene resets. 
*******************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LoopController : MonoBehaviour
{
    //private Timer _loopTimer;
    //[SerializeField] private string _loopTimerName;
    //[SerializeField] private int _loopTimerTime;
    //[SerializeField] private int _endScreenDelay;
    //[SerializeField] private NpcEvent _temporaryLoop;
    //[SerializeField] private NpcEventTags _temporaryTag;
    //public int LoopTimerTimer => _loopTimerTime;
    //public string LoopTimerName => _loopTimerName;

    private List<Timer> _runningTimersAtStart = new List<Timer>();
    private List<Timer> _pausedTimersAtStart= new List<Timer>();

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
        
        if(_looped == true)
        {
            PlayerController.Animator.SetTrigger("Reset");
        }
    }
    /// <summary>
    /// Handler for the event
    /// </summary>
    private void HandleLoopTimerEnd()
    {
        //TimerManager.Instance.RemoveTimer(LoopTimerName);

        ResetLoop();

        //_loopTimer.TimesUp -= HandleLoopTimerEnd;
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
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
        _looped = true;
        
        //Place achievement "Die" here
        SteamAchievements.Instance.UnlockSteamAchievement("DIE");
    }

    /// <summary>
    /// Called to load the game
    /// </summary>
    private void LoadSave()
    {
        SaveLoadManager.Instance.LoadGameFromSaveFile();
    }
}
