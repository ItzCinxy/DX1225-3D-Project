using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private LayerMask layer;
    [SerializeField] private InputActionReference pickUpAction;

    private Dictionary<Transform, bool> doorStates = new Dictionary<Transform, bool>(); // Track open/closed state
    private Dictionary<Transform, Coroutine> activeRotations = new Dictionary<Transform, Coroutine>(); // Track active rotations

    private void Update()
    {
        if (pickUpAction.action.WasPressedThisFrame())
        {
            TryOpenDoor();
        }
    }

    private void TryOpenDoor()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 2f, layer))
        {
            Transform door = hitInfo.transform;
            Transform doorPivot = FindParentWithName(door, "Door Pivot") ?? door; // Use door if no pivot

            if (activeRotations.ContainsKey(doorPivot))
                return; // Ignore if already rotating

            bool isOpen = doorStates.ContainsKey(doorPivot) && doorStates[doorPivot];

            // Fixed rotation: Always add ±90 degrees
            float angle = isOpen ? -90f : 90f;

            Coroutine rotationCoroutine = StartCoroutine(RotateDoor(doorPivot, angle));
            activeRotations[doorPivot] = rotationCoroutine;
            doorStates[doorPivot] = !isOpen;
        }
    }

    private Transform FindParentWithName(Transform child, string name)
    {
        while (child != null)
        {
            if (child.name.Contains(name))
                return child;
            child = child.parent;
        }
        return null;
    }

    private IEnumerator RotateDoor(Transform doorTransform, float angle)
    {
        Quaternion startRotation = doorTransform.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, angle, 0);
        float elapsedTime = 0f;
        float duration = 1f;

        while (elapsedTime < duration)
        {
            doorTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
        activeRotations.Remove(doorTransform);
    }
}
