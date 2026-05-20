using UnityEngine;
using TMPro;

public class PlayerShield : MonoBehaviour
{
    public int shields;

    [Header("UI")]
    public TextMeshProUGUI shieldText;

    void Start()
    {
        UpdateUI();
    }

    public void AddShield(int amount)
    {
        shields += amount;
        UpdateUI();
    }

    public bool TryBlockDamage()
    {
        if (shields <= 0) return false;

        shields--;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (shieldText)
            shieldText.text = $"Escudo: {shields}";
    }
}