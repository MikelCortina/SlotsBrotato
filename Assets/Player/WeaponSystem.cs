using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public WeaponData CurrentWeapon { get; private set; }

    private PlayerShooter _shooter;

    void Awake()
    {
        _shooter = GetComponent<PlayerShooter>();
    }

    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        CurrentWeapon = weapon;

        if (_shooter != null)
            _shooter.ApplyWeaponData(weapon);

        Debug.Log($"Weapon equipped: {weapon.weaponName}");
    }
}