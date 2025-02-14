/******************************************************************
 *    Author: Nick Grinstead
 *    Contributors: 
 *    Date Created: 7/11/2024
 *    Description: A menu controller script for the main menu and its various functions.
 *******************************************************************/
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Users;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _mainMenuDoc;
    [SerializeField] private int _introVideoBuildIndex;
    [SerializeField] private float _tabAnimationTime;
    [SerializeField] private EventReference mainMenuMusicEvent;
    [SerializeField] private EventReference clickEvent;
    [SerializeField] private int _gameSceneVideoIndex = 2;

    #region Constants
    private const string NewGameButtonName = "NewGameButton";
    private const string ContinueButtonName = "ContinueButton";
    private const string SettingsButtonName = "SettingsButton";
    private const string QuitButtonName = "QuitButton";
    private const string ConfirmNoButtonName = "ConfirmNoButton";
    private const string ConfirmYesButtonName = "ConfirmYesButton";
    private const string ConfirmationTextName = "ProceedText";
    private const string AudioButtonName = "AudioButton";
    private const string ControlsButtonName = "ControlsButton";
    private const string SplashScreenName = "SplashScreenHolder";
    private const string MainScreenName = "MainMenuHolder";
    private const string MainButtonsHolderName = "MainButtonHolder";
    private const string SettingsSelectionName = "SettingsSelectionHolder";
    private const string SettingsBackPromptName = "SelectionBackPrompt";
    private const string ControlsScreenName = "ControlsHolder";
    private const string AudioScreenName = "AudioHolder";
    private const string NewGameTabName = "NewGameTab";
    private const string ContinueTabName = "ContinueTab";
    private const string SettingsTabName = "SettingsTab";
    private const string QuitTabName = "QuitTab";
    private const string MouseSensSliderName = "MouseSensSlider";
    private const string MasterSliderName = "MasterSlider";
    private const string MusicSliderName = "MusicSlider";
    private const string SfxSliderName = "SFXSlider";
    #endregion

    #region Private
    private Button _newGameButton;
    private Button _continueButton;
    private Button _settingsButton;
    private Button _quitButton;
    private Button _confirmNoButton;
    private Button _confirmYesButton;
    private Label _confirmText;
    private Button _controlsButton;
    private Button _audioButton;
    private VisualElement _splashScreen;
    private VisualElement _mainMenuScreen;
    private VisualElement _mainButtonHolder;
    private VisualElement _controlsScreen;
    private VisualElement _audioScreen;
    private VisualElement _settingsSelectionHolder;
    private VisualElement _settingsBackPrompt;
    private VisualElement _newGameTab;
    private VisualElement _continueTab;
    private VisualElement _settingsTab;
    private VisualElement _quitTab;
    private Slider _mouseSensSlider;
    private Slider _masterVolSlider;
    private Slider _musicVolSlider;
    private Slider _sfxVolSlider;

    private Coroutine _activeCoroutine;
    private bool _canAnimateTabs = true;

    private GameObject _lastFocusedElement;
    private Button _lastFocusedVisualElement;

    // 0 = splash, 1 = main, 2 = settings selection, 3 = settings submenu
    private int _currentScreenIndex = 0;

    private int _currentMenuButtonIndex = 0;

    private SaveLoadManager _savingManager;
    private PlayerControls _playerControls;
    private InputAction _startGame;
    private InputAction _backInput;
    private InputAction _quitInput;

    private List<VisualElement> _defaultDraggers;
    private List<VisualElement> _newDraggers = new List<VisualElement>();
    private UQueryBuilder<Button> _allButtons;
    private List<Slider> _sliders = new List<Slider>();
    private EventInstance _mainMenuMusicInstance;
    private SettingsManager _settingsManager;

    private bool _isFocused = false;
    #endregion

    #region Initialization
    /// <summary>
    /// Registering button callbacks and finding visual elements
    /// </summary>
    private void Awake()
    {
        // Setting up player inputs
        _playerControls = new PlayerControls();
        _playerControls.UI.Enable();
        _startGame = _playerControls.FindAction("StartGame");
        _backInput = _playerControls.FindAction("Cancel");
        _playerControls.UI.ControllerDetection.performed += ctx => ControllerUsed();
        _quitInput = _playerControls.FindAction("QuitGame");
        _startGame.performed += ctx => CloseSplashScreen();
        _backInput.performed += ctx => BackButtonClicked();
        _quitInput.performed += ctx => QuitButtonClicked(null);

        // Assigning screen element references
        _splashScreen = _mainMenuDoc.rootVisualElement.Q(SplashScreenName);
        _mainMenuScreen = _mainMenuDoc.rootVisualElement.Q(MainScreenName);
        _mainButtonHolder = _mainMenuDoc.rootVisualElement.Q(MainButtonsHolderName);
        _settingsSelectionHolder = _mainMenuDoc.rootVisualElement.Q(SettingsSelectionName);
        _settingsBackPrompt = _mainMenuDoc.rootVisualElement.Q(SettingsBackPromptName);
        _controlsScreen = _mainMenuDoc.rootVisualElement.Q(ControlsScreenName);
        _audioScreen = _mainMenuDoc.rootVisualElement.Q(AudioScreenName);

        // Assigning button related references
        _newGameButton = _mainMenuDoc.rootVisualElement.Q<Button>(NewGameButtonName);
        _settingsButton = _mainMenuDoc.rootVisualElement.Q<Button>(SettingsButtonName);
        _continueButton = _mainMenuDoc.rootVisualElement.Q<Button>(ContinueButtonName);
        _quitButton = _mainMenuDoc.rootVisualElement.Q<Button>(QuitButtonName);
        _confirmNoButton = _mainMenuDoc.rootVisualElement.Q<Button>(ConfirmNoButtonName);
        _confirmYesButton = _mainMenuDoc.rootVisualElement.Q<Button>(ConfirmYesButtonName);
        _confirmText = _mainMenuDoc.rootVisualElement.Q<Label>(ConfirmationTextName);
        _audioButton = _mainMenuDoc.rootVisualElement.Q<Button>(AudioButtonName);
        _controlsButton = _mainMenuDoc.rootVisualElement.Q<Button>(ControlsButtonName);

        // Assigning slider references
        _mouseSensSlider = _mainMenuDoc.rootVisualElement.Q<Slider>(MouseSensSliderName);
        _masterVolSlider = _mainMenuDoc.rootVisualElement.Q<Slider>(MasterSliderName);
        _musicVolSlider = _mainMenuDoc.rootVisualElement.Q<Slider>(MusicSliderName);
        _sfxVolSlider = _mainMenuDoc.rootVisualElement.Q<Slider>(SfxSliderName);

        // Assigning animated tab references
        _newGameTab = _mainMenuDoc.rootVisualElement.Q(NewGameTabName);
        _continueTab = _mainMenuDoc.rootVisualElement.Q(ContinueTabName);
        _settingsTab = _mainMenuDoc.rootVisualElement.Q(SettingsTabName);
        _quitTab = _mainMenuDoc.rootVisualElement.Q(QuitTabName);

        // Registering general button NavigationSubmitEvent callbacks
        _newGameButton.RegisterCallback<NavigationSubmitEvent>(NewGameButtonClicked);
        _settingsButton.RegisterCallback<NavigationSubmitEvent>(SettingsButtonClicked);
        _continueButton.RegisterCallback<NavigationSubmitEvent>(ContinueButtonClicked);
        _quitButton.RegisterCallback<NavigationSubmitEvent>(QuitButtonClicked);
        _confirmNoButton.RegisterCallback<NavigationSubmitEvent>(ConfirmNoButtonClicked);
        _confirmYesButton.RegisterCallback<NavigationSubmitEvent>(StartNewGame);
        _audioButton.RegisterCallback<NavigationSubmitEvent>(AudioButtonClicked);
        _controlsButton.RegisterCallback<NavigationSubmitEvent>(ControlsButtonClicked);

        // Registering callbacks for animated tabs
        _newGameButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_newGameTab, true); });
        _newGameButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_newGameTab, false); });
        _audioButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
        _audioButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        _controlsButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_quitTab, true); });
        _controlsButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_quitTab, false); });

        _newGameButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_newGameButton); });
        _audioButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_audioButton); });
        _controlsButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_controlsButton); });
        _settingsButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_settingsButton); });
        _quitButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_quitButton); });

        _newGameButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _audioButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _controlsButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _settingsButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _quitButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });

        _newGameButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_newGameTab, true); });
        _newGameButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_newGameTab, false); });
        _audioButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
        _audioButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        _controlsButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_quitTab, true); });
        _controlsButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_quitTab, false); });

        _mainMenuMusicInstance = AudioManager.PlaySound(mainMenuMusicEvent, Vector3.zero);
        _allButtons = _mainMenuDoc.rootVisualElement.Query<Button>();
        _allButtons.ForEach(button => button.RegisterCallback<NavigationSubmitEvent>(PlayConfirmSound));
        _sliders = _audioScreen.Query<Slider>().ToList();
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName("MasterVolume", out float volume);
        _sliders[0].value = volume;
        _sliders[0].RegisterCallback<ChangeEvent<float>>(MasterAudioSliderChanged);
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName("SFXVolume", out volume);
        _sliders[1].value = volume;
        _sliders[1].RegisterCallback<ChangeEvent<float>>(SFXAudioSliderChanged);
        FMODUnity.RuntimeManager.StudioSystem.getParameterByName("MusicVolume", out volume);
        _sliders[2].value = volume;
        _sliders[2].RegisterCallback<ChangeEvent<float>>(MusicAudioSliderChanged);

        foreach (var slider in _sliders)
        {
            slider.RegisterCallback<NavigationMoveEvent>(evt => UpdateSliderWithEvent(slider, evt.direction));
        }

        _mouseSensSlider.RegisterCallback<NavigationMoveEvent>(evt => UpdateSliderWithEvent(_mouseSensSlider, evt.direction));
    }

    /// <summary>
    /// Grabbing reference to settings manager
    /// </summary>
    private void Start()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        _settingsManager = SettingsManager.Instance;
        if (_settingsManager != null)
        {
            _mouseSensSlider.value = _settingsManager.MouseSensitivity;
            _masterVolSlider.value = _settingsManager.MasterVolume;
            _musicVolSlider.value = _settingsManager.MusicVolume;
            _sfxVolSlider.value = _settingsManager.SfxVolume;
        }

        _savingManager = SaveLoadManager.Instance;
        // Makes continue button visible if saved data exists and registers relevant animated tab callbacks
        if (_savingManager != null && _savingManager.DoesSaveFileExist())
        {
            _continueButton.style.display = DisplayStyle.Flex;

            _continueButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_continueTab, true); });
            _continueButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _settingsButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
            _settingsButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });
            _quitButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_quitTab, true); });
            _quitButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_quitTab, false); });

            _continueButton.RegisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_continueButton); });
            _continueButton.RegisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });

            _continueButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_continueTab, true); });
            _continueButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _settingsButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
            _settingsButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
            _quitButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_quitTab, true); });
            _quitButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_quitTab, false); });
        }
        else
        {
            // Using different tabs to account for the lack of a continue button
            _continueButton.style.display = DisplayStyle.None;

            _settingsButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_continueTab, true); });
            _settingsButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _quitButton.RegisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
            _quitButton.RegisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });

            _settingsButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_continueTab, true); });
            _settingsButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _quitButton.RegisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
            _quitButton.RegisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        }
    }

    /// <summary>
    /// Unregistering button callbacks
    /// </summary>
    private void OnDisable()
    {
        // Removing player input callbacks
        _startGame.performed -= ctx => CloseSplashScreen();
        _backInput.performed -= ctx => BackButtonClicked();
        _playerControls.UI.ControllerDetection.performed -= ctx => ControllerUsed();

        // Unregistering button NavigationSubmitEvent callbacks
        _newGameButton.UnregisterCallback<NavigationSubmitEvent>(NewGameButtonClicked);
        _continueButton.UnregisterCallback<NavigationSubmitEvent>(ContinueButtonClicked);
        _settingsButton.UnregisterCallback<NavigationSubmitEvent>(SettingsButtonClicked);
        _quitButton.UnregisterCallback<NavigationSubmitEvent>(QuitButtonClicked);
        _confirmNoButton.UnregisterCallback<NavigationSubmitEvent>(ConfirmNoButtonClicked);
        _confirmYesButton.UnregisterCallback<NavigationSubmitEvent>(StartNewGame);
        _audioButton.UnregisterCallback<NavigationSubmitEvent>(AudioButtonClicked);
        _controlsButton.UnregisterCallback<NavigationSubmitEvent>(ControlsButtonClicked);
        // Unregistering animated tab related callbacks
        _newGameButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_newGameTab, true); });
        _newGameButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_newGameTab, false); });
        _audioButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
        _audioButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        _controlsButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_quitTab, true); });
        _controlsButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_quitTab, false); });

        _newGameButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_newGameButton); });
        _audioButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_audioButton); });
        _controlsButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_controlsButton); });
        _settingsButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_settingsButton); });
        _quitButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_quitButton); });
        _continueButton.UnregisterCallback<MouseOverEvent>(evt => { ChangeButtonFocus(_continueButton); });

        _newGameButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _audioButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _controlsButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _settingsButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _quitButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });
        _continueButton.UnregisterCallback<MouseOutEvent>(evt => { ClearButtonFocus(); });

        _newGameButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_newGameTab, true); });
        _newGameButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_newGameTab, false); });
        _audioButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
        _audioButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        _controlsButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_quitTab, true); });
        _controlsButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_quitTab, false); });

        // Unregistering animated tab callbacks dependent on the continue button being present
        if (_savingManager != null && _savingManager.DoesSaveFileExist())
        {
            _continueButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_continueTab, true); });
            _continueButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _settingsButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
            _settingsButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });
            _quitButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_quitTab, true); });
            _quitButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_quitTab, false); });

            _continueButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_continueTab, true); });
            _continueButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _settingsButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
            _settingsButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
            _quitButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_quitTab, true); });
            _quitButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_quitTab, false); });
        }
        else
        {
            _settingsButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_continueTab, true); });
            _settingsButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _quitButton.UnregisterCallback<MouseOverEvent>(evt => { AnimateTab(_settingsTab, true); });
            _quitButton.UnregisterCallback<MouseOutEvent>(evt => { AnimateTab(_settingsTab, false); });

            _settingsButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_continueTab, true); });
            _settingsButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_continueTab, false); });
            _quitButton.UnregisterCallback<FocusInEvent>(evt => { AnimateTab(_settingsTab, true); });
            _quitButton.UnregisterCallback<FocusOutEvent>(evt => { AnimateTab(_settingsTab, false); });
        }
        _allButtons.ForEach(button => button.UnregisterCallback<NavigationSubmitEvent>(PlayConfirmSound));
        _sliders[0].UnregisterCallback<ChangeEvent<float>>(MasterAudioSliderChanged);
        _sliders[1].UnregisterCallback<ChangeEvent<float>>(SFXAudioSliderChanged);
        _sliders[2].UnregisterCallback<ChangeEvent<float>>(MusicAudioSliderChanged);
        AudioManager.StopSound(_mainMenuMusicInstance);

        foreach (var slider in _sliders)
        {
            slider.UnregisterCallback<NavigationMoveEvent>(evt => UpdateSliderWithEvent(slider, evt.direction));
        }

        _mouseSensSlider.UnregisterCallback<NavigationMoveEvent>(evt => UpdateSliderWithEvent(_mouseSensSlider, evt.direction));
    }
    #endregion

    #region ButtonFunctions
    /// <summary>
    /// Called when mouse leaves a button
    /// </summary>
    private void ClearButtonFocus()
    {
        EventSystem.current.SetSelectedGameObject(null);
        _isFocused = false;
    }

    /// <summary>
    /// Called when the mouse hovers over a button after using a controller
    /// to change what button is focused
    /// </summary>
    private void ChangeButtonFocus(Button buttonToFocus)
    {
        buttonToFocus.Focus();
        _lastFocusedVisualElement = buttonToFocus;
    }

    /// <summary>
    /// When a controller is used and the game isn't focused on something,
    /// focus on a button
    /// </summary>
    private void ControllerUsed()
    {
        if (_isFocused) { return; }

        _isFocused = true;

        if (_lastFocusedVisualElement != null)
        {
            _lastFocusedVisualElement.Focus();
        }
        else if (_currentScreenIndex == 0)
        {
            _newGameButton.Focus();
        }
        else if (_currentScreenIndex == 1)
        {
            _audioButton.Focus();
        }
    }

    /// <summary>
    /// Closes the splash screen when enter is pressed
    /// </summary>
    private void CloseSplashScreen()
    {
        if (_currentScreenIndex == 0)
        {
            _currentScreenIndex = 1;
            _startGame.performed -= ctx => CloseSplashScreen();
            _splashScreen.style.display = DisplayStyle.None;
            _mainMenuScreen.style.display = DisplayStyle.Flex;
            _newGameButton.Focus();
        }
    }

    /// <summary>
    /// Loads intro cutscene
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void ContinueButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 1)
            SceneManager.LoadScene(_gameSceneVideoIndex);
    }

    /// <summary>
    /// Opens settings selection menu
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void SettingsButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 1)
        {
            _currentScreenIndex = 2;
            _mainButtonHolder.style.display = DisplayStyle.None;
            _settingsSelectionHolder.style.display = DisplayStyle.Flex;
            _settingsBackPrompt.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Opens audio options submenu
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void AudioButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 2)
        {
            _currentScreenIndex = 3;
            if (_settingsManager != null)
            {
                _masterVolSlider.value = _settingsManager.MasterVolume;
                _musicVolSlider.value = _settingsManager.MusicVolume;
                _sfxVolSlider.value = _settingsManager.SfxVolume;
            }
            _settingsSelectionHolder.style.display = DisplayStyle.None;
            _settingsBackPrompt.style.display = DisplayStyle.None;
            _audioScreen.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Opens controls options submenu
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void ControlsButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 2)
        {
            _currentScreenIndex = 3;
            if (_settingsManager != null)
            {
                _mouseSensSlider.value = _settingsManager.MouseSensitivity;
            }
            _settingsSelectionHolder.style.display = DisplayStyle.None;
            _settingsBackPrompt.style.display = DisplayStyle.None;
            _controlsScreen.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Pulls up confirmation UI
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void NewGameButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 1)
        {
            AnimateTab(_newGameTab, true, true);
            _canAnimateTabs = false;

            _continueButton.SetEnabled(false);
            _settingsButton.SetEnabled(false);
            _quitButton.SetEnabled(false);
        }
    }

    /// <summary>
    /// Deletes existing save data and loads the intro scene
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void StartNewGame(NavigationSubmitEvent clicked)
    {
        if (_savingManager != null)
        {
            _savingManager.DeleteSaveData();
        }

        SceneManager.LoadScene(_introVideoBuildIndex);
    }

    /// <summary>
    /// Pulls up new game confirmation options
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void ConfirmNoButtonClicked(NavigationSubmitEvent clicked)
    {
        _canAnimateTabs = true;
        AnimateTab(_newGameTab, false);

        _confirmText.style.display = DisplayStyle.None;
        _confirmNoButton.style.display = DisplayStyle.None;
        _confirmYesButton.style.display = DisplayStyle.None;

        _continueButton.SetEnabled(true);
        _settingsButton.SetEnabled(true);
        _quitButton.SetEnabled(true);
    }

    /// <summary>
    /// Closes application
    /// </summary>
    /// <param name="clicked">Click event</param>
    private void QuitButtonClicked(NavigationSubmitEvent clicked)
    {
        if (_currentScreenIndex == 1)
            Application.Quit();
    }

    /// <summary>
    /// Returns to main menu screen from submenu
    /// </summary>
    private void BackButtonClicked()
    {
        if (_currentScreenIndex == 2)
        {
            _currentScreenIndex = 1;
            _settingsSelectionHolder.style.display = DisplayStyle.None;
            _settingsBackPrompt.style.display = DisplayStyle.None;
            _mainButtonHolder.style.display = DisplayStyle.Flex;
        }
        else if (_currentScreenIndex == 3)
        {
            _currentScreenIndex = 2;
            if (_settingsManager != null)
            {
                _settingsManager.SetMouseSensitivity(_mouseSensSlider.value);
                _settingsManager.SetVolumeValues(_masterVolSlider.value, _musicVolSlider.value, _sfxVolSlider.value);
            }
            _controlsScreen.style.display = DisplayStyle.None;
            _audioScreen.style.display = DisplayStyle.None;
            _settingsSelectionHolder.style.display = DisplayStyle.Flex;
            _settingsBackPrompt.style.display = DisplayStyle.Flex;
        }
    }
    #endregion

    #region TabAnimationFunctions
    /// <summary>
    /// Called when hovering over or off of a main menu button to start an animation
    /// </summary>
    /// <param name="tabToAnimate">Tab that needs to animate</param>
    /// <param name="isActive">True if tab should move on screen</param>
    /// <param name="extendNGButton">Normally false, true when extending new game tab out for confirmation</param>
    private void AnimateTab(VisualElement tabToAnimate, bool isActive, bool extendNGButton = false)
    {
        Debug.Log("Tab to animate: " + tabToAnimate.name);

        if (_canAnimateTabs)
        {
            if (_activeCoroutine != null && !isActive)
            {
                StopCoroutine(_activeCoroutine);
            }

            if (extendNGButton)
            {
                _activeCoroutine = StartCoroutine(ScaleTabWidth(tabToAnimate, 1171f, (float)tabToAnimate.resolvedStyle.width));
            }
            else if (isActive)
            {
                _activeCoroutine = StartCoroutine(ScaleTabWidth(tabToAnimate, 322f, (float)tabToAnimate.resolvedStyle.width));
            }
            else
            {
                _activeCoroutine = StartCoroutine(ScaleTabWidth(tabToAnimate, 0f, (float)tabToAnimate.resolvedStyle.width));
            }
        }
    }

    /// <summary>
    /// Lerps a tab's width over a period of time
    /// </summary>
    /// <param name="tabToAnimate">Tab being animated</param>
    /// <param name="targetWidth">Width to reach</param>
    /// <param name="startingWidth">The starting width of the tab</param>
    /// <returns></returns>
    private IEnumerator ScaleTabWidth(VisualElement tabToAnimate, float targetWidth, float startingWidth)
    {
        float elapsedTime = 0f;
        float lerpingTime;

        while (elapsedTime < _tabAnimationTime)
        {
            lerpingTime = elapsedTime / _tabAnimationTime;
            tabToAnimate.style.width = Mathf.Lerp(startingWidth, targetWidth, lerpingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        tabToAnimate.style.width = targetWidth;
        
        // Displays proceed options if new game tab was extended
        if (targetWidth > 322f)
        {
            _confirmNoButton.style.display = DisplayStyle.Flex;
            _confirmYesButton.style.display = DisplayStyle.Flex;
            _confirmText.style.display = DisplayStyle.Flex;
        }
    }

    private void PlayConfirmSound(NavigationSubmitEvent evt)
    {
        AudioManager.PlaySound(clickEvent, transform.position);
    }
    
    private void MasterAudioSliderChanged(ChangeEvent<float> evt)
    {
        //0-100 value expected.
        var newVolume = evt.newValue;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", newVolume);
    }

    private void SFXAudioSliderChanged(ChangeEvent<float> evt)
    {
        //0-100 value expected.
        var newVolume = evt.newValue;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SFXVolume", newVolume);
    }

    private void MusicAudioSliderChanged(ChangeEvent<float> evt)
    {
        //0-100 value expected.
        var newVolume = evt.newValue;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MusicVolume", newVolume);
    }
    #endregion

    /// <summary>
    /// Moves the sliders more smoothly when using a controller
    /// </summary>
    /// <param name="selectedSlider">Slider to move</param>
    /// <param name="direction">Direction to move the slider</param>
    private void UpdateSliderWithEvent(Slider selectedSlider, NavigationMoveEvent.Direction direction)
    {
        float multiplier = direction == NavigationMoveEvent.Direction.Left ? -1f :
            direction == NavigationMoveEvent.Direction.Right ? 1f : 0f;
        selectedSlider.value += 2.5f * multiplier;
    }
}
