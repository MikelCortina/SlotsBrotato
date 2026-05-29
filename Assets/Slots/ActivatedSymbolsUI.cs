using TMPro;
using UnityEngine;

public class ActivatedSymbolsUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI activatedText;

    void Update()
    {
        if (activatedText == null) return;
        if (RunConfig.Instance == null) return;

        var symbols = RunConfig.Instance.activatedSymbols;

        if (symbols == null || symbols.Count == 0)
        {
            activatedText.text = "Símbolos activados: ninguno";
            return;
        }

        string text = "Símbolos activados:\n";

        foreach (var pair in symbols)
        {
            text += $"{pair.Key} x{pair.Value}\n";
        }

        activatedText.text = text;
    }
}