using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    [SerializeField] private float currentStamina;

    [Header("UI Settings")]
    [SerializeField] private Slider staminaBar;

    private Coroutine staminaBarLerpCoroutine;

    void Start()
    {
        currentStamina = maxStamina;

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
        else
        {
            Debug.LogError("Player Stamina Bar is not assigned in the Inspector!");
        }
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // Prevents negative stamina

        Debug.Log($"Player used {amount} stamina! Current stamina: {currentStamina}");

        if (staminaBar != null)
        {
            if (staminaBarLerpCoroutine != null)
            {
                StopCoroutine(staminaBarLerpCoroutine); // Stop any ongoing update
            }
            staminaBarLerpCoroutine = StartCoroutine(SmoothStaminaBarUpdate());
        }

        if (currentStamina <= 0)
        {
            StartCoroutine(ExhaustWithDelay());
        }
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // Ensure stamina doesn't exceed max

        Debug.Log($"Player recovered {amount} stamina! Current stamina: {currentStamina}");

        if (staminaBar != null)
        {
            if (staminaBarLerpCoroutine != null)
            {
                StopCoroutine(staminaBarLerpCoroutine);
            }
            staminaBarLerpCoroutine = StartCoroutine(SmoothStaminaBarUpdate());
        }
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    private IEnumerator SmoothStaminaBarUpdate()
    {
        float elapsedTime = 0f;
        float duration = 0.5f;
        float startValue = staminaBar.value;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            staminaBar.value = Mathf.Lerp(startValue, currentStamina, elapsedTime / duration);
            yield return null;
        }

        staminaBar.value = currentStamina;
    }

    private IEnumerator ExhaustWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Player is exhausted!");
        // Implement exhaustion behavior here (e.g., disable sprinting)
    }
}