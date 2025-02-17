using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActions;

    private Vector2 inputDirection;

    void Start()
    {
        _inputActions = _playerInput.actions;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        RotateWithCamera();
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
        _animator.SetBool("IsWalking", isMoving);

        if (isMoving)
        {
            float angle = Vector3.SignedAngle(cameraForward, moveDirection, Vector3.up);

            PlaySimpleDirectionalAnimation(angle);

            _characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            _animator.SetBool("IsWalking", false);
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
