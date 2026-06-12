using TMPro;
using UnityEngine;

public class PermanentHealthUpgradeButton : MonoBehaviour
{
    const string HealthUpgradeKey = "PermanentHealthBonus";

    public int voucherCost = 5;
    public int healthBonus = 10;
    public TextMeshProUGUI text;

    public void BuyUpgrade()
    {
        if (WaveVoucherManager.Instance == null) return;

        if (!WaveVoucherManager.Instance.SpendVoucher(voucherCost))
            return;

        int currentBonus = PlayerPrefs.GetInt(HealthUpgradeKey, 0);
        currentBonus += healthBonus;

        PlayerPrefs.SetInt(HealthUpgradeKey, currentBonus);
        PlayerPrefs.Save();

        RefreshUI();
    }

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        int currentBonus = PlayerPrefs.GetInt(HealthUpgradeKey, 0);

        if (text != null)
        {
            text.text =
                $"Vida inicial +{healthBonus}\n" +
                $"Actual: +{currentBonus}\n" +
                $"Coste: {voucherCost} vales";
        }
    }

    public static int GetPermanentHealthBonus()
    {
        return PlayerPrefs.GetInt(HealthUpgradeKey, 0);
    }
}