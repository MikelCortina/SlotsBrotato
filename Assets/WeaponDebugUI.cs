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

        weaponText.text =
            $"Arma: {weapon.weaponName}\n" +
            $"Tipo: {weapon.weaponType}\n" +
            $"Daño arma: {weapon.damage}\n" +
            $"Daño actual disparo: {playerShooter.damage}\n" +
            $"Cadencia arma: {weapon.fireRate}\n" +
            $"Cadencia actual: {playerShooter.fireRate}\n" +
            $"Velocidad bala: {playerShooter.bulletSpeed}\n" +
            $"Balas por disparo: {playerShooter.bulletsPerShot}\n" +
            $"Spread: {playerShooter.spreadAngle}";
    }
}