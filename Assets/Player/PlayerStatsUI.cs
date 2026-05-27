using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    [Header("UI")]
    public TextMeshProUGUI statsText;

    void Update()
    {
        if (playerStats == null || statsText == null) return;

        statsText.text =
            $"DMG: {playerStats.damage}\n" +
            $"FIRE RATE: {playerStats.fireRate}\n" +
            $"CRIT: {playerStats.critChance * 100f}%\n" +
            $"MOVE: {playerStats.moveSpeed}\n" +
            $"MAX HP: {playerStats.maxHealth}\n" +
            $"REDUCTION: {playerStats.damageReduction * 100f}%\n" +
            $"REGEN: {playerStats.regeneration}";
    }
}