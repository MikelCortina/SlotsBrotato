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
    private WeaponData _weapon;
    private int _cost;
    private bool _bought;
    private MechanicModifierOfferData _modifierOffer;

    public void SetupBuySymbol(SlotSymbolData symbol, int cost)
    {
        _offerType = ShopOfferType.BuySymbol;
        _symbol = symbol;
        _weapon = null;
        _cost = cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }

    public void SetupUpgradeSymbol(SlotSymbolData symbol, int cost)
    {
        _offerType = ShopOfferType.UpgradeSymbol;
        _symbol = symbol;
        _weapon = null;
        _cost = cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }

    public void SetupUpgradeWeapon(WeaponData weapon, int cost)
    {
        _offerType = ShopOfferType.UpgradeWeapon;
        _weapon = weapon;
        _symbol = null;
        _cost = cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }

    public void Buy()
    {
        if (_bought) return;
        if (PlayerWallet.Instance == null) return;

        if (_offerType == ShopOfferType.BuySymbol || _offerType == ShopOfferType.UpgradeSymbol)
        {
            if (_symbol == null) return;
            if (RunConfig.Instance == null) return;

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
            else
            {
                RunConfig.Instance.UpgradeSymbol(_symbol.symbolType);
            }

            FindFirstObjectByType<SymbolInventoryListUI>()?.Refresh();
        }
        else if (_offerType == ShopOfferType.UpgradeWeapon)
        {
            if (_weapon == null) return;
            if (WeaponLevelSystem.Instance == null) return;

            if (!PlayerWallet.Instance.SpendCoins(_cost))
                return;

            WeaponLevelSystem.Instance.UpgradeWeapon(_weapon);
        }
        else if (_offerType == ShopOfferType.BuyModifier)
        {
            if (_modifierOffer == null) return;
            if (MechanicModifierManager.Instance == null) return;

            if (MechanicModifierManager.Instance.HasModifier(_modifierOffer.modifier))
                return;

            if (!MechanicModifierManager.Instance.HasFreeSlot())
            {
                Debug.Log("No tienes ranuras libres para modificadores.");
                return;
            }

            if (!PlayerWallet.Instance.SpendCoins(_cost))
                return;

            MechanicModifierManager.Instance.AddModifier(_modifierOffer.modifier);
        }

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

        if (_offerType == ShopOfferType.BuyModifier)
        {
            if (titleText && _modifierOffer != null)
                titleText.text = $"Comprar {_modifierOffer.displayName}";

            if (costText)
                costText.text = $"{_cost}G";

            return;
        }
        if (_offerType == ShopOfferType.UpgradeWeapon)
        {
            if (iconImage && _weapon != null)
                iconImage.sprite = _weapon.icon;

            if (titleText && _weapon != null)
            {
                int level = WeaponLevelSystem.Instance != null
                    ? WeaponLevelSystem.Instance.GetWeaponLevel(_weapon)
                    : 1;

                titleText.text = $"Mejorar {_weapon.weaponName} Lv.{level}";
            }

            if (costText)
                costText.text = $"{_cost}G";

            return;
        }

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

    public void SetupBuyModifier(MechanicModifierOfferData modifierOffer)
    {
        _offerType = ShopOfferType.BuyModifier;
        _modifierOffer = modifierOffer;
        _symbol = null;
        _weapon = null;
        _cost = modifierOffer.cost;
        _bought = false;

        RefreshUI();
        gameObject.SetActive(true);
    }
}