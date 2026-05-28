using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RunSymbolsDebugUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI symbolsText;

    void Update()
    {
        if (symbolsText == null) return;
        if (RunConfig.Instance == null) return;

        List<SlotSymbolData> symbols = RunConfig.Instance.selectedSymbols;

        if (symbols == null || symbols.Count == 0)
        {
            symbolsText.text = "Símbolos: ninguno";
            return;
        }

        string text = "Símbolos elegidos:\n";

        foreach (var symbol in symbols)
        {
            if (symbol == null) continue;

            text += $"- {symbol.symbolName}\n";
        }

        symbolsText.text = text;
    }
}