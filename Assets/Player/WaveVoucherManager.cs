using TMPro;
using UnityEngine;

public class WaveVoucherManager : MonoBehaviour
{
    public static WaveVoucherManager Instance { get; private set; }

    [Header("Vouchers")]
    public int vouchers;

    [Header("UI")]
    public TextMeshProUGUI vouchersText;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddVoucher(int amount)
    {
        vouchers += amount;
        UpdateUI();
    }

    public bool SpendVoucher(int amount)
    {
        if (vouchers < amount)
            return false;

        vouchers -= amount;
        UpdateUI();
        return true;
    }

    void UpdateUI()
    {
        if (vouchersText)
            vouchersText.text = $"Vales: {vouchers}";
    }
}