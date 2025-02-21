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

        // If any skill in skillThatBlock is unlocked, this skill CANNOT be unlocked
        foreach (SkillNode skill in skillThatBlock)
        {
            if (skill != null && skill.IsUnlocked)
            {
                return false; // Blocked from unlocking
            }
        }

        // If all required skills are NOT unlocked, this skill cannot be unlocked
        foreach (SkillNode skill in requiredSkills)
        {
            if (skill != null && !skill.IsUnlocked)
            {
                return false;
            }
        }

        return true; // Can unlock if no blocked skills are active and all required skills are unlocked
    }


    private void ApplyUpgrade()
    {
        if (gameObject.name.Contains("MaxHealth")) skillTree.UpgradeHealth();
        if (gameObject.name.Contains("MaxStamina")) skillTree.UpgradeStamina();
        if (gameObject.name.Contains("HealthRegen")) skillTree.UpgradeHealthRegen();
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
            skillImage.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillNameText != null && CanUnlock())
        {
            skillNameText.text = GetSkillName();
            skillNameText.gameObject.SetActive(true); // Show the text
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (skillNameText != null)
        {
            skillNameText.gameObject.SetActive(false); // Hide the text
        }
    }

    private string GetSkillName()
    {
        if (gameObject.name.Contains("MaxHealth")) return "Max HP Up";
        if (gameObject.name.Contains("MaxStamina")) return "Max Stamina Up";
        if (gameObject.name.Contains("HealthRegen")) return "Health Regen Up";
        return "Unknown Skill";
    }
}
