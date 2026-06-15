using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    public float damage = 25f;
    public float fireRate = 0f;
    public float critChance = 0f;
    public float critMultiplier = 2f;

    [Header("Defense")]
    public int maxHealth = 5;
    public float damageReduction = 0f;
    public float regeneration = 0f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Coin Collection")]
    public float coinPickupRadius = 3f;

    [Header("Slot Machine")]
    public float slotChargeTime = 10f;


    void Start()
    {
        maxHealth += PermanentHealthUpgradeButton.GetPermanentHealthBonus();
    }

    // ✅ FIX: añadidos scalingFactor y canCrit como parámetros
    public float GetFinalDamage(float baseDamage, float scalingFactor, bool canCrit)
    {
        return GetScaledDamage(baseDamage, 1f, true);
    }

    public float GetScaledDamage(float baseDamage, float scalingFactor, bool canCrit)
    {
        float finalDamage = baseDamage + (damage * scalingFactor);

        if (canCrit && Random.value < critChance)
            finalDamage *= critMultiplier;

        return finalDamage;
    }

    public float GetScaledFireRate(float baseFireRate, float scalingFactor)
    {
        return baseFireRate + (fireRate * scalingFactor);
    }

    public int GetFinalReceivedDamage(int incomingDamage)
    {
        float reduced = incomingDamage * (1f - damageReduction / 100f);
        return Mathf.Max(1, Mathf.RoundToInt(reduced));
    }

    public float GetMoveSpeed() => moveSpeed;

    public float GetFireRate(float weaponFireRate)
    {
        return weaponFireRate + fireRate;
    }

    public float GetCoinPickupRadius() => coinPickupRadius;

    public void AddCoinPickupRadius(float amount) => coinPickupRadius += amount;

    public void ApplyPassive(PassiveData passive)
    {
        if (passive == null) return;

        damage += passive.bonusDamage;
        fireRate += passive.bonusFireRate;
        moveSpeed += passive.bonusMoveSpeed;
        maxHealth += passive.bonusMaxHealth;
    }

    public void ApplyPassives(List<PassiveData> passives)
    {
        if (passives == null) return;

        foreach (var passive in passives)
            ApplyPassive(passive);
    }

    public void ReduceSlotChargeTime(float amount)
    {
        slotChargeTime = Mathf.Max(1f, slotChargeTime - amount);
    }

    public void AddDamage(float amount) => damage += amount;
    public void AddFireRate(float amount) => fireRate += amount;
    public void AddMaxHealth(int amount) => maxHealth += amount;
    public void AddMoveSpeed(float amount) => moveSpeed += amount;

    public void AddCritChance(float amount)
    {
        critChance = Mathf.Clamp01(critChance + amount);
    }

    public void AddCritMultiplier(float amount) => critMultiplier += amount;
    public void AddRegeneration(float amount) => regeneration += amount;

    public void AddDamageReduction(float amount)
    {
        damageReduction = Mathf.Clamp(damageReduction + amount, 0f, 100f);
    }
}