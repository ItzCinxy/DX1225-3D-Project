using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Canvas _skillTreeCanvas;
    [SerializeField] private Transform _raycastStart;
    [SerializeField] private SkinnedMeshRenderer _skin;
    [SerializeField] private WeaponHolder _weaponHolder;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera _thirdPersonCamera;
    [SerializeField] private CinemachineVirtualCamera _firstPersonCamera;

    [Header("Active Panels")]
    [SerializeField] private List<GameObject> activePanels;

    private bool isFirstPerson = false;
    private bool isSkillTreeOpen = false;
    private Vector2 inputDirection;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;
    private float jumpTimer;
    private bool jumped;
    private InputActionAsset _inputActions;

    private Vector3 targetShoulderOffset; 

    private void Start()
    {
        _inputActions = _playerInput.actions;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _skillTreeCanvas?.gameObject.SetActive(false);

    }

    private void Update()
    {
        HandleSkillTreeToggle();
        HandleCameraToggle();
        HandleGamePause();

        if (isSkillTreeOpen) return;

        HandleGroundCheck();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        RotateWithCamera();
    }

    private void HandleSkillTreeToggle()
    {
        if (_inputActions["ToggleSkillTree"].WasPressedThisFrame())
        {
            isSkillTreeOpen = !isSkillTreeOpen;
        }
    }

    private void HandleCameraToggle()
    {
        if (_inputActions["ToggleCamera"].WasPressedThisFrame())
        {
            isFirstPerson = !isFirstPerson;
            UpdateCameraView();
        }
    }

    private void HandleGamePause()
    {
        bool isAnyPanelActive = activePanels.Exists(panel => panel.activeSelf);

        if (isSkillTreeOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            _skillTreeCanvas?.gameObject.SetActive(true);
        }
        else if (isAnyPanelActive) // Check if any panel is active
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log($"SkillTree: {isSkillTreeOpen}, AnyPanelActive: {isAnyPanelActive}");
    }

    private void UpdateCameraView()
    {
        _firstPersonCamera.Priority = isFirstPerson ? 20 : 10;
        _thirdPersonCamera.Priority = isFirstPerson ? 10 : 20;
        _skin.gameObject.SetActive(!isFirstPerson);
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(_raycastStart.position, Vector3.down, groundCheckDistance, groundLayer);
        if (isGrounded && velocity.y < 0) velocity.y = -5f;
    }

    private void HandleCrouch()
    {
        isCrouching = _inputActions["Crouch"].IsPressed();

        if (isCrouching)
        {
            isSprinting = false;
            _characterController.height = 0.9f;
            _characterController.center = new Vector3(0, 0.65f, 0);
            targetShoulderOffset = new Vector3(0.5f, -1f, 0f);
        }
        else
        {
            _characterController.height = 1.7f;
            _characterController.center = new Vector3(0, 1.1f, 0);
            targetShoulderOffset = new Vector3(0.5f, -0.5f, 0f);
        }

        Cinemachine3rdPersonFollow followComponent = _thirdPersonCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (followComponent != null)
        {
            followComponent.ShoulderOffset = Vector3.Lerp(followComponent.ShoulderOffset, targetShoulderOffset, Time.deltaTime * 8f);
        }

        _animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleMovement()
    {
        inputDirection = _inputActions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = CalculateMoveDirection();
        isMoving = moveDirection.magnitude > 0;

        isSprinting = _inputActions["Sprint"].IsPressed() && inputDirection.y > 0 && isGrounded && !isCrouching;
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : (isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed);

        ResetMovementAnimations();
        SetMovementAnimations(inputDirection);

        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        _characterController.Move(finalMove * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
    }

    private Vector3 CalculateMoveDirection()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        return (cameraForward * inputDirection.y + cameraRight * inputDirection.x).normalized;
    }

    private void RotateWithCamera()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        if (cameraForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }
    }

    private void HandleJump()
    {
        if (!jumped && _inputActions["Jump"].WasPressedThisFrame() && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _animator.SetBool("IsJumping", true);
            jumped = true;
        }

        if (jumped)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= 0.5f)
            {
                jumped = false;
                _animator.SetBool("IsJumping", false);
                jumpTimer = 0f;
            }
        }
    }

    private void ResetMovementAnimations()
    {
        string[] animations = { "WalkForward", "WalkBackward", "WalkLeft", "WalkRight", "CrouchForward", "CrouchBackward", "CrouchLeft", "CrouchRight" };
        foreach (string anim in animations) _animator.SetBool(anim, false);
    }

    private void SetMovementAnimations(Vector2 input)
    {
        if (isCrouching)
        {
            _animator.SetBool("CrouchForward", input.y > 0);
            _animator.SetBool("CrouchBackward", input.y < 0);
            _animator.SetBool("CrouchLeft", input.x < 0);
            _animator.SetBool("CrouchRight", input.x > 0);
        }
        else
        {
            _animator.SetBool("WalkForward", input.y > 0);
            _animator.SetBool("WalkBackward", input.y < 0);
            _animator.SetBool("WalkLeft", input.x < 0);
            _animator.SetBool("WalkRight", input.x > 0);
        }

        _animator.SetBool("IsWalking", isMoving && !isCrouching);
        _animator.SetBool("IsSprinting", isSprinting);
    }
}