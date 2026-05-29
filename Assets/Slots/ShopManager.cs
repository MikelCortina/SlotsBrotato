using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public int refreshCost = 5;
    public ShopUpgrade[] upgrades;

    public void RefreshShop()
    {
        if (PlayerWallet.Instance == null) return;

        if (!PlayerWallet.Instance.SpendCoins(refreshCost))
            return;

        foreach (var upgrade in upgrades)
        {
            if (upgrade != null)
                upgrade.ResetUpgrade();
        }
    }
}