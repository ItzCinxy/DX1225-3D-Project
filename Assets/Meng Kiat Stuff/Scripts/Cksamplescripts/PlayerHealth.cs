using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("UI Settings")]
    [SerializeField] private Slider healthBar;

    private Coroutine healthBarLerpCoroutine;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogError("Player Health Bar is not assigned in the Inspector!");
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Prevents negative HP

        //SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerHurt);

        Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");

        if (healthBar != null)
        {
            if (healthBarLerpCoroutine != null)
            {
                StopCoroutine(healthBarLerpCoroutine); // Stop any ongoing HP update
            }
            healthBarLerpCoroutine = StartCoroutine(SmoothHealthBarUpdate());
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't exceed max

        Debug.Log($"Player healed by {amount}! Current health: {currentHealth}");

        if (healthBar != null)
        {
            if (healthBarLerpCoroutine != null)
            {
                StopCoroutine(healthBarLerpCoroutine);
            }
            healthBarLerpCoroutine = StartCoroutine(SmoothHealthBarUpdate());
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private IEnumerator SmoothHealthBarUpdate()
    {
        float elapsedTime = 0f;
        float duration = 0.5f;
        float startValue = healthBar.value;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.value = Mathf.Lerp(startValue, currentHealth, elapsedTime / duration);
            yield return null;
        }

        healthBar.value = currentHealth;
    }

    private IEnumerator DieWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Player died!");
        //SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerDie);

        Destroy(gameObject);
    }
}