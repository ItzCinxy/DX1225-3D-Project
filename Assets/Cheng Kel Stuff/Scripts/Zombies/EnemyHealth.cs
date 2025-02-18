using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;

    private UIEnemyHealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<UIEnemyHealthBar>();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Remaining HP: {currentHealth}");

        //SoundManager.Instance.enemyChannel.PlayOneShot(SoundManager.Instance.enemyHurt);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
    }

    private IEnumerator DieWithDelay()
    {
        yield return new WaitForSeconds(0.5f); // Small delay to let attack sound finish

        Debug.Log($"{gameObject.name} died!");

        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        //SoundManager.Instance.enemyChannel.PlayOneShot(SoundManager.Instance.enemyDie);

        Destroy(gameObject);
    }
}
