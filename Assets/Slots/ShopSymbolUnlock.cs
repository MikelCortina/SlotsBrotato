using TMPro;
using UnityEngine;

public class ShopSymbolUnlock : MonoBehaviour
{
    [Header("Symbol")]
    public SlotSymbolData symbolToUnlock;

    [Header("Cost")]
    public int cost = 30;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;

    private bool _bought;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (titleText && symbolToUnlock != null)
            titleText.text = $"Comprar {symbolToUnlock.symbolName}";

        if (costText)
            costText.text = $"{cost}G";
    }

    public void Buy()
    {
        if (_bought) return;

        if (RunConfig.Instance == null) return;
        if (PlayerWallet.Instance == null) return;

        if (!PlayerWallet.Instance.SpendCoins(cost))
            return;

        if (!RunConfig.Instance.selectedSymbols.Contains(symbolToUnlock))
        {
            RunConfig.Instance.selectedSymbols.Add(symbolToUnlock);

            Debug.Log($"Símbolo comprado: {symbolToUnlock.symbolName}");
        }

        _bought = true;
        gameObject.SetActive(false);
    }
}