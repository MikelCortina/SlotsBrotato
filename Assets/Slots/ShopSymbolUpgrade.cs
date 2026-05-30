using UnityEngine;
using TMPro;

public class ShopSymbolUpgrade : MonoBehaviour
{
    public SlotSymbolType symbolType;
    public int cost = 25;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;

    void Start()
    {
        UpdateUI();
    }

    public void Buy()
    {
        if (RunConfig.Instance == null) return;
        if (PlayerWallet.Instance == null) return;

        if (!PlayerWallet.Instance.SpendCoins(cost))
            return;

        RunConfig.Instance.UpgradeSymbol(symbolType);

        UpdateUI();
    }

    void UpdateUI()
    {
        int level = RunConfig.Instance != null
            ? RunConfig.Instance.GetSymbolLevel(symbolType)
            : 1;

        if (titleText)
            titleText.text = $"Mejorar {symbolType} Lv.{level}";

        if (costText)
            costText.text = $"{cost}G";
    }
}