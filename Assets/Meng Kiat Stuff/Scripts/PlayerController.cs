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
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private Abilities _abilities;

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

    [Header("Camera Bob")]
    [SerializeField] private Transform firstPersonCameraTransform;
    [SerializeField] private float bobSpeed = 6f;
    [SerializeField] private float bobAmount = 0.05f;

    private float defaultCameraY;
    private float bobTimer;

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

    private float knockbackTimer = 0f;
    private float knockbackDuration = 2f;

    private void Start()
    {
        _inputActions = _playerInput.actions;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _skillTreeCanvas?.gameObject.SetActive(false);

        if (firstPersonCameraTransform != null)
        {
            defaultCameraY = firstPersonCameraTransform.localPosition.y;
        }
    }

    private void Update()
    {
        HandleSkillTreeToggle();
        HandleCameraToggle();
        HandleGamePause();

        if (isSkillTreeOpen) return;

        HandleGroundCheck();
        HandleCrouch();

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime; // Countdown knockback duration

            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 5f);
        }
        else
        {
            HandleMovement(); // Allow normal movement again
        }

        HandleJump();
        RotateWithCamera();
        HandleAbilities();

        if (isFirstPerson)
        {
            HandleCameraBob();
        }

        velocity.y += gravity * 2 * Time.deltaTime;
        _characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleAbilities()
    {
        if (_inputActions["Ability1"].WasPressedThisFrame())
        {
            _abilities.ActivatePush();
        }

        if (_inputActions["Ability2"].WasPressedThisFrame())
        {
            _abilities.ActivateFrenzyMode();
        }
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
            Cursor.visible = true;
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
            _skillTreeCanvas?.gameObject.SetActive(false);
        }

        //Debug.Log($"SkillTree: {isSkillTreeOpen}, AnyPanelActive: {isAnyPanelActive}");
    }

    private void UpdateCameraView()
    {
        if (isFirstPerson)
        {
            _firstPersonCamera.Priority = 20;
            _thirdPersonCamera.Priority = 10;
            _skin.gameObject.SetActive(false);
        }
        else
        {
            _firstPersonCamera.Priority = 10;
            _thirdPersonCamera.Priority = 20;
            _skin.gameObject.SetActive(true);
        }
    }

    private void HandleCameraBob()
    {
        // Only bob in first-person and when grounded
        if (!isFirstPerson || !isGrounded || firstPersonCameraTransform == null)
        {
            bobTimer = 0f;
            firstPersonCameraTransform.localPosition = new Vector3(
                firstPersonCameraTransform.localPosition.x,
                Mathf.Lerp(firstPersonCameraTransform.localPosition.y, defaultCameraY, Time.deltaTime * 10f),
                firstPersonCameraTransform.localPosition.z
            );
            return;
        }

        // Get movement input magnitude
        float moveInput = inputDirection.magnitude;

        // Stop bobbing if no movement
        if (moveInput < 0.1f)
        {
            bobTimer = 0f;
            firstPersonCameraTransform.localPosition = new Vector3(
                firstPersonCameraTransform.localPosition.x,
                Mathf.Lerp(firstPersonCameraTransform.localPosition.y, defaultCameraY, Time.deltaTime * 10f),
                firstPersonCameraTransform.localPosition.z
            );
            return;
        }

        // Determine current movement speed based on sprint/crouch
        float currentSpeed = moveSpeed;
        if (isSprinting) currentSpeed *= sprintSpeedMultiplier;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;

        // Scale bobbing intensity with speed
        float speedFactor = currentSpeed / moveSpeed;
        float bobFrequency = bobSpeed * speedFactor; // Faster bobbing when sprinting
        float bobAmplitude = bobAmount * speedFactor; // Higher bob when moving faster

        // Bobbing effect using sine wave
        bobTimer += Time.deltaTime * bobFrequency;
        float verticalOffset = Mathf.Sin(bobTimer) * bobAmplitude;

        // Apply to camera position
        firstPersonCameraTransform.localPosition = new Vector3(
            firstPersonCameraTransform.localPosition.x,
            defaultCameraY + verticalOffset,
            firstPersonCameraTransform.localPosition.z
        );
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
            targetShoulderOffset = new Vector3(0.2f, -1f, 0f);
        }
        else
        {
            _characterController.height = 1.7f;
            _characterController.center = new Vector3(0, 1.1f, 0);
            targetShoulderOffset = new Vector3(0.2f, -0.5f, 0f);
        }

        Cinemachine3rdPersonFollow followComponent = _thirdPersonCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (followComponent != null)
        {
            followComponent.ShoulderOffset = Vector3.Lerp(followComponent.ShoulderOffset, targetShoulderOffset, Time.deltaTime * 5f);
        }

        Cinemachine3rdPersonFollow followComponent2 = _firstPersonCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (followComponent2 != null)
        {
            followComponent2.ShoulderOffset = Vector3.Lerp(followComponent2.ShoulderOffset, targetShoulderOffset, Time.deltaTime * 5f);
        }

        _animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleMovement()
    {
        inputDirection = _inputActions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = CalculateMoveDirection();
        isMoving = moveDirection.magnitude > 0;

        if (_playerStats.GetCurrentStamina() > 0)
        {
            isSprinting = _inputActions["Sprint"].IsPressed() && inputDirection.y > 0 && isGrounded && !isCrouching;
        }
        else
        {
            isSprinting = false;
        }

        float currentSpeed = moveSpeed;

        if (isCrouching)
        {
            currentSpeed *= crouchSpeedMultiplier;
        }
        else if (isSprinting)
        {
            currentSpeed *= sprintSpeedMultiplier;
            _playerStats.UseStamina(0.1f);
        }

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
            SoundManager.Instance.PlaySFX(SoundManager.Instance.playerJump);
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

    public bool GetIsSprinting()
    {
        return isSprinting;
    }

    public void ApplyKnockback(Vector3 enemyPosition, float knockbackForce, float upwardForce = 1f)
    {
        // Calculate knockback direction away from enemy
        Vector3 knockbackDirection = (transform.position - enemyPosition).normalized;

        // Apply knockback velocity
        velocity = knockbackDirection * knockbackForce + Vector3.up * upwardForce;

        // Start knockback timer
        knockbackTimer = knockbackDuration;

        Debug.Log("Player knocked back!");
    }

    public bool GetIsFirstPerson()
    {
        return isFirstPerson;
    }
}