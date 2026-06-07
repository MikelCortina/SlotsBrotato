using System.Collections.Generic;
using UnityEngine;

public class MechanicModifierManager : MonoBehaviour
{
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

        activeModifiers.Add(modifier);

        ApplyModifierEffect(modifier);

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
        }
    }
}