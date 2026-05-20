using System.Collections.Generic;
using UnityEngine;

public class RunConfig : MonoBehaviour
{
    public static RunConfig Instance { get; private set; }

    public List<SlotSymbolData> selectedSymbols = new List<SlotSymbolData>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddSymbol(SlotSymbolData symbol)
    {
        if (symbol == null) return;

        selectedSymbols.Add(symbol);
    }

    public void RemoveSymbol(SlotSymbolData symbol)
    {
        if (symbol == null) return;

        selectedSymbols.Remove(symbol);
    }

    public bool HasAtLeastOneSymbol()
    {
        return selectedSymbols.Count > 0;
    }
}