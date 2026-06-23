using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BoosterSlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image icon;
    public Image background;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private BoosterData _booster;

    public void Setup(BoosterData booster)
    {
        _booster = booster;
        Refresh();
    }

    public void Refresh()
    {
        if (_booster == null) return;

        bool unlocked = BoosterManager.Instance.IsUnlocked(_booster);
        bool active = BoosterManager.Instance.IsActive(_booster);

        // Info
        if (icon) icon.sprite = _booster.icon;
        if (nameText) nameText.text = _booster.boosterName;
        if (descriptionText) descriptionText.text = _booster.description;

        // Estado
        if (unlocked)
        {
            if (costText) costText.text = "Desbloqueado";
            if (background) background.color = active ? _booster.equippedColor : _booster.unlockedColor;
            if (actionButtonText) actionButtonText.text = active ? "Desequipar" : "Equipar";
        }
        else
        {
            if (costText) costText.text = $"Coste: {_booster.voucherCost} vales";
            if (background) background.color = _booster.lockedColor;
            if (actionButtonText) actionButtonText.text = "Comprar";
        }
    }

    public void OnActionButton()
    {
        if (_booster == null) return;

        bool unlocked = BoosterManager.Instance.IsUnlocked(_booster);

        if (!unlocked)
        {
            bool success = BoosterManager.Instance.TryPurchase(_booster);
            if (!success) Debug.Log("No tienes suficientes vales");
        }
        else
        {
            BoosterManager.Instance.SetActive(_booster);
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.RefreshMaxHealthFromStats();
            Debug.Log("Booster equipado real: " +
    (BoosterManager.Instance.GetActiveBooster() != null
        ? BoosterManager.Instance.GetActiveBooster().boosterName
        : "ninguno"));
        }

        Refresh();
    }
}