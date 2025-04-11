/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: 
 *    Date Created: 7/9/2024
 *    Description: A manager script for the slideshow player. Has functions that
 *                 play different videos when called.
 *******************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

public class SlideshowManager : MonoBehaviour
{
    [Serializable]
    public struct Video
    {
        public VideoClip Footage;
        public EventReference Audio;
    }

    [Header("Demo Build")]
    [SerializeField] private bool _skipCinematic = false;
    
    [SerializeField] private bool _isIntroVideoPlayer = false;

    [SerializeField] private int _levelSceneBuildIndex;
    [SerializeField] private int _mainMenuBuildIndex;

    [SerializeField] private Video _introVideo;
    [SerializeField] private Video[] _endingVideos;
    [SerializeField] private Video _creditsVideo;
    private EventReference _selectedAudio;
    private PlayerControls _playerControls;
    private InputAction _skipVideo;
    private InputAction _controllerDetection;
    private InputAction _mouseDetection;
    private UIDocument _slideshowUI;
    private VideoPlayer _slideshowPlayer;
    private bool _wasCreditsShown = false;
    private EventInstance _currentAudioPlayback;
    private VisualElement _xboxSkip;
    private VisualElement _xboxMeter;
    private VisualElement _psSkip;
    private VisualElement _psMeter;
    private VisualElement _keyboardSkip;
    private VisualElement _keyboardMeter;

    // 0 = keyboard, 1 = xbox, 2 = ps
    private int _inputDeviceType = 0;

    private const float MeterHoldTime = 2.4f;
    private const int KeyboardMeterWidth = 385;
    private const int XboxMeterWidth = 306;
    private const int PsMeterWidth = 307;


