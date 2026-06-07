using TMPro;
using UnityEngine;

public class WeaponDebugUI : MonoBehaviour
{
    [Header("References")]
    public WeaponSystem weaponSystem;
    public PlayerShooter playerShooter;

    [Header("UI")]
    public TextMeshProUGUI weaponText;

    void Update()
    {
        if (weaponText == null) return;

        WeaponData weapon = weaponSystem != null ? weaponSystem.CurrentWeapon : null;

        if (weapon == null || playerShooter == null)
        {
            weaponText.text = "Arma: ninguna";
            return;
        }

        int weaponLevel = WeaponLevelSystem.Instance != null
            ? WeaponLevelSystem.Instance.GetWeaponLevel(weapon)
            : 1;

        float weaponMultiplier = WeaponLevelSystem.Instance != null
            ? WeaponLevelSystem.Instance.GetWeaponScalingMultiplier(weapon)
            : 1f;

        float estimatedDamage = weapon.damage * weaponMultiplier;

        weaponText.text =
            $"Arma: {weapon.weaponName}\n" +
            $"Tipo: {weapon.weaponType}\n" +
            $"Nivel arma: {weaponLevel}\n" +
            $"Multiplicador nivel: x{weaponMultiplier:0.0}\n" +
            $"Daño base arma: {weapon.damage}\n" +
            $"Daño estimado: {estimatedDamage:0.0}\n" +
            $"Cadencia arma: {weapon.fireRate}\n" +
            $"Cadencia actual: {playerShooter.fireRate}\n" +
            $"Velocidad bala: {playerShooter.bulletSpeed}\n" +
            $"Balas por disparo: {playerShooter.bulletsPerShot}\n" +
            $"Spread: {playerShooter.spreadAngle}";
    }
}