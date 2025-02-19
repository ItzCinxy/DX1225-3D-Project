using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed; // Slower speed when crouching
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private PlayerInput _playerInput;

    [SerializeField] private CinemachineFreeLook _freelookCamera;
    [SerializeField] private Transform _standLookAt;
    [SerializeField] private Transform _crouchLookAt;

    [SerializeField] private Canvas _skillTreeCanvas;
    private bool isSkillTreeOpen;

    private InputActionAsset _inputActions;
    private Vector2 inputDirection;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;
    private float jumpTimer;
    private bool jumped;

    private void Start()
    {
        _inputActions = _playerInput.actions;

        sprintSpeed = moveSpeed * 1.5f;
        crouchSpeed = moveSpeed * 0.5f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (_skillTreeCanvas != null)
        {
            _skillTreeCanvas.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_inputActions["ToggleSkillTree"].WasPressedThisFrame())
        {
            isSkillTreeOpen = !isSkillTreeOpen;
        }

        if (isSkillTreeOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _skillTreeCanvas.gameObject.SetActive(true);
            _freelookCamera.gameObject.SetActive(false);

            velocity = Vector3.zero;
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsSprinting", false);
            _animator.SetBool("IsCrouching", false);
            ResetMovementAnimations(); // Reset directional movement
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _skillTreeCanvas.gameObject.SetActive(false);
            _freelookCamera.gameObject.SetActive(true);

            _animator.speed = 1;
        }

        if (isSkillTreeOpen) return;

        HandleGroundCheck();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        RotateWithCamera();
        CinemachineLookAt();

        // **Reset animations when NOT moving**
        if (!isMoving)
        {
            ResetMovementAnimations();
        }
    }

    private void CinemachineLookAt()
    {
        if (_freelookCamera == null) return; // Ensure camera exists

        if (!isCrouching)
        {
            _freelookCamera.LookAt = _standLookAt; // Look at standing target
            _freelookCamera.Follow = _standLookAt;
        }
        else
        {
            _freelookCamera.LookAt = _crouchLookAt; // Look at crouching target
            _freelookCamera.Follow = _crouchLookAt;
        }
    }


    private void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f; // Stick to ground
        }
    }

    private void HandleCrouch()
    {
        isCrouching = _inputActions["Crouch"].IsPressed(); // Hold to crouch

        if (isCrouching)
        {
            isSprinting = false; // Disable sprinting if crouching
        }

        _animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleMovement()
    {
        inputDirection = _inputActions["Move"].ReadValue<Vector2>();

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * inputDirection.y + cameraRight * inputDirection.x).normalized;

        isMoving = moveDirection.magnitude > 0;
        bool isMovingForward = inputDirection.y > 0;

        // Sprinting Logic (Disabled if crouching)
        isSprinting = _inputActions["Sprint"].IsPressed() && isMovingForward && isGrounded && !isCrouching;

        // Set movement speed based on state
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : moveSpeed);

        // Update animator parameters
        _animator.SetBool("IsWalking", isMoving && !isCrouching); // Walking only when not crouching
        _animator.SetBool("IsSprinting", isSprinting);
        _animator.SetBool("IsCrouching", isCrouching);

        if (isMoving)
        {
            float angle = Vector3.SignedAngle(cameraForward, moveDirection, Vector3.up);
            PlayDirectionalAnimation(angle);
        }

        // Apply movement
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        _characterController.Move(finalMove * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
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

    private void PlayDirectionalAnimation(float angle)
    {
        // **Reset all movement animations before setting new one**
        ResetMovementAnimations();

        if (isCrouching)
        {
            if (angle >= -45f && angle <= 45f)
                _animator.SetBool("CrouchForward", true);
            else if (angle > 45f && angle <= 135f)
                _animator.SetBool("CrouchRight", true);
            else if (angle < -45f && angle >= -135f)
                _animator.SetBool("CrouchLeft", true);
            else
                _animator.SetBool("CrouchBackward", true);
        }
        else
        {
            if (angle >= -45f && angle <= 45f)
                _animator.SetBool("WalkForward", true);
            else if (angle > 45f && angle <= 135f)
                _animator.SetBool("WalkRight", true);
            else if (angle < -45f && angle >= -135f)
                _animator.SetBool("WalkLeft", true);
            else
                _animator.SetBool("WalkBackward", true);
        }
    }

    private void RotateWithCamera()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;

        if (cameraForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 500f);
        }
    }

    private void ResetMovementAnimations()
    {
        _animator.SetBool("WalkForward", false);
        _animator.SetBool("WalkBackward", false);
        _animator.SetBool("WalkLeft", false);
        _animator.SetBool("WalkRight", false);
        _animator.SetBool("CrouchForward", false);
        _animator.SetBool("CrouchBackward", false);
        _animator.SetBool("CrouchLeft", false);
        _animator.SetBool("CrouchRight", false);
    }
}
