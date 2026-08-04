using System.Collections.Generic;
using UnityEngine;

public class WeaponLevelSystem : MonoBehaviour
{
    public static WeaponLevelSystem Instance { get; private set; }

    private readonly Dictionary<WeaponData, int> weaponLevels =
        new Dictionary<WeaponData, int>();

    void Awake()
    {
        Instance = this;
    }

    public int GetWeaponLevel(WeaponData weapon)
    {
        if (weapon == null)
            return 1;

        if (!weaponLevels.ContainsKey(weapon))
            weaponLevels[weapon] = 1;

        return weaponLevels[weapon];
    }

    public void UpgradeWeapon(WeaponData weapon)
    {
        if (weapon == null)
            return;

        if (!weaponLevels.ContainsKey(weapon))
            weaponLevels[weapon] = 1;

        weaponLevels[weapon]++;

        Debug.Log(
            $"{weapon.weaponName} sube a nivel {weaponLevels[weapon]}. " +
            $"Cargador: {GetFinalMagazineSize(weapon)}, " +
            $"Recarga: {GetFinalReloadDuration(weapon):0.00}s"
        );
    }

    public float GetWeaponScalingMultiplier(WeaponData weapon)
    {
        int level = GetWeaponLevel(weapon);

        return 1f + ((level - 1) * 0.2f);
    }

    public int GetFinalMagazineSize(WeaponData weapon)
    {
        if (weapon == null)
            return 0;

        if (!weapon.usesAmmo)
            return 0;

        int level = GetWeaponLevel(weapon);
        int extraLevels = level - 1;

        return Mathf.Max(
            1,
            weapon.magazineSize +
            extraLevels * weapon.magazineSizePerLevel
        );
    }

    public float GetFinalReloadDuration(WeaponData weapon)
    {
        if (weapon == null)
            return 0f;

        if (!weapon.usesAmmo)
            return 0f;

        int level = GetWeaponLevel(weapon);
        int extraLevels = level - 1;

        float reductionMultiplier = Mathf.Pow(
            1f - weapon.reloadReductionPerLevel,
            extraLevels
        );

        float finalDuration =
            weapon.reloadDuration * reductionMultiplier;

        return Mathf.Max(
            weapon.minimumReloadDuration,
            finalDuration
        );
    }
}