using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;

    private ShopOfferType _offerType;
    private SlotSymbolData _symbol;
    private int _cost;
    private bool _bought;

    public void SetupBuySymbol(SlotSymbolData symbol, int cost)
    {
        _offerType = ShopOfferType.BuySymbol;
        _symbol = symbol;
        _cost = cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }

    public void SetupUpgradeSymbol(SlotSymbolData symbol, int cost)
    {
        _offerType = ShopOfferType.UpgradeSymbol;
        _symbol = symbol;
        _cost = cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }

    public void Buy()
    {
        if (_bought) return;
        if (_symbol == null) return;
        if (RunConfig.Instance == null) return;
        if (PlayerWallet.Instance == null) return;

        bool currentlyOwned = RunConfig.Instance.selectedSymbols.Contains(_symbol);

        if (_offerType == ShopOfferType.UpgradeSymbol && !currentlyOwned)
        {
            ConvertToBuyOffer();
            return;
        }

        if (!PlayerWallet.Instance.SpendCoins(_cost))
            return;

        if (_offerType == ShopOfferType.BuySymbol)
        {
            if (!currentlyOwned)
                RunConfig.Instance.selectedSymbols.Add(_symbol);
        }
        else if (_offerType == ShopOfferType.UpgradeSymbol)
        {
            RunConfig.Instance.UpgradeSymbol(_symbol.symbolType);
        }

        FindFirstObjectByType<SymbolInventoryListUI>()?.Refresh();

        _bought = true;
        gameObject.SetActive(false);
    }

    public void RefreshIfSymbolChanged(SlotSymbolData changedSymbol)
    {
        if (_symbol != changedSymbol) return;
        if (RunConfig.Instance == null) return;

        bool currentlyOwned = RunConfig.Instance.selectedSymbols.Contains(_symbol);

        if (_offerType == ShopOfferType.UpgradeSymbol && !currentlyOwned)
            ConvertToBuyOffer();
        else
            RefreshUI();
    }

    private void ConvertToBuyOffer()
    {
        _offerType = ShopOfferType.BuySymbol;
        _cost = 30;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (iconImage && _symbol)
            iconImage.sprite = _symbol.icon;

        if (titleText && _symbol)
        {
            if (_offerType == ShopOfferType.BuySymbol)
            {
                titleText.text = $"Comprar {_symbol.symbolName}";
            }
            else
            {
                int level = RunConfig.Instance != null
                    ? RunConfig.Instance.GetSymbolLevel(_symbol.symbolType)
                    : 1;

                titleText.text = $"Mejorar {_symbol.symbolName} Lv.{level}";
            }
        }

        if (costText)
            costText.text = $"{_cost}G";
    }
}