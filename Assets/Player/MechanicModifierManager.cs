using System.Collections.Generic;
using UnityEngine;

public class MechanicModifierManager : MonoBehaviour
{
    public int maxModifierSlots = 2;
    public static MechanicModifierManager Instance { get; private set; }

    private readonly HashSet<MechanicModifierType> activeModifiers =
        new HashSet<MechanicModifierType>();

    void Awake()
    {
        Instance = this;

    }

    public bool HasModifier(MechanicModifierType modifier)
    {
        return activeModifiers.Contains(modifier);
    }

    public bool AddModifier(MechanicModifierType modifier)
    {
        if (activeModifiers.Contains(modifier))
            return false;

        if (!HasFreeSlot())
        {
            Debug.Log("No quedan ranuras de modificador.");
            return false;
        }

        activeModifiers.Add(modifier);
        ApplyModifierEffect(modifier);

        Debug.Log($"Modificador activado: {modifier}");
        return true;
    }

    private void ApplyModifierEffect(MechanicModifierType modifier)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;

        switch (modifier)
        {
            case MechanicModifierType.AutoPickupCoins:
                if (stats != null)
                    stats.AddCoinPickupRadius(4f);
                break;

            case MechanicModifierType.SlotChargeBoost:
                if (stats != null)
                    stats.ReduceSlotChargeTime(2f);
                break;

            case MechanicModifierType.FourthReel:
                SlotMachine.Instance?.RefreshReelVisibility();
                break;
        }
    }

    public bool HasFreeSlot()
    {
        return activeModifiers.Count < maxModifierSlots;
    }
    public IEnumerable<MechanicModifierType> GetActiveModifiers()
    {
        return activeModifiers;
    }
}