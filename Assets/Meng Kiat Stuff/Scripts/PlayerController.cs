using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Canvas _skillTreeCanvas;
    [SerializeField] private Transform _raycastStart;

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

            velocity = Vector3.zero;
            _animator.SetBool("IsWalking", false);
            _animator.SetBool("IsSprinting", false);
            _animator.SetBool("IsCrouching", false);
            ResetMovementAnimations();
            return;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _skillTreeCanvas.gameObject.SetActive(false);
        }

        HandleGroundCheck();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        RotateWithCamera();

        if (!isMoving)
        {
            ResetMovementAnimations();
        }
    }

    private void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(_raycastStart.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }
    }

    private void HandleCrouch()
    {
        isCrouching = _inputActions["Crouch"].IsPressed();

        if (isCrouching)
        {
            isSprinting = false;
        }

        _animator.SetBool("IsCrouching", isCrouching);
    }

    private void HandleMovement()
    {
        inputDirection = _inputActions["Move"].ReadValue<Vector2>();

        // ✅ Get camera forward and right directions
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // ✅ Movement is relative to the camera's rotation
        Vector3 moveDirection = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
        moveDirection.Normalize();

        isMoving = moveDirection.magnitude > 0;
        bool isMovingForward = inputDirection.y > 0;
        bool isMovingBackward = inputDirection.y < 0;
        bool isMovingLeft = inputDirection.x < 0;
        bool isMovingRight = inputDirection.x > 0;

        isSprinting = _inputActions["Sprint"].IsPressed() && isMovingForward && isGrounded && !isCrouching;

        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : moveSpeed);

        // ✅ NO ROTATION ON MOVEMENT (Camera controls rotation)
        // Player only moves, it does NOT rotate itself

        // ✅ Set Movement Animations
        ResetMovementAnimations();

        if (isCrouching)
        {
            if (isMovingForward) _animator.SetBool("CrouchForward", true);
            if (isMovingBackward) _animator.SetBool("CrouchBackward", true);
            if (isMovingLeft) _animator.SetBool("CrouchLeft", true);
            if (isMovingRight) _animator.SetBool("CrouchRight", true);
        }
        else
        {
            if (isMovingForward) _animator.SetBool("WalkForward", true);
            if (isMovingBackward) _animator.SetBool("WalkBackward", true);
            if (isMovingLeft) _animator.SetBool("WalkLeft", true);
            if (isMovingRight) _animator.SetBool("WalkRight", true);
        }

        _animator.SetBool("IsWalking", isMoving && !isCrouching);
        _animator.SetBool("IsSprinting", isSprinting);
        _animator.SetBool("IsCrouching", isCrouching);

        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        _characterController.Move(finalMove * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
    }

    private void RotateWithCamera()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        if (cameraForward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = targetRotation; // ✅ Instantly align with camera
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
