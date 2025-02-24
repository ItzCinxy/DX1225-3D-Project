using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Key : MonoBehaviour
{
    [SerializeField] private LayerMask layer;
    [SerializeField] private InputActionReference pickUpAction;

    private void Update()
    {
        // Check for pickup input press
        if (pickUpAction.action.WasPressedThisFrame())
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        // Create a ray from the center of the screen (camera view)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 2f, layer))
        {
            ObjectiveManager.Instance.PickUpKey();
            Destroy(hitInfo.collider.gameObject);
            Debug.Log("Key picked up!");
        }
    }
}