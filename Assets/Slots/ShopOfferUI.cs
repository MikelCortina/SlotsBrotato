using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopOfferUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;

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
        if (descriptionText)
            descriptionText.text = "";

        if (_offerType == ShopOfferType.BuyModifier)
        {
            if (titleText && _modifierOffer != null)
                titleText.text = $"Comprar {_modifierOffer.displayName}";

            if (costText)
                costText.text = $"{_cost}G";

            if (descriptionText && _modifierOffer != null)
                descriptionText.text = _modifierOffer.description;

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

            if (descriptionText && _weapon != null)
            {
                int level = WeaponLevelSystem.Instance != null
                    ? WeaponLevelSystem.Instance.GetWeaponLevel(_weapon)
                    : 1;

                float currentMultiplier = WeaponLevelSystem.Instance != null
                    ? WeaponLevelSystem.Instance.GetWeaponScalingMultiplier(_weapon)
                    : 1f;

                float nextMultiplier = 1f + level * 0.2f;

                descriptionText.text =
                    $"Mejora el escalado del arma.\n\n" +
                    $"Mejora: Lv.{level} → Lv.{level + 1}\n" +
                    $"Escalado actual: x{currentMultiplier:0.0}\n" +
                    $"Nuevo escalado: x{nextMultiplier:0.0}";
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

        if (descriptionText && _symbol != null)
        {
            int currentLevel = RunConfig.Instance != null
                ? RunConfig.Instance.GetSymbolLevel(_symbol.symbolType)
                : 1;

            if (_offerType == ShopOfferType.BuySymbol)
            {
                float value = GetSymbolValue(_symbol, 1);

                descriptionText.text =
                    $"{_symbol.description}\n\n" +
                    $"Nivel inicial: Lv.1\n" +
                    $"Valor: {GetSymbolValueLabel(_symbol, value)}\n" +
                    $"Jackpot: {GetSymbolValueLabel(_symbol, value * _symbol.jackpotMultiplier)}";
            }
            else
            {
                int nextLevel = currentLevel + 1;

                float currentValue = GetSymbolValue(_symbol, currentLevel);
                float nextValue = GetSymbolValue(_symbol, nextLevel);
                float nextJackpotValue = nextValue * _symbol.jackpotMultiplier;

                descriptionText.text =
                    $"{_symbol.description}\n\n" +
                    $"Mejora: Lv.{currentLevel} → Lv.{nextLevel}\n" +
                    $"Actual: {GetSymbolValueLabel(_symbol, currentValue)}\n" +
                    $"Nuevo: {GetSymbolValueLabel(_symbol, nextValue)}\n" +
                    $"Jackpot nuevo: {GetSymbolValueLabel(_symbol, nextJackpotValue)}";
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

    float GetSymbolValue(SlotSymbolData symbol, int level)
    {
        if (symbol == null)
            return 0f;

        return symbol.baseEffectValue +
               symbol.valuePerLevel * (level - 1);
    }
    string GetSymbolValueLabel(SlotSymbolData symbol, float value)
    {
        if (symbol == null)
            return value.ToString("0.#");

        switch (symbol.symbolType)
        {
            case SlotSymbolType.Power:
                return $"+{value:0.#} daño";

            case SlotSymbolType.Coin:
                return $"+{value:0.#} monedas";

            case SlotSymbolType.Shield:
                return $"+{value:0.#} escudo";

            case SlotSymbolType.Berserk:
                return $"+{value:0.#} daño temporal";

            case SlotSymbolType.Static:
                return $"{value:0.#} rayos";

            default:
                return value.ToString("0.#");
        }
    }
}