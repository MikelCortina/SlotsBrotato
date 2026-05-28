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

    [Header("Rarity")]
    public UpgradeRarity rarity;

    public UnityEngine.UI.Image backgroundImage;


    private bool _bought;

    void Start()
    {
        ApplyRarity();
        UpdateUI();
       
    }

    void UpdateUI()
    {
        if (titleText)
            titleText.text = upgradeType.ToString();

        if (costText)
            costText.text = $"{cost}G";
        if (titleText)
            titleText.text = $"{upgradeType} [{rarity}] +{value}";
    }

    void ApplyRarity()
    {
        float multiplier = 1f;

        switch (rarity)
        {
            case UpgradeRarity.Common:
                multiplier = 1f;
                if (backgroundImage) backgroundImage.color = Color.white;
                break;

            case UpgradeRarity.Rare:
                multiplier = 1.5f;
                if (backgroundImage) backgroundImage.color = Color.cyan;
                break;

            case UpgradeRarity.Epic:
                multiplier = 2.5f;
                if (backgroundImage) backgroundImage.color = new Color(0.7f, 0.3f, 1f);
                break;

            case UpgradeRarity.Legendary:
                multiplier = 5f;
                if (backgroundImage) backgroundImage.color = Color.yellow;
                break;
        }

        value *= multiplier;
        cost = Mathf.RoundToInt(cost * multiplier);
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

        PlayerStats stats = FindObjectOfType<PlayerStats>();


        switch (upgradeType)
        {
            case UpgradeType.Damage:
                if (stats != null) stats.damage += value;
                break;

            case UpgradeType.FireRate:
                if (stats != null) stats.fireRate += value;
                break;

            case UpgradeType.MoveSpeed:
                if (stats != null) stats.moveSpeed += value;
                break;

            case UpgradeType.CritChance:
                if (stats != null) stats.critChance += value;
                break;

            case UpgradeType.MaxHealth:
                if (stats != null) stats.maxHealth += Mathf.RoundToInt(value);
                break;

            case UpgradeType.Regen:
                if (stats != null) stats.regeneration += value;
                break;

            case UpgradeType.MultiShot:
                if (shooter != null) shooter.bulletsPerShot += Mathf.RoundToInt(value);
                break;
        }

        _bought = true;
        gameObject.SetActive(false);
    }

    public void ResetUpgrade()
    {
        _bought = false;
        gameObject.SetActive(true);
    }
}