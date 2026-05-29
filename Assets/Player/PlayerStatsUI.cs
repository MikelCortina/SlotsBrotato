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
            $"Daño actual: {playerStats.damage}\n" +
            $"Cadencia extra: {playerStats.fireRate}\n" +
            $"Crítico: {playerStats.critChance * 100f:0}%\n" +
            $"Velocidad: {playerStats.moveSpeed}\n" +
            $"Vida máxima: {playerStats.maxHealth}\n" +
            $"Reducción daño: {playerStats.damageReduction * 100f:0}%\n" +
            $"Regeneración: {playerStats.regeneration}";
    }
}