using UnityEngine;

public class ShopPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ContinueFromShop();
    }
}