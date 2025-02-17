using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        originalColor = targetRenderer.material.color;
    }

    public void Hit()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FadeDamageColor());
    }

    private IEnumerator FadeDamageColor()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            targetRenderer.material.color = Color.Lerp(originalColor, damageColor, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetRenderer.material.color = damageColor;

        elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            targetRenderer.material.color = Color.Lerp(damageColor, originalColor, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetRenderer.material.color = originalColor;
    }
}
