using TMPro;
using UnityEngine;

public class WaveVoucherManager : MonoBehaviour
{
    public static WaveVoucherManager Instance { get; private set; }

    private const string VouchersKey = "WaveVouchers";

    [Header("Vouchers")]
    public int vouchers;

    [Header("UI")]
    public TextMeshProUGUI vouchersText;

    void Awake()
    {
        Instance = this;
        vouchers = PlayerPrefs.GetInt(VouchersKey, 0);
        UpdateUI();
    }

    public void AddVoucher(int amount)
    {
        vouchers += amount;
        Save();
        UpdateUI();
    }

    public bool SpendVoucher(int amount)
    {
        if (vouchers < amount)
            return false;

        vouchers -= amount;
        Save();
        UpdateUI();
        return true;
    }

    void Save()
    {
        PlayerPrefs.SetInt(VouchersKey, vouchers);
        PlayerPrefs.Save();
    }

    void UpdateUI()
    {
        if (vouchersText)
            vouchersText.text = $"Vales: {vouchers}";
    }
}