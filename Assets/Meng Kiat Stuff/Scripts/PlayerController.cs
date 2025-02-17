using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;

    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActions;

    [SerializeField] private CinemachineFreeLook _freelookCamera;

    public enum PlayerState
    {
        Idle,
        Walking
    }

    private PlayerState _currentState;

    // Start is called before the first frame update
    void Start()
    {
        _inputActions = _playerInput.actions;
        _currentState = PlayerState.Idle;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
        
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        bool isMoving = moveDirection.magnitude > 0;
        _animator.SetBool("IsWalking", isMoving);

        RotateWithCamera();

        if (moveDirection.magnitude > 0)
        {
            moveDirection = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 250f);
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
