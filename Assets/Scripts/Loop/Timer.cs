/******************************************************************
*    Author: Elijah Vroman
*    Contributors: Elijah Vroman
*    Date Created: 5/29/24
*    Description: Just a timer class with some helper methods. The time
*    manager script does the creation, running, and deletion of these
*******************************************************************/
using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Timer
{
    [SerializeField] public float _maxTime;
    [SerializeField] private bool _isRunning;
    public float _timeRemaining { get; private set; }

    [Header("Events")]
    [SerializeField] private NpcEvent _eventTimerCalls;
    [SerializeField] private NpcEventTags _NPCToAlert;

    public Timer(float maxTime, NpcEvent eventTimerCalls, NpcEventTags npcToAlert)
    {
        _maxTime = maxTime;
        _timeRemaining = _maxTime;
        _eventTimerCalls = eventTimerCalls;
        _NPCToAlert = npcToAlert;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (_isRunning)
        {
            _timeRemaining -= deltaTime;
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                _isRunning = false;
                if (!_eventTimerCalls.IsUnityNull())
                {
                    _eventTimerCalls.TriggerEvent(_NPCToAlert);
                }
                else
                {
                    Debug.Log("No event called; is null.");
                }
            }
        }
    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }

    public float GetCurrentTimeInSeconds()
    {
        return _timeRemaining;
    }

    public NpcEventTags GetTimerTag()
    { 
        return _NPCToAlert; 
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public void ResetTimer()
    {
        _timeRemaining = _maxTime;
    }

    public void IncreaseTime(int minutes, float seconds)
    {
        _timeRemaining = Mathf.Clamp(_timeRemaining + (minutes * 60) + seconds, 0, _maxTime);
    }

    public void ReduceTime(int minutes, float seconds)
    {
        _timeRemaining = Mathf.Clamp(_timeRemaining - (minutes * 60) + seconds, 0, _maxTime);
    }
}
