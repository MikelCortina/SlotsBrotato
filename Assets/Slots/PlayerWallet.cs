using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Money")]
    [SerializeField] public int coins;
    public int Coins => coins;
    [Header("UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI shopCoinsText;

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

        if (shopCoinsText)
            shopCoinsText.text = $"Monedas: {coins}";
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
            return false;

        coins -= amount;
        UpdateUI();
        return true;
    }
}