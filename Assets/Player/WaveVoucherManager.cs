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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            vouchers = 0;
            PlayerPrefs.SetInt(VouchersKey, 0);
            PlayerPrefs.SetInt("PermanentHealthBonus", 0);
            PlayerPrefs.Save();
            UpdateUI();

            // Resetea la vida del jugador a su valor base
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            Debug.Log("Vales y bonus reseteados a 0");
        }
    }
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
        Debug.Log($"AddVoucher llamado: +{amount} | Total: {vouchers}", this);
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