using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Refresh")]
    public int refreshCost = 5;

    [Header("Catalog")]
    public SlotSymbolData[] allSymbols;

    [Header("Modifier Offers")]
    public MechanicModifierOfferData[] allModifiers;

    [Header("Debug Offers")]
    [SerializeField] private bool forceModifierOffer = false;
    [SerializeField] private MechanicModifierOfferData forcedModifier;

    [Header("Weapon Offers")]
    public WeaponSystem weaponSystem;
    public int weaponUpgradeBaseCost = 40;
    [Range(0f, 1f)] public float weaponUpgradeChance = 0.35f;

    [Header("Offers")]
    public ShopOfferUI[] offerSlots;

    private bool _freeRefreshUsed;

    void OnEnable()
    {
        _freeRefreshUsed = false;

        ApplyCompoundInterest();

        GenerateOffers();
    }

    public void RefreshShop()
    {
        bool hasFreeRefresh =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.FreeRefresh);

        if (hasFreeRefresh && !_freeRefreshUsed)
        {
            _freeRefreshUsed = true;
            GenerateOffers();
            Debug.Log("Refresco gratuito usado.");
            return;
        }

        if (PlayerWallet.Instance == null) return;

        if (!PlayerWallet.Instance.SpendCoins(refreshCost))
            return;

        GenerateOffers();
    }

    void ApplyCompoundInterest()
    {
        if (MechanicModifierManager.Instance == null) return;

        if (!MechanicModifierManager.Instance.HasModifier(
            MechanicModifierType.CompoundInterest))
            return;

        if (PlayerWallet.Instance == null) return;

        int bonus =
            Mathf.RoundToInt(PlayerWallet.Instance.Coins * 0.10f);

        PlayerWallet.Instance.AddCoins(bonus);

        Debug.Log($"Interés compuesto: +{bonus} monedas");
    }

    public void GenerateOffers()
    {
        if (RunConfig.Instance == null) return;
        if (allSymbols == null || allSymbols.Length == 0) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (forceModifierOffer && forcedModifier != null)
        {
            if (offerSlots != null && offerSlots.Length > 0 && offerSlots[0] != null)
            {
                offerSlots[0].SetupBuyModifier(forcedModifier);
            }
        }
#endif

        List<SlotSymbolData> availableSymbols =
            new List<SlotSymbolData>(allSymbols);

        availableSymbols.RemoveAll(symbol =>
            symbol == null ||
            (SymbolUnlockManager.Instance != null &&
             !SymbolUnlockManager.Instance.IsUnlocked(symbol))
        );

        List<MechanicModifierOfferData> availableModifiers =
            allModifiers != null
                ? new List<MechanicModifierOfferData>(allModifiers)
                : new List<MechanicModifierOfferData>();

        bool weaponOfferAlreadyUsed = false;

        int startIndex = 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (forceModifierOffer && forcedModifier != null)
            startIndex = 1;
#endif

        for (int i = startIndex; i < offerSlots.Length; i++)
        {
            if (offerSlots[i] == null) continue;

            bool canOfferModifier =
                availableModifiers.Count > 0 &&
                Random.value <= 0.20f;

            if (canOfferModifier)
            {
                List<MechanicModifierOfferData> validModifiers =
                    new List<MechanicModifierOfferData>();

                foreach (var modifier in availableModifiers)
                {
                    if (modifier == null)
                        continue;

                    if (GameManager.Instance != null &&
                        GameManager.Instance.CurrentWave < modifier.unlockWave)
                    {
                        continue;
                    }

                    if (MechanicModifierManager.Instance != null &&
                        MechanicModifierManager.Instance.HasModifier(modifier.modifier))
                    {
                        continue;
                    }

                    // La quinta ruleta solo puede aparecer si ya tenemos la cuarta.
                    if (modifier.modifier == MechanicModifierType.FifthReel)
                    {
                        bool hasFourthReel =
                            MechanicModifierManager.Instance != null &&
                            MechanicModifierManager.Instance.HasModifier(
                                MechanicModifierType.FourthReel
                            );

                        if (!hasFourthReel)
                            continue;
                    }

                    if (MechanicModifierManager.Instance != null &&
                        !MechanicModifierManager.Instance.HasFreeSlot())
                    {
                        continue;
                    }

                    validModifiers.Add(modifier);
                }
                if (validModifiers.Count > 0)
                {
                    MechanicModifierOfferData randomModifier =
                        validModifiers[Random.Range(0, validModifiers.Count)];

                    availableModifiers.Remove(randomModifier);

                    offerSlots[i].SetupBuyModifier(randomModifier);
                    continue;
                }
            }

            bool canOfferWeapon =
                !weaponOfferAlreadyUsed &&
                weaponSystem != null &&
                weaponSystem.CurrentWeapon != null &&
                Random.value <= weaponUpgradeChance;

            if (canOfferWeapon)
            {
                WeaponData weapon = weaponSystem.CurrentWeapon;

                int level = WeaponLevelSystem.Instance != null
                    ? WeaponLevelSystem.Instance.GetWeaponLevel(weapon)
                    : 1;

                int cost = weaponUpgradeBaseCost + level * 20;

                offerSlots[i].SetupUpgradeWeapon(weapon, cost);

                weaponOfferAlreadyUsed = true;
                continue;
            }

            if (availableSymbols.Count == 0)
                break;

            int randomIndex = Random.Range(0, availableSymbols.Count);
            SlotSymbolData randomSymbol = availableSymbols[randomIndex];

            availableSymbols.RemoveAt(randomIndex);

            bool alreadyOwned =
                RunConfig.Instance.selectedSymbols.Contains(randomSymbol);

            if (alreadyOwned)
            {
                int level =
                    RunConfig.Instance.GetSymbolLevel(randomSymbol.symbolType);

                int cost = 15 + level * 10;

                offerSlots[i].SetupUpgradeSymbol(randomSymbol, cost);
            }
            else
            {
                offerSlots[i].SetupBuySymbol(randomSymbol, 30);
            }
        }
    }
}