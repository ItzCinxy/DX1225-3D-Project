using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [Header("Health Settings")]
    private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Stamina Settings")]
    private float baseStaminaRegen = 0.01f;
    private float maxStamina = 100f;
    [SerializeField] private float currentStamina;

    [Header("Fire Resistance Settings")]
    private float fireResistance = 0f;

    [Header("Regen Stuff")]
    private float healthRegenSpeed = 0f;
    private float staminaRegenSpeed = 0f;

    [Header("UI Settings")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private TMP_Text staminaText;

    private Coroutine healthBarLerpCoroutine;
    private Coroutine staminaBarLerpCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }

        // Update text on start
        UpdateUIText();
    }

    private void Update()
    {
        PassiveRegen();
    }

    private void PassiveRegen()
    {
        if (!_playerController.GetIsSprinting())
        {
            RecoverStamina(staminaRegenSpeed + baseStaminaRegen);
        }

        Heal(healthRegenSpeed);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        UpdateUIText();

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        UpdateUIText();
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        UpdateStaminaBar();
        UpdateUIText();

        if (currentStamina <= 0)
        {
            StartCoroutine(ExhaustWithDelay());
        }
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        UpdateStaminaBar();
        UpdateUIText();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            if (healthBarLerpCoroutine != null)
            {
                StopCoroutine(healthBarLerpCoroutine);
            }
            healthBarLerpCoroutine = StartCoroutine(SmoothHealthBarUpdate());
        }
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            if (staminaBarLerpCoroutine != null)
            {
                StopCoroutine(staminaBarLerpCoroutine);
            }
            staminaBarLerpCoroutine = StartCoroutine(SmoothStaminaBarUpdate());
        }
    }

    private void UpdateUIText()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }

        if (staminaText != null)
        {
            staminaText.text = $"{Mathf.RoundToInt(currentStamina)} / {Mathf.RoundToInt(maxStamina)}";
        }
    }

    private IEnumerator SmoothHealthBarUpdate()
    {
        float elapsedTime = 0f;
        float duration = 0.3f;
        float startValue = healthBar.value;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.value = Mathf.Lerp(startValue, currentHealth, elapsedTime / duration);
            yield return null;
        }

        healthBar.value = currentHealth;
    }

    private IEnumerator SmoothStaminaBarUpdate()
    {
        float elapsedTime = 0f;
        float duration = 0.3f;
        float startValue = staminaBar.value;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            staminaBar.value = Mathf.Lerp(startValue, currentStamina, elapsedTime / duration);
            yield return null;
        }

        staminaBar.value = currentStamina;
    }

    private IEnumerator DieWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Player died!");
        Destroy(gameObject);
    }
    private IEnumerator ExhaustWithDelay()
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log("Player is exhausted!");
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public void IncreaseMaxHealth(float amountToIncrease)
    {
        maxHealth += amountToIncrease;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        UpdateUIText();
    }

    public void IncreaseMaxStamina(float amountToIncrease)
    {
        maxStamina += amountToIncrease;

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }

        UpdateUIText();
    }

    public void IncreaseHealthRegen(float amountToIncrease)
    {
        healthRegenSpeed += amountToIncrease;
    }

    public void IncreaseStaminaRegen(float amountToIncrease)
    {
        staminaRegenSpeed += amountToIncrease;
    }

    public void IncreaseFireResistance(float amountToIncrease)
    {
        fireResistance += amountToIncrease;
    }
}
