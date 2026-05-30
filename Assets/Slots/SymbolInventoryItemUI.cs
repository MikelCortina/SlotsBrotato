using TMPro;
using UnityEngine;

public class SymbolInventoryItemUI : MonoBehaviour
{
    public SlotSymbolData symbol;
    public TextMeshProUGUI text;
    public int sellValue = 10;

    public void Refresh()
    {
        if (symbol == null) return;
        if (RunConfig.Instance == null) return;

        int level = RunConfig.Instance.GetSymbolLevel(symbol.symbolType);
        int finalSellValue = sellValue * level;

        text.text = $"{symbol.symbolName} Lv.{level} - Vender +{finalSellValue}G";
    }

    public void Sell()
    {
        if (RunConfig.Instance == null) return;

        if (RunConfig.Instance.selectedSymbols.Count <= 1)
        {
            Debug.Log("No puedes vender el último símbolo.");
            return;
        }

        int level = RunConfig.Instance.GetSymbolLevel(symbol.symbolType);
        int finalSellValue = sellValue * level;

        RunConfig.Instance.RemoveSymbol(symbol);

        PlayerWallet.Instance?.AddCoins(finalSellValue);

        ShopOfferUI[] offers = FindObjectsOfType<ShopOfferUI>();

        foreach (var offer in offers)
        {
            offer.RefreshIfSymbolChanged(symbol);
        }

        FindFirstObjectByType<SymbolInventoryListUI>()?.Refresh();

        Debug.Log($"Símbolo vendido: {symbol.symbolName}");

        Destroy(gameObject);
    }
}