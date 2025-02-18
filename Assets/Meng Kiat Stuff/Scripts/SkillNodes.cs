using UnityEngine;
using UnityEngine.UI;

public class SkillNode : MonoBehaviour
{
    [SerializeField] private SkillTree skillTree;
    [SerializeField] private Button skillButton;
    [SerializeField] private Image skillImage;
    [SerializeField] private SkillNode[] requiredSkills;
    [SerializeField] private Image line;

    private bool isUnlocked = false;

    public bool IsUnlocked => isUnlocked;

    private void Start()
    {
        if (line != null)
        {
            line.gameObject.SetActive(false);
        }
        skillButton.onClick.AddListener(UnlockSkill);
        UpdateUI();
    }

    private void UnlockSkill()
    {
        if (CanUnlock() && skillTree.UseSkillPoint())
        {
            isUnlocked = true;
            ApplyUpgrade();
            if (line != null)
            {
                line.gameObject.SetActive(true);
            }

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

        foreach (SkillNode skill in requiredSkills)
        {
            if (skill == null) continue;

            if (!skill.IsUnlocked)
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyUpgrade()
    {
        if (gameObject.name.Contains("Health")) skillTree.UpgradeHealth();
        if (gameObject.name.Contains("Stamina")) skillTree.UpgradeStamina();
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


}
