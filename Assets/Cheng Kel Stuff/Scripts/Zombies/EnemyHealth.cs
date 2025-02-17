using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    [SerializeField] private int damageAmount = 50; // Damage taken when pressing F

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(damageAmount);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            StartCoroutine(DieWithDelay());
        }
    }

    private IEnumerator DieWithDelay()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"{gameObject.name} has died!");
        Destroy(gameObject);
    }
}
