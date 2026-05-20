using UnityEngine;
using TMPro;

public class ShopUpgrade : MonoBehaviour
{
    [Header("Upgrade")]
    public UpgradeType upgradeType;
    public int cost = 10;
    public float value = 1f;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;

    private bool _bought;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (titleText)
            titleText.text = upgradeType.ToString();

        if (costText)
            costText.text = $"{cost}G";
    }

    public void Buy()
    {
        if (_bought) return;

        if (PlayerWallet.Instance == null) return;
        if (PlayerWallet.Instance.coins < cost) return;

        if (!PlayerWallet.Instance.SpendCoins(cost))
            return;

        PlayerShooter shooter = FindObjectOfType<PlayerShooter>();
        if (shooter == null) return;

        switch (upgradeType)
        {
            case UpgradeType.FireRate:
                shooter.fireRate += value;
                break;

            case UpgradeType.Damage:
                shooter.damage += value;
                break;

            case UpgradeType.MultiShot:
                shooter.bulletsPerShot += Mathf.RoundToInt(value);
                break;
        }

        _bought = true;
        gameObject.SetActive(false);
    }
}