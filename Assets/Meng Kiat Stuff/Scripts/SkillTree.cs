using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints = 100;
    [SerializeField] private TMP_Text skillPointsText;
    [SerializeField] private PlayerStats playerStats;

    private void Start()
    {
        skillPointsText.text = "Skill Points: " + skillPoints;
    }

    public bool UseSkillPoint()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            UpdateSkillPointText();
            return true;
        }
        return false;
    }

    private void UpdateSkillPointText()
    {
        skillPointsText.text = "Skill Points: " + skillPoints;
    }

    public void UpgradeStamina() => playerStats.IncreaseMaxStamina(5);
    public void UpgradeHealth() => playerStats.IncreaseMaxHealth(20);
    public void UpgradeHealthRegen() => playerStats.IncreaseHealthRegen(0.002f);
    public void UpgradeStaminaRegen() => playerStats.IncreaseStaminaRegen(0.005f);
    public void UpgradeFireResistance() => playerStats.IncreaseFireResistance(1f);
    public int GetSkillPoints() => skillPoints;
}
