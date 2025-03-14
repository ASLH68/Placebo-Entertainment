/******************************************************************
*    Author: Marissa Moser
*    Contributors: Nick Grinstead
*    Date Created: June 19, 2024
*    Description: This script is the manager for the Angel Minigame. It keeps track
*    of starting and ending the game, the state of each station, switching between
*    stations, as well as counting the rounds and stations completed.
*******************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

[System.Serializable]
public struct Station
{
    public GameObject StationScreen;
    public ScreenBehavior ScreenBehavior;
    public GameObject StationConsole;
    public StationBehavior StationBehavior;
}

public class AngelMinigameManager : MonoBehaviour
{
    [SerializeField] private List<Station> _stations;
    [SerializeField] private GameObject _winScreen;

    [SerializeField] private Image[] _roundTrackers;
    [SerializeField] private Color _trackerDimmedColor = new Color(8, 137, 0, 100);
    [SerializeField] private Color _trackerHighlightedColor = new Color(8, 137, 0, 255);

    [SerializeField] private float _bridgeLayoutDisplayTime;
    [SerializeField] private GameObject _bridgeLayoutScreen;
    [SerializeField] private Image[] _layoutStations;
    [SerializeField] private Sprite _stationHighlightedSprite;
    [SerializeField] private Sprite _stationGreyedOutSprite;

    [SerializeField] int _countDownTime;
    private int _currentTime;

    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private NpcEvent _endMinigameEvent;
    [SerializeField] private NpcEventTags _endMinigameEventTag;

    //this will keep track of which station is active. -1 is pre-minigame, then
    //stations 1-4 are 0-3 (matched list of structs in inspector
    private int _stationCount;

    public static Action AngelStationComplete, CheckState, TriggerFail, TriggerStart;
    private int _round;

    /// <summary>
    /// Assigns values for play
    /// </summary>
    private void Start()
    {
        AngelStationComplete += SwitchStation;
        CheckState += CheckStates;
        TriggerFail += StartStation;

        _timerText.text = "";

        //find the scripts on all the screens to ref from the struct
        for (int i = 0; i < _stations.Count; i++)
        {
            var structCopy = _stations[i];
            structCopy.ScreenBehavior = _stations[i].StationScreen.GetComponent<ScreenBehavior>();
            structCopy.StationBehavior = _stations[i].StationConsole.GetComponent<StationBehavior>();
            _stations[i] = structCopy;
        }

        foreach (var VARIABLE in _stations)
        {
            Debug.Log(VARIABLE.StationBehavior);
        }
    }

    /// <summary>
    /// This function is called from the confirm button to check the state of the
    /// interactables at each station as the game progresses. It keeps track of which 
    /// round the player is on as well as which station.
    /// </summary>
    private void CheckStates()
    {
        bool state = _stations[_stationCount].StationBehavior.CheckStates();

        if (state)
        {
            _round++;
            //checks how many rounds are left
            if(_round >= 3)
            {
                //checks how many stations are left. This is based on how many stations
                //are in the array, so if some are cut this will still work.
                if(_stationCount < _stations.Count - 1)
                {
                    _round = 0;
                    StopAllCoroutines();
                    StartCoroutine(nameof(StationTransition));
                }
                else
                {
                    StopMinigame();
                }
            }
            else
            {
                //correct answer
                StartStation();
            }
        }
        else
        {
            //decrease round count on failure if not at 0
            if (_round > 0)
            {
                _round--;
            }

            //wrong answer
            StartStation();
        }

        //print(_round);
    }

    /// <summary>
    /// Disables previous screen and displays spotlight and map for next station
    /// </summary>
    /// <returns>Waits for a specified amount of time before displaying next screen</returns>
    private IEnumerator StationTransition()
    {
        if (_stationCount < _stations.Count && _stationCount >= 0 && _stationCount < _layoutStations.Length)
        {
            _stations[_stationCount].StationScreen.SetActive(false);
            _stations[_stationCount].StationBehavior.MakeStationUnconfirmable();
            _layoutStations[_stationCount].sprite = _stationGreyedOutSprite;
        }

        _stationCount++;

        SetSpotlight();

        for (int i = 0; i < _roundTrackers.Length; ++i)
        {
            _roundTrackers[i].color = Color.clear;
        }
        _timerText.text = "";

        if (_stationCount < _layoutStations.Length)
        {
            _layoutStations[_stationCount].sprite = _stationHighlightedSprite;
        }
        _bridgeLayoutScreen.SetActive(true);

        yield return new WaitForSeconds(_bridgeLayoutDisplayTime);

        SwitchStation();
    }

    /// <summary>
    /// This function will contain the functionality to switch stations once one is
    /// complete. It then starts the next station.
    /// </summary>
    private void SwitchStation()
    {
        SetSpotlight();
        UpdateRoundTracker();

        StopAllCoroutines();
        _bridgeLayoutScreen.SetActive(false);
        _timerText.text = "0:00";
        if (_stationCount < _stations.Count)
        {
            _stations[_stationCount].StationScreen.SetActive(true);
        }

        _stations[_stationCount].StationBehavior.MakeStationConfirmable();
        StartStation();
    }

    /// <summary>
    /// Updates round tracker UI to refect the current number of completed rounds
    /// </summary>
    private void UpdateRoundTracker()
    {
        for (int i = 0; i < _roundTrackers.Length; ++i)
        {
            if (i < _round)
            {
                _roundTrackers[i].color = _trackerHighlightedColor;
            }
            else
            {
                _roundTrackers[i].color = _trackerDimmedColor;
            }
        }
    }

    /// <summary>
    /// This function will set the station and screen with the correct information
    /// based on the station count.
    /// </summary>
    private void StartStation()
    {
        UpdateRoundTracker();

        //set random to match list of ints on station and screen 
        List<int> target = _stations[_stationCount].StationBehavior.SetRandomToMatch();
        _stations[_stationCount].ScreenBehavior.SetRandomOrderList(target);
        RestartTimer();
    }

    /// <summary>
    /// Called to start the timer
    /// </summary>
    private void RestartTimer()
    {
        StopAllCoroutines();
        _currentTime = _countDownTime;
        StartCoroutine(nameof(Timer));
    }

    /// <summary>
    /// Counts down from _countDownTime to 0, updating the timer screen text
    /// as it goes.
    /// </summary>
    /// <returns>Waits 1 second</returns>
    private IEnumerator Timer()
    {
        while(_currentTime >= 0)
        {
            yield return new WaitForSeconds(1f);

            int minutes = _currentTime / 60;
            int seconds = _currentTime % 60;
            if (seconds >= 10)
            {
                _timerText.text = minutes + ":" + seconds;
            }
            else
            {
                _timerText.text = minutes + ":0" + seconds;
            }

            _currentTime -= 1;
        }

        if (_round > 0)
        {
            _round--;
        }
        UpdateRoundTracker();
        TriggerFail?.Invoke();
    }

    /// <summary>
    /// This function manages turning the spotlights on and off at the begining of 
    /// the game, then the station switches, and at the end of the game.
    /// </summary>
    private void SetSpotlight()
    {
        //turns off previous spotlight
        if(_stationCount >= 1 && _stationCount < _stations.Count)
        {
            _stations[_stationCount - 1].StationBehavior.SetSpotlight(false);
        }

        //enables next spotlight
        if(_stationCount < _stations.Count)
        {
            
            _stations[_stationCount].StationBehavior.SetSpotlight(true);
        }
    }

    /// <summary>
    /// Function called to start the minigame. Triggered by NpcEventListener
    /// looking for OnMinigameStart with tag Angel.
    /// </summary>
    public void StartMinigame()
    {
        _stationCount = 0;
        SwitchStation();
        TriggerStart?.Invoke();
    }

    /// <summary>
    /// Function called when minigame is complete.
    /// </summary>
    private void StopMinigame()
    {
        StopAllCoroutines();

        for (int i = 0; i < _roundTrackers.Length; ++i)
        {
            _roundTrackers[i].color = Color.clear;
        }

        _timerText.text = "0:00";
        _endMinigameEvent.TriggerEvent(_endMinigameEventTag);
        SetSpotlight();
        _stations[_stationCount].StationBehavior.MakeStationUnconfirmable();

        if (_stationCount < _stations.Count && _stationCount >= 0)
        {
            _stations[_stationCount].StationScreen.SetActive(false);
        }
        _winScreen.SetActive(true);
        print("game over");
    }

    private void OnDisable()
    {
        AngelStationComplete -= SwitchStation;
        CheckState -= CheckStates;
        TriggerFail -= StartStation;
    }
}
