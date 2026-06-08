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
        AddModifier(MechanicModifierType.DamageCharge);
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

        Debug.Log($"Modificador activado: {modifier}");
        return true;
    }

    private void ApplyModifierEffect(MechanicModifierType modifier)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;

        if (stats == null) return;

        switch (modifier)
        {
            case MechanicModifierType.AutoPickupCoins:
                stats.AddCoinPickupRadius(4f);
                break;
            case MechanicModifierType.SlotChargeBoost:
                stats.ReduceSlotChargeTime(2f);
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