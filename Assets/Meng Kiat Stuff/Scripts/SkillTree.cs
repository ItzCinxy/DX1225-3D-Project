using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints = 100;
    [SerializeField] private PlayerStats playerStats;
    public bool UseSkillPoint()
    {
        if (skillPoints > 0)
        {
            skillPoints--;
            return true;
        }
        return false;
    }

    public void UpgradeStamina() => playerStats.IncreaseMaxStamina(5);
    public void UpgradeHealth() => playerStats.IncreaseMaxHealth(20);
    public void UpgradeHealthRegen() => playerStats.IncreaseHealthRegen(0.002f);

    public int GetSkillPoints() => skillPoints;
}
