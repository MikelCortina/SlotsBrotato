using System.Collections.Generic;
using UnityEngine;

public class RunConfig : MonoBehaviour
{
    public static RunConfig Instance { get; private set; }

    [Header("Symbols")]
    public List<SlotSymbolData> selectedSymbols = new List<SlotSymbolData>();

    [Header("Weapon")]
    public WeaponData selectedWeapon;

    [Header("Symbol Levels")]
    public Dictionary<SlotSymbolType, int> symbolLevels =
    new Dictionary<SlotSymbolType, int>();

    [Header("Passives")]
    public List<PassiveData> selectedPassives = new List<PassiveData>();
    [Header("Activated Symbols")]
    public Dictionary<string, int> activatedSymbols =
    new Dictionary<string, int>();

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

    public void SelectWeapon(WeaponData weapon)
    {
        selectedWeapon = weapon;
    }

    public bool HasWeapon()
    {
        return selectedWeapon != null;
    }

    public void AddPassive(PassiveData passive)
    {
        if (passive == null) return;

        if (!selectedPassives.Contains(passive))
            selectedPassives.Add(passive);
    }

    public void RemovePassive(PassiveData passive)
    {
        if (passive == null) return;

        selectedPassives.Remove(passive);
    }

    public void RegisterActivatedSymbol(SlotSymbolData symbol)
    {
        if (symbol == null) return;

        string symbolName = symbol.symbolName;

        if (!activatedSymbols.ContainsKey(symbolName))
            activatedSymbols[symbolName] = 0;

        activatedSymbols[symbolName]++;
    }
    public int GetSymbolLevel(SlotSymbolType type)
    {
        if (!symbolLevels.ContainsKey(type))
            symbolLevels[type] = 1;

        return symbolLevels[type];
    }

    public void UpgradeSymbol(SlotSymbolType type)
    {
        if (!symbolLevels.ContainsKey(type))
            symbolLevels[type] = 1;

        symbolLevels[type]++;
    }
}