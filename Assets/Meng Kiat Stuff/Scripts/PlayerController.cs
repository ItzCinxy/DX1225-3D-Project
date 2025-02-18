using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f; // Sprint speed
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.3f; // Distance for ground check
    [SerializeField] private LayerMask groundLayer; // Ensure this is set in the Inspector

    [SerializeField] private PlayerInput _playerInput;

    private InputActionAsset _inputActions;
    private Vector2 inputDirection;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;
    private float jumpTimer;
    private bool jumped;

    private void Start()
    {
        _inputActions = _playerInput.actions;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        RotateWithCamera();
    }

    private void HandleGroundCheck()
    {
        // Raycast slightly below the character to detect ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f; // Ensure player sticks to the ground
        }
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

        bool isMoving = moveDirection.magnitude > 0;
        bool isMovingForward = inputDirection.y > 0; // Only sprint when moving forward

        // Sprinting Logic
        isSprinting = _inputActions["Sprint"].IsPressed() && isMovingForward && isGrounded;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Ensure IsWalking remains true while sprinting
        _animator.SetBool("IsWalking", isMoving);
        _animator.SetBool("IsSprinting", isSprinting);

        if (isMoving)
        {
            float angle = Vector3.SignedAngle(cameraForward, moveDirection, Vector3.up);
            PlaySimpleDirectionalAnimation(angle);
        }

        // Apply both movement and jump force
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y; // Preserve jump velocity
        _characterController.Move(finalMove * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleJump()
    {
        if (!jumped && _inputActions["Jump"].WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Jump calculation
            _animator.SetBool("IsJumping", true);
            jumped = true;
        }

        // Reset jumping state when back on the ground
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

    private void PlaySimpleDirectionalAnimation(float angle)
    {
        _animator.SetBool("WalkForward", false);
        _animator.SetBool("WalkBackward", false);
        _animator.SetBool("WalkLeft", false);
        _animator.SetBool("WalkRight", false);

        if (angle >= -45f && angle <= 45f)
        {
            _animator.SetBool("WalkForward", true);
        }
        else if (angle > 45f && angle <= 135f)
        {
            _animator.SetBool("WalkRight", true);
        }
        else if (angle < -45f && angle >= -135f)
        {
            _animator.SetBool("WalkLeft", true);
        }
        else
        {
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
}
