using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Money")]
    public int coins;

    [Header("UI")]
    public TextMeshProUGUI coinsText;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinsText)
            coinsText.text = $"Monedas: {coins}";
    }
}