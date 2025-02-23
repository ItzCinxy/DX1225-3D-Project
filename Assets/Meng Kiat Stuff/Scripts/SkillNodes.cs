using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SkillTree skillTree;
    [SerializeField] private Button skillButton;
    [SerializeField] private Image skillImage;
    [SerializeField] private SkillNode[] requiredSkills;
    [SerializeField] private SkillNode[] skillThatBlock;
    [SerializeField] private TMP_Text skillNameText;

    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    private void Start()
    {
        skillButton.onClick.AddListener(UnlockSkill);
        UpdateUI();

        skillNameText.text = "";
    }

    private void UnlockSkill()
    {
        if (CanUnlock() && skillTree.UseSkillPoint())
        {
            isUnlocked = true;
            ApplyUpgrade();

            SkillNode[] allSkills = FindObjectsOfType<SkillNode>();
            foreach (SkillNode skill in allSkills)
            {
                skill.UpdateUI();
            }
        }
    }

    private bool CanUnlock()
    {
        if (isUnlocked) return false;

        foreach (SkillNode skill in skillThatBlock)
        {
            if (skill != null && skill.IsUnlocked)
            {
                return false;
            }
        }

        foreach (SkillNode skill in requiredSkills)
        {
            if (skill != null && !skill.IsUnlocked)
            {
                return false;
            }
        }

        return true;
    }


    private void ApplyUpgrade()
    {
        if (gameObject.name.Contains("MaxHealth")) skillTree.UpgradeHealth();
        if (gameObject.name.Contains("MaxStamina")) skillTree.UpgradeStamina();
        if (gameObject.name.Contains("HealthRegen")) skillTree.UpgradeHealthRegen();
        if (gameObject.name.Contains("StaminaRegen")) skillTree.UpgradeStaminaRegen();
        if (gameObject.name.Contains("FireResistance")) skillTree.UpgradeFireResistance();

        // ?? Unlocking Active Abilities
        if (gameObject.name.Contains("UnlockPush")) skillTree.UnlockPush();
        if (gameObject.name.Contains("UnlockFrenzy")) skillTree.UnlockFrenzy();
    }


    public void UpdateUI()
    {
        bool canUnlock = CanUnlock();

        skillButton.interactable = canUnlock && !isUnlocked;

        if (isUnlocked)
        {
            skillImage.color = Color.white;
        }
        else
        {
            skillImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillNameText != null && CanUnlock())
        {
            skillNameText.text = GetSkillName();
            skillNameText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillNameText != null)
        {
            skillNameText.gameObject.SetActive(false);
        }
    }

    private string GetSkillName()
    {
        if (gameObject.name.Contains("MaxHealth")) return "Max HP Up";
        if (gameObject.name.Contains("MaxStamina")) return "Max Stamina Up";
        if (gameObject.name.Contains("HealthRegen")) return "Health Regen Up";
        if (gameObject.name.Contains("StaminaRegen")) return "Stamina Regen Up";
        if (gameObject.name.Contains("FireResistance")) return "Fire Resistance Up";
        if (gameObject.name.Contains("UnlockPush")) return "Unlock Push Ability";
        if (gameObject.name.Contains("UnlockFrenzy")) return "Unlock Frenzy Ability";
        return "Unknown Skill";
    }
}
