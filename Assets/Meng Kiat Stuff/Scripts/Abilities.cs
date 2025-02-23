using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Abilities : MonoBehaviour
{
    [Header("Push Ability Settings")]
    private bool canUsePush = false;
    [SerializeField] private float pushPower = 10f;
    [SerializeField] private float pushRadius = 5f;
    [SerializeField] private float pushCooldown = 5f;
    [SerializeField] private Image pushLoadingImage;
    [SerializeField] private TMP_Text pushCoolDownTimerText;

    [Header("Frenzy Ability Settings")]
    private bool canUseFrenzy = false;
    [SerializeField] private float frenzyDuration = 5f;
    [SerializeField] private float frenzyFireRateMultiplier = 3f;
    [SerializeField] private float frenzyReloadMultiplier = 2f;
    [SerializeField] private float frenzyCooldown = 5f;
    [SerializeField] private Image frenzyLoadingImage;
    [SerializeField] private TMP_Text frenzyCoolDownTimerText;
    private Coroutine frenzyCoroutine;

    [SerializeField] private LayerMask zombieLayer;
    [SerializeField] private WeaponHolder _weaponHolder;

    private void Start()
    {
        pushLoadingImage.fillAmount = 1;
        pushCoolDownTimerText.text = "";

        frenzyLoadingImage.fillAmount = 1;
        frenzyCoolDownTimerText.text = "";
    }

    public void ActivatePush()
    {
        if (!canUsePush) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushRadius, zombieLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 pushDirection = (hitCollider.transform.position - transform.position).normalized;
                rb.AddForce(pushDirection * pushPower, ForceMode.Impulse);
            }
        }

        Debug.Log("Push Activated! " + hitColliders.Length + " zombies affected.");

        StartCoroutine(PushCooldownRoutine());
    }

    private IEnumerator PushCooldownRoutine()
    {
        canUsePush = false;
        float cooldownRemaining = pushCooldown;
        pushLoadingImage.fillAmount = 1;

        while (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
            pushLoadingImage.fillAmount = cooldownRemaining / pushCooldown;
            pushCoolDownTimerText.text = Mathf.CeilToInt(cooldownRemaining).ToString();

            yield return null;
        }

        pushLoadingImage.fillAmount = 0;
        pushCoolDownTimerText.text = "";
        canUsePush = true;
    }

    public void ActivateFrenzyMode()
    {
        if (!canUseFrenzy) return;

        if (_weaponHolder == null) return;

        if (_weaponHolder.GetIsWeaponEquipped())
        {
            WeaponBase currentWeapon = _weaponHolder.GetEquippedWeapon();
            if (currentWeapon != null)
            {
                if (frenzyCoroutine != null)
                {
                    StopCoroutine(frenzyCoroutine);
                }
                frenzyCoroutine = StartCoroutine(FrenzyModeCoroutine(currentWeapon));
            }
        }
    }

    private IEnumerator FrenzyModeCoroutine(WeaponBase weapon)
    {
        canUseFrenzy = false;
        float originalFireRate = weapon.GetFireRate();
        float originalReloadTime = weapon.GetReloadTime();

        weapon.SetFireRate(originalFireRate / frenzyFireRateMultiplier);
        weapon.SetReloadTime(originalReloadTime / frenzyReloadMultiplier);

        frenzyLoadingImage.fillAmount = 1;

        yield return new WaitForSeconds(frenzyDuration);

        weapon.SetFireRate(originalFireRate);
        weapon.SetReloadTime(originalReloadTime);
        StartCoroutine(FrenzyCooldownRoutine());

        Debug.Log("Frenzy Mode Ended. Fire rate reset.");
    }

    private IEnumerator FrenzyCooldownRoutine()
    {
        float cooldownRemaining = frenzyCooldown;
        frenzyLoadingImage.fillAmount = 1;

        while (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
            frenzyLoadingImage.fillAmount = cooldownRemaining / frenzyCooldown;
            frenzyCoolDownTimerText.text = Mathf.CeilToInt(cooldownRemaining).ToString();

            yield return null;
        }

        frenzyLoadingImage.fillAmount = 0;
        frenzyCoolDownTimerText.text = "";
        canUseFrenzy = true;
    }

    public void EnablePush()
    {
        StartCoroutine(PushCooldownRoutine());
    }

    public void EnableFrenzy()
    {
        StartCoroutine(FrenzyCooldownRoutine());
    }
}
