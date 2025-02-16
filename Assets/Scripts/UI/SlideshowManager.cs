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
    private UIDocument _slideshowUI;
    private VideoPlayer _slideshowPlayer;
    private bool _wasCreditsShown = false;
    private EventInstance _currentAudioPlayback;

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
        _skipVideo.Enable();
        
        // Skip video on hold, pause on press
        _skipVideo.performed +=
            ctx =>
            {
                if (ctx.interaction is HoldInteraction)
                    OnSkipVideo();
                else // Could check for PressInteraction but easier to just assume it's a press.
                    TogglePlayPause();
            };
        
        
        _slideshowUI = GetComponent<UIDocument>();
        _slideshowPlayer = GetComponent<VideoPlayer>();

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
    }

    ~SlideshowManager()
    {
        _slideshowPlayer.loopPointReached -= DonePlaying;
        _slideshowPlayer.prepareCompleted -= PlayVideo;
        
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
}
