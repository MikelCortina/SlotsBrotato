using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Refresh")]
    public int refreshCost = 5;

    [Header("Catalog")]
    public SlotSymbolData[] allSymbols;

    [Header("Offers")]
    public ShopOfferUI[] offerSlots;

    void OnEnable()
    {
        GenerateOffers();
    }

    public void RefreshShop()
    {
        if (PlayerWallet.Instance == null) return;

        if (!PlayerWallet.Instance.SpendCoins(refreshCost))
            return;

        GenerateOffers();
    }

    public void GenerateOffers()
    {
        List<SlotSymbolData> availableSymbols =
    new List<SlotSymbolData>(allSymbols);

        if (RunConfig.Instance == null) return;
        if (allSymbols == null || allSymbols.Length == 0) return;

        List<SlotSymbolData> owned = RunConfig.Instance.selectedSymbols;

        for (int i = 0; i < offerSlots.Length; i++)
        {
            if (offerSlots[i] == null) continue;

            if (availableSymbols.Count == 0)
                break;

            int randomIndex =
                Random.Range(0, availableSymbols.Count);

            SlotSymbolData randomSymbol =
                availableSymbols[randomIndex];

            availableSymbols.RemoveAt(randomIndex);

            bool alreadyOwned =
                RunConfig.Instance.selectedSymbols.Contains(randomSymbol);

            if (alreadyOwned)
            {
                int level =
                    RunConfig.Instance.GetSymbolLevel(
                        randomSymbol.symbolType
                    );

                int cost = 15 + (level * 10);

                offerSlots[i].SetupUpgradeSymbol(
                    randomSymbol,
                    cost
                );
            }
            else
            {
                offerSlots[i].SetupBuySymbol(
                    randomSymbol,
                    30
                );
            }
        }
    }
}