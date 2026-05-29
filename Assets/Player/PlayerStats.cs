using System.Collections.Generic;
using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    [Header("Offense")]
    public float damage = 25f;
    public float fireRate = 2f;
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

    public float GetFinalDamage(float baseDamage)
    {
        float finalDamage = baseDamage + damage;

        if (Random.value < critChance)
            finalDamage *= critMultiplier;

        return finalDamage;
    }

    public int GetFinalReceivedDamage(int incomingDamage)
    {
        float reduced = incomingDamage * (1f - damageReduction);
        return Mathf.Max(1, Mathf.RoundToInt(reduced));
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetFireRate(float weaponFireRate)
    {
        return weaponFireRate + fireRate;
    }

    public float GetCoinPickupRadius()
    {
        return coinPickupRadius;
    }

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
        {
            ApplyPassive(passive);
        }
    }

}