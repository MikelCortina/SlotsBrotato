using System.Collections.Generic;
using UnityEngine;

public class WeaponLevelSystem : MonoBehaviour
{
    public static WeaponLevelSystem Instance { get; private set; }

    private Dictionary<WeaponData, int> weaponLevels = new Dictionary<WeaponData, int>();

    void Awake()
    {
        Instance = this;
    }

    public int GetWeaponLevel(WeaponData weapon)
    {
        if (weapon == null) return 1;

        if (!weaponLevels.ContainsKey(weapon))
            weaponLevels[weapon] = 1;

        return weaponLevels[weapon];
    }

    public void UpgradeWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        if (!weaponLevels.ContainsKey(weapon))
            weaponLevels[weapon] = 1;

        weaponLevels[weapon]++;

        Debug.Log($"{weapon.weaponName} sube a nivel {weaponLevels[weapon]}");
    }

    public float GetWeaponScalingMultiplier(WeaponData weapon)
    {
        int level = GetWeaponLevel(weapon);

        return 1f + ((level - 1) * 0.2f);
    }
}