    /// <summary>
    /// Setting references and pause inputs. Also plays intro slide show if needed.
    /// </summary>
    private void Awake()
    {
        if (_skipCinematic && _isIntroVideoPlayer)
        {
            SceneManager.LoadScene(_levelSceneBuildIndex);
        }
        _playerControls = new PlayerControls();
        _playerControls.BasicControls.Enable();
        
        _skipVideo = _playerControls.FindAction("SkipPause");
        _controllerDetection = _playerControls.FindAction("ControllerDetection");
        _mouseDetection = _playerControls.FindAction("Look");
        _skipVideo.Enable();
        _controllerDetection.Enable();
        _mouseDetection.Enable();
        
        // Skip video on hold, pause on press
        _skipVideo.performed +=
            ctx =>
            {
                if (ctx.interaction is HoldInteraction)
                    OnSkipVideo();
                else // Could check for PressInteraction but easier to just assume it's a press.
                    TogglePlayPause();
            };

        _skipVideo.started += ctx => DisplaySkipMeter(true);
        _skipVideo.canceled += ctx => DisplaySkipMeter(false);

        _skipVideo.started += DetectInputType;
        _controllerDetection.performed += DetectInputType;
        _mouseDetection.performed += DetectInputType;
        
        
        _slideshowUI = GetComponent<UIDocument>();
        _slideshowPlayer = GetComponent<VideoPlayer>();

        _xboxSkip = _slideshowUI.rootVisualElement.Q("CutsceneSkipXbox");
        _psSkip = _slideshowUI.rootVisualElement.Q("CutsceneSkipPS");
        _keyboardSkip = _slideshowUI.rootVisualElement.Q("CutsceneSkipMnK");
        _keyboardMeter = _slideshowUI.rootVisualElement.Q("SkipMeterMnK");
        _psMeter = _slideshowUI.rootVisualElement.Q("SkipMeterPS");
        _xboxMeter = _slideshowUI.rootVisualElement.Q("SkipMeterXbox");

        _xboxSkip.style.display = DisplayStyle.None;
        _xboxMeter.style.display = DisplayStyle.None;
        _psSkip.style.display = DisplayStyle.None;
        _psMeter.style.display = DisplayStyle.None;

        _slideshowPlayer.loopPointReached += DonePlaying;
        _slideshowPlayer.prepareCompleted += PlayVideo;

        if (_isIntroVideoPlayer)
        {
            PlayIntroSlideshow();
        }
        else
        {
            _slideshowUI.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    private void OnDisable()
    {
        _slideshowPlayer.loopPointReached -= DonePlaying;
        _slideshowPlayer.prepareCompleted -= PlayVideo;
        _skipVideo.performed -= DetectInputType;
        _controllerDetection.performed -= DetectInputType;
        _mouseDetection.performed -= DetectInputType;
        _skipVideo.performed -= ctx => DisplaySkipMeter(true);
        _skipVideo.canceled -= ctx => DisplaySkipMeter(false);
    }

    ~SlideshowManager()
    {
        _slideshowPlayer.loopPointReached -= DonePlaying;
        _slideshowPlayer.prepareCompleted -= PlayVideo;
        _skipVideo.performed -= DetectInputType;
        _controllerDetection.performed -= DetectInputType;
        _mouseDetection.performed -= DetectInputType;
        _skipVideo.performed -= ctx => DisplaySkipMeter(true);
        _skipVideo.canceled -= ctx => DisplaySkipMeter(false);
    }

    /// <summary>
    /// Invoked when slideshow is over
    /// </summary>
    private void DonePlaying(VideoPlayer vp)
    {
        if (_isIntroVideoPlayer)
        {
            SceneManager.LoadScene(_levelSceneBuildIndex);
        }
        else if (!_wasCreditsShown)
        {
            _selectedAudio = _creditsVideo.Audio;
            _slideshowPlayer.clip = _creditsVideo.Footage;
            _slideshowPlayer.Prepare();
            _wasCreditsShown = true;
        }
        else
        {
            SceneManager.LoadScene(_mainMenuBuildIndex);
            SceneManager.LoadScene(_mainMenuBuildIndex);
        }
    }

    /// <summary>
    /// Invoked once the video content has been prepared
    /// </summary>
    private void PlayVideo(VideoPlayer vp)
    {
        _slideshowUI.rootVisualElement.style.display = DisplayStyle.Flex;
        AudioManager.StopSound(_currentAudioPlayback);
        _currentAudioPlayback = AudioManager.PlaySound(_selectedAudio, transform.position);
        _slideshowPlayer.Play();
    }

    /// <summary>
    /// Plays intro slideshow
    /// </summary>
    public void PlayIntroSlideshow()
    {
        _selectedAudio = _introVideo.Audio;
        _slideshowPlayer.clip = _introVideo.Footage;
        _slideshowPlayer.Prepare();
    }

    /// <summary>
    /// Plays an ending slideshow corresponding to the given index
    /// </summary>
    /// <param name="videoIndex">Index of video to play</param>
    public void PlayEndingSlideshow(int videoIndex = 0)
    {
        if (videoIndex < _endingVideos.Length && videoIndex >= 0)
        {
            PlayerController.Instance.enabled = false;
            AudioManager.StopAllSounds();

            _selectedAudio = _endingVideos[videoIndex].Audio;
            _slideshowPlayer.clip = _endingVideos[videoIndex].Footage;
            _slideshowPlayer.Prepare();
        }
        else
        {
            Debug.LogError("Ending video index " + videoIndex + " is out of bounds", gameObject);
        }
    }

    /// <summary>
    /// Called when play/pause input is given to toggle if video is playing
    /// </summary>
    public void TogglePlayPause()
    {
        if (_slideshowPlayer != null)
        {
            if (_slideshowPlayer.isPlaying)
            {
                _slideshowPlayer.Pause();
            }
            else
            {
                _slideshowPlayer.Play();
            }
        }
    }
    
    /// <summary>
    /// Skip the video when space is held
    /// </summary>
    private void OnSkipVideo()
    {
        DonePlaying(_slideshowPlayer);
    }

    /// <summary>
    /// Called to toggle filling the hold to skip meter
    /// </summary>
    /// <param name="shouldFill"></param>
    private void DisplaySkipMeter(bool shouldFill)
    {
        StopAllCoroutines();

        if (shouldFill)
        {
            StartCoroutine(UpdateSkipMeter());
        }
        else
        {
            _keyboardMeter.style.width = 0;
            _psMeter.style.width = 0;
            _xboxMeter.style.width = 0;
        }
    }

    /// <summary>
    /// Fills the hold to skip meter over time
    /// </summary>
    private IEnumerator UpdateSkipMeter()
    {
        float elapsedTime = 0f;
        float lerpingTime;

        while (elapsedTime < MeterHoldTime)
        {
            lerpingTime = elapsedTime / MeterHoldTime;
            _keyboardMeter.style.width = Mathf.Lerp(0, KeyboardMeterWidth, lerpingTime);
            _xboxMeter.style.width = Mathf.Lerp(0, XboxMeterWidth, lerpingTime);
            _psMeter.style.width = Mathf.Lerp(0, PsMeterWidth, lerpingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _keyboardMeter.style.width = KeyboardMeterWidth;
        _xboxMeter.style.width = XboxMeterWidth;
        _psMeter.style.width = PsMeterWidth;
    }

    /// <summary>
    /// Called to determine what type of input device is being used
    /// </summary>
    /// <param name="context">Input context</param>
    private void DetectInputType(InputAction.CallbackContext context)
    {
        string controlName = context.control.device.displayName.ToLower();
        int newInputDevice;

        if (controlName.Contains("xbox"))
        {
            newInputDevice = 1;
        }
        else if (controlName.Contains("playstation") ||
            controlName.Contains("dualsense") || controlName.Contains("dualshock"))
        {
            newInputDevice = 2;
        }
        else
        {
            newInputDevice = 0;
        }

        if (newInputDevice == _inputDeviceType) { return; }

        _inputDeviceType = newInputDevice;
        UpdateInputPrompts();
    }

    /// <summary>
    /// Changes input prompts when the device type changes
    /// </summary>
    private void UpdateInputPrompts()
    {
        _keyboardSkip.style.display = _inputDeviceType == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        _keyboardMeter.style.display = _inputDeviceType == 0 ? DisplayStyle.Flex : DisplayStyle.None;

        _xboxSkip.style.display = _inputDeviceType == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        _xboxMeter.style.display = _inputDeviceType == 1 ? DisplayStyle.Flex : DisplayStyle.None;

        _psSkip.style.display = _inputDeviceType == 2 ? DisplayStyle.Flex : DisplayStyle.None;
        _psMeter.style.display = _inputDeviceType == 2 ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
