using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    private Slider slider;
    private float timeUntilBarIsHidden = 0;
    private Coroutine healthBarLerpCoroutine; // Reference to running coroutine
    private Transform activeCamera; // Reference to currently active camera

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogError("Slider Component Missing in UIEnemyHealthBar!");
        }

        // Set the initial active camera
        UpdateActiveCamera();
    }

    public void SetHealth(int health)
    {


        if (healthBarLerpCoroutine != null)
        {
            StopCoroutine(healthBarLerpCoroutine); // Stop any ongoing HP update
        }

        healthBarLerpCoroutine = StartCoroutine(SmoothHealthBarUpdate(health));
        timeUntilBarIsHidden = 5; // Updated hide timer to 5 seconds
    }

    private IEnumerator SmoothHealthBarUpdate(int targetHealth)
    {
        float elapsedTime = 0f;
        float duration = 0.5f; // Duration of smooth effect
        float startValue = slider.value;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetHealth, elapsedTime / duration);
            yield return null;
        }

        slider.value = targetHealth; // Ensure final value is accurate
    }

    public void SetMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    private void Update()
    {
        // Ensure we have the correct active camera
        UpdateActiveCamera();

        // Rotate health bar to always face the active camera
        if (activeCamera != null)
        {
            transform.LookAt(transform.position + activeCamera.forward);
        }

        // Handle visibility timer
        timeUntilBarIsHidden -= Time.deltaTime;

        if (slider != null)
        {
            if (timeUntilBarIsHidden <= 0)
            {
                timeUntilBarIsHidden = 0;
                slider.gameObject.SetActive(false);
            }
            else
            {
                if (!slider.gameObject.activeInHierarchy)
                {
                    slider.gameObject.SetActive(true);
                }
            }

            if (slider.value <= 0)
            {
                Destroy(slider.gameObject);
            }
        }
    }

    private void UpdateActiveCamera()
    {
        // Find all active cameras in the scene
        Camera[] cameras = Camera.allCameras;

        foreach (Camera cam in cameras)
        {
            if (cam.isActiveAndEnabled)
            {
                activeCamera = cam.transform; // Use the currently active camera
                return;
            }
        }

        // Fallback if no active camera found
        if (activeCamera == null)
        {
            Debug.LogWarning("No active camera found for UIEnemyHealthBar!");
        }
    }
}
