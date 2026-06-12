using TMPro;
using UnityEngine;

public class ModifierSlotUpgradeButton : MonoBehaviour
{
    public int voucherCost = 3;
    public TextMeshProUGUI text;

    public void BuySlot()
    {
        if (MechanicModifierManager.Instance == null) return;
        if (WaveVoucherManager.Instance == null) return;

        if (MechanicModifierManager.Instance.maxModifierSlots >= 4)
            return;

        if (!WaveVoucherManager.Instance.SpendVoucher(voucherCost))
            return;

        MechanicModifierManager.Instance.maxModifierSlots++;
        MechanicModifierManager.Instance.SaveModifierSlots();

        RefreshUI();
    }

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (text == null) return;

        int currentSlots =
            MechanicModifierManager.Instance != null
            ? MechanicModifierManager.Instance.maxModifierSlots
            : 0;

        text.text =
            $"Ranuras: {currentSlots}/4\nCoste: {voucherCost} vales";
    }
}