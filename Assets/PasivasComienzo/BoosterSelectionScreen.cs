using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BoosterSelectionScreen : MonoBehaviour
{
    [Header("Referencias")]
    public Transform slotsParent;
    public BoosterSlotUI slotPrefab;
    public TextMeshProUGUI vouchersText;

    [Header("Navegación")]
    public string nextSceneName = "WeaponSelection"; // tu canvas de armas

    [Header("Debug")]
    public KeyCode debugResetKey = KeyCode.P;

    [Header("Navegación")]
    public GameObject thisPanel;
    public GameObject nextPanel; // tu panel de armas/símbolos

    private List<BoosterSlotUI> _slots = new List<BoosterSlotUI>();

    void Start()
    {
        SpawnSlots();
        RefreshVouchers();
    }

    void Update()
    {
        if (Input.GetKeyDown(debugResetKey))
        {
            BoosterManager.Instance.DebugResetAll();
            RefreshAllSlots();
            Debug.Log("Debug: boosters reseteados");
        }
    }

    void SpawnSlots()
    {
        foreach (var booster in BoosterManager.Instance.allBoosters)
        {
            BoosterSlotUI slot = Instantiate(slotPrefab, slotsParent);
            slot.Setup(booster);
            _slots.Add(slot);
        }
    }

    void RefreshAllSlots()
    {
        foreach (var slot in _slots)
            slot.Refresh();
    }

    void RefreshVouchers()
    {
        if (vouchersText && WaveVoucherManager.Instance != null)
            vouchersText.text = $"Vales: {WaveVoucherManager.Instance.vouchers}";
    }



    public void OnContinueButton()
    {
        thisPanel.SetActive(false);
        if (nextPanel != null)
            nextPanel.SetActive(true);
    }
}