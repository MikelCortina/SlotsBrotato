using TMPro;
using UnityEngine;

public class SymbolInventoryUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI inventoryText;

    void OnEnable()
    {
        Refresh();
    }

    void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (inventoryText == null) return;
        if (RunConfig.Instance == null) return;

        if (RunConfig.Instance.selectedSymbols.Count == 0)
        {
            inventoryText.text = "Inventario:\nVacío";
            return;
        }

        string text = "Inventario de símbolos:\n";

        foreach (var symbol in RunConfig.Instance.selectedSymbols)
        {
            if (symbol == null) continue;

            int level = RunConfig.Instance.GetSymbolLevel(symbol.symbolType);
            text += $"- {symbol.symbolName} Lv.{level}\n";
        }

        inventoryText.text = text;
    }
}