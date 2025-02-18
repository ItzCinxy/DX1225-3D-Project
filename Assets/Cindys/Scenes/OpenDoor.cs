using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class OpenDoor : MonoBehaviour
{
    [SerializeField] private LayerMask layer;
    [SerializeField] private InputActionReference pickUpAction;

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
            Transform doorPivot = FindParentWithName(door, "Door Pivot");

            if (doorPivot != null)
            {
                Debug.Log($"Rotating DoorPivot: {doorPivot.name}");
                StartCoroutine(RotateDoor(doorPivot));
            }
            else
            {
                Debug.Log($"No DoorPivot found. Rotating Door: {door.name}");
                StartCoroutine(RotateDoor(door)); // Rotate the door directly
            }
        }
    }

    /// <summary>
    /// Recursively searches for a parent object with a specific name.
    /// </summary>
    private Transform FindParentWithName(Transform child, string name)
    {
        while (child != null)
        {
            if (child.name.Contains(name))
            {
                return child;
            }
            child = child.parent;
        }
        return null;
    }


    private IEnumerator RotateDoor(Transform doorTransform)
    {
        Quaternion startRotation = doorTransform.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 90f, 0);
        float elapsedTime = 0f;
        float duration = 1f; // Adjust rotation speed

        while (elapsedTime < duration)
        {
            doorTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
    }

}

