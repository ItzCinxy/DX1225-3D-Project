using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIEnemyHealthBar : MonoBehaviour
{
    private Slider slider;
    private float timeUntilBarIsHidden = 0;
    private Coroutine healthBarLerpCoroutine; // Reference to running coroutine

    private void Awake()
    {
        slider = GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogError("Slider Component Missing in UIEnemyHealthBar!");
        }
    }

    public void SetHealth(int health)
    {
        Debug.Log($"Updating Enemy HP Bar: {health} / {slider.maxValue}");

        if (healthBarLerpCoroutine != null)
        {
            StopCoroutine(healthBarLerpCoroutine); // Stop any ongoing HP update
        }

        healthBarLerpCoroutine = StartCoroutine(SmoothHealthBarUpdate(health));
        timeUntilBarIsHidden = 3; // Reset hide timer
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
}
