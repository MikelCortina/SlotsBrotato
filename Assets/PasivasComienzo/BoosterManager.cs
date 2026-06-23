using System.Collections.Generic;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
    public static BoosterManager Instance { get; private set; }

    [Header("Todos los boosters del juego")]
    public List<BoosterData> allBoosters;

    private const string ActiveBoosterKey = "ActiveBoosterId";
    private string activeBoosterId = ""; // el que está equipado para esta partida
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        activeBoosterId = PlayerPrefs.GetString(ActiveBoosterKey, "");
    }
    // ¿Está comprado?
    public bool IsUnlocked(BoosterData booster)
    {
        return PlayerPrefs.GetInt("Booster_Unlocked_" + booster.id, 0) == 1;
    }

    // Comprar
    public bool TryPurchase(BoosterData booster)
    {
        if (IsUnlocked(booster)) return false;
        if (WaveVoucherManager.Instance == null) return false;
        if (!WaveVoucherManager.Instance.SpendVoucher(booster.voucherCost)) return false;

        PlayerPrefs.SetInt("Booster_Unlocked_" + booster.id, 1);
        PlayerPrefs.Save();
        return true;
    }

    // Equipar / desequipar
    public void SetActive(BoosterData booster)
    {
        if (!IsUnlocked(booster)) return;

        activeBoosterId = (activeBoosterId == booster.id) ? "" : booster.id;

        PlayerPrefs.SetString(ActiveBoosterKey, activeBoosterId);
        PlayerPrefs.Save();
    }

    public bool IsActive(BoosterData booster)
    {
        return activeBoosterId == booster.id;
    }

    public BoosterData GetActiveBooster()
    {
        if (string.IsNullOrEmpty(activeBoosterId)) return null;
        return allBoosters.Find(b => b.id == activeBoosterId);
    }

    // DEBUG — resetea todo
    public void DebugResetAll()
    {
        foreach (var b in allBoosters)
            PlayerPrefs.DeleteKey("Booster_Unlocked_" + b.id);
        PlayerPrefs.Save();
        activeBoosterId = "";
        Debug.Log("Boosters reseteados");
    }
}