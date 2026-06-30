using System.Collections.Generic;
using UnityEngine;

public class SymbolUnlockManager : MonoBehaviour
{
    public static SymbolUnlockManager Instance { get; private set; }

    [Header("Symbols")]
    public SlotSymbolData[] allSymbols;

    private const string SaveKeyPrefix = "UnlockedSymbol_";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UnlockStartingSymbols();
    }

    void UnlockStartingSymbols()
    {
        if (allSymbols == null) return;

        foreach (SlotSymbolData symbol in allSymbols)
        {
            if (symbol == null) continue;

            if (symbol.unlockWave <= 1)
                UnlockSymbol(symbol);
        }
    }

    public bool IsUnlocked(SlotSymbolData symbol)
    {
        if (symbol == null) return false;

        return PlayerPrefs.GetInt(GetSaveKey(symbol), 0) == 1;
    }

    public void UnlockSymbol(SlotSymbolData symbol)
    {
        if (symbol == null) return;

        PlayerPrefs.SetInt(GetSaveKey(symbol), 1);
        PlayerPrefs.Save();
    }

    public void UnlockSymbolsUpToWave(int reachedWave)
    {
        if (allSymbols == null) return;

        foreach (SlotSymbolData symbol in allSymbols)
        {
            if (symbol == null) continue;

            if (reachedWave >= symbol.unlockWave)
                UnlockSymbol(symbol);
        }
    }

    string GetSaveKey(SlotSymbolData symbol)
    {
        return SaveKeyPrefix + symbol.symbolType.ToString();
    }

    public void ResetUnlocks()
    {
        if (allSymbols == null) return;

        foreach (SlotSymbolData symbol in allSymbols)
        {
            if (symbol == null) continue;

            PlayerPrefs.DeleteKey(GetSaveKey(symbol));
        }

        PlayerPrefs.Save();

        UnlockStartingSymbols();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ResetUnlocks();
            Debug.Log("Desbloqueos de símbolos reiniciados.");
        }
    }
}