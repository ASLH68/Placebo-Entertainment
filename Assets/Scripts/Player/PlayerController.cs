/*****************************************************************************
// File Name :         PlayerController.cs
// Author :            Nick Grinsteasd & Mark Hanson, Andrea Swihart-DeCoster
// Creation Date :     5/16/2024
//
// Brief Description : All player actions which includes movement, looking around, and interacting with the world
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.VisualScripting;
using Utils;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    [SerializeField] bool _isOnMainMenu = false;
    [SerializeField] float _moveSpeed;
    [SerializeField] float _jumpForce;
    [Header("Audio")]
    [SerializeField] private Oscillator oscillator = new(30f);
    [SerializeField] private FMODUnity.EventReference footStepEvent;
    [Header("VFX")]
    [SerializeField] private ParticleSystem _footPrints;
    private ParticleSystem.EmissionModule _footPrintEmission;

    [SerializeField] public Texture2D[] _psControllerUI;
    [SerializeField] public Texture2D[] _xboxControllerUI;
    
    //code specifically for the "The other company" achievement
    private float _animatorIdleTime;
    private bool _idleAchievementNeeded = true;

    //Anim Controller
    public static Animator Animator { get; private set; }

    public PlayerControls PlayerControls { get; private set; }
    public InputAction Move, Interact, Reset, Shoot;

    Rigidbody _rb;
    CinemachineVirtualCamera _mainCamera;
    CinemachineTransposer _transposer;

    PlayerInteractSystem InteractionCheck;
    private bool _doOnce;
    private bool _isInDialogue = false;

    [SerializeField] bool _isKinemat;

    private bool _isMoving = false;
    private Vector2 _moveDirection;
    private Vector3 _velocity;

    private bool _isGrounded = true;
    private float _groundedDistance = 0.3f;
    [SerializeField] LayerMask _groundMask;
    [SerializeField] Transform _groundChecker;

    private void OnEnable()
    {
        Move.performed += ctx => _moveDirection = Move.ReadValue<Vector2>();
        Move.performed += ctx => _isMoving = true;
        Move.canceled += ctx => _moveDirection = Move.ReadValue<Vector2>();
        Move.canceled += ctx => _isMoving = false;
        Move.canceled += ctx => HaltVelocity();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _rb = GetComponent<Rigidbody>();
        _mainCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _transposer = _mainCamera.GetCinemachineComponent<CinemachineTransposer>();
        _footPrintEmission = _footPrints.emission;

        PlayerControls = new PlayerControls();
        PlayerControls.BasicControls.Enable();
        PlayerControls.UI.Enable();

        InteractionCheck = new PlayerInteractSystem("Default None");
        _doOnce = true;

        _mainCamera.transform.rotation = transform.rotation;
        _mainCamera.transform.position = transform.position;

        //Finding Anim Controller
        Animator = GetComponentInChildren<Animator>();

        Move = PlayerControls.FindAction("Move");
        Interact = PlayerControls.FindAction("Interact");
        Reset = PlayerControls.FindAction("Reset");
        Shoot = PlayerControls.FindAction("LeftClick");
    }

    void FixedUpdate()
    {
        // Player Movement
        if (!_isInDialogue && _isMoving)
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            _velocity = transform.right * _moveDirection.x + transform.forward * _moveDirection.y;
            _velocity = _velocity.normalized * _moveSpeed;
            if (_velocity.sqrMagnitude >= 1f)
            {
                oscillator.Advance(Time.fixedDeltaTime);
                if (oscillator.Wrapped)
                {
                    AudioManager.PlaySoundUnManaged(footStepEvent, _groundChecker.position);
                }
            }
            _rb.AddForce(_velocity, ForceMode.VelocityChange);
            _footPrintEmission.enabled = true;
            
            //Starts Walking Anim
            Animator.SetFloat("Speed", _velocity.magnitude);
            
            //Code specifically for the "The other company" achievement
            if(_idleAchievementNeeded)
            ++_animatorIdleTime;

        }

        if (_animatorIdleTime > 24 && _idleAchievementNeeded)
        {
            //Put "The other company" achievement here and also a way to save the achievement then use in the if statement if null so it doesn't keep giving the achievement
            if (!SteamAchievements.Instance.IsUnityNull())
            {
                SteamAchievements.Instance.UnlockSteamAchievement("COMPANY");
                _idleAchievementNeeded = false; 
            }
        }
        if(_isKinemat)
        {
            _rb.isKinematic = true;
        }
        if(_isKinemat == false)
        {
            _rb.isKinematic = false;
        }

        // Ground Check
        if (!_isGrounded)
            _isGrounded = Physics.CheckSphere(_groundChecker.position, _groundedDistance, _groundMask);

        if (Interact.IsPressed() && _doOnce)
        {
            InteractionCheck.CallInteract();
        }

#if UNITY_EDITOR
        if (Reset.IsPressed())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
#endif

        // Player Rotation
        if (!_isInDialogue)
            _rb.rotation = Quaternion.Euler(0, _mainCamera.transform.eulerAngles.y, 0);
    }

    public void RotateCharacterToTransform(Transform lookTarget)
    {
        //Vector3 direction = lookTarget.transform.position - transform.position;
        //Quaternion rotation = Quaternion.LookRotation(direction);
        _rb.constraints = RigidbodyConstraints.None;
        float angle = Mathf.Atan2(lookTarget.localPosition.y - transform.localPosition.y,
            transform.localPosition.x - lookTarget.localPosition.x) * Mathf.Rad2Deg;
        _rb.rotation = Quaternion.Euler(0, angle, 0);
        transform.LookAt(lookTarget);
        _mainCamera.transform.eulerAngles = new Vector3(0, angle, 0);
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | 
            RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Called to lock or unlock player and camera movement during dialogue
    /// </summary>
    /// <param name="isLocked">Whether the player should move or not</param>
    public void LockCharacter(bool isLocked)
    {
        // Stops the camera from jittering interacting with NPCs that have no dialogue
        if (_isInDialogue == isLocked)
            return;

        _isInDialogue = isLocked;

        if (isLocked)
        {
            CinemachineCore.UniformDeltaTimeOverride = 0;
        }
        else
        {
            Invoke(nameof(DelayedCameraUnlock), 0.1f);
        }

        _mainCamera.gameObject.SetActive(!isLocked);
    }

    /// <summary>
    /// Helper function inovoked to delay regaining camera control post-dialogue
    /// </summary>
    private void DelayedCameraUnlock()
    {
        if (!_isInDialogue)
            CinemachineCore.UniformDeltaTimeOverride = 1;
    }

    /// <summary>
    /// Invoked when move input is cancelled to stop player movement
    /// </summary>
    private void HaltVelocity()
    {
        if (_rb != null)
        {
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            _footPrintEmission.enabled = false;
            
            //Stops Walking Anim
            Animator.SetFloat("Speed", 0);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Interactable")
        {
            InteractionCheck = new PlayerInteractSystem(col.name);
        }
    }
    void OnTriggerExit(Collider col)
    {
        InteractionCheck = new PlayerInteractSystem("Default None");
    }

    private void OnDisable()
    {
        Move.performed -= ctx => _moveDirection = Move.ReadValue<Vector2>();
        Move.performed -= ctx => _isMoving = true;
        Move.canceled -= ctx => _moveDirection = Move.ReadValue<Vector2>();
        Move.canceled -= ctx => _isMoving = false;
        Move.canceled -= ctx => HaltVelocity();
    }

}
