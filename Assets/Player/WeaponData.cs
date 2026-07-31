using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [TextArea] public string description;
    public Sprite icon;

    public WeaponType weaponType;

    [Header("Damage")]
    public float damage = 10f;
    public float damageScalingFactor = 1f;

    [Header("Fire Rate")]
    public float fireRate = 2f;
    public float fireRateScalingFactor = 1f;

    [Header("Projectile")]
    public float bulletSpeed = 10f;
    public float bulletRange = 8f;
    public int bulletsPerShot = 1;
    public float spreadAngle = 0f;
    public float singleShotBloomAngle = 0f;
    public float bulletSize = 1f;
    public GameObject bulletPrefab;

    [Header("Prefab")]
    public GameObject weaponPrefab;

    [Header("Boomerang")]
    public float boomerangDistance = 5f;
    public Sprite boomerangThrownWeaponSprite;

    [Header("Shell Ejection")]
    public GameObject shellPrefab;
    [Min(0)] public int shellsPerShot = 0;
    public string shellEjectPointName = "ShellEjectPoint";
    public float shellEjectAngle = 120f;
    public float shellEjectAngleRandom = 12f;
    public float shellEjectForce = 2.5f;
    public float shellTorque = 120f;

    [Header("Auto Find Transforms")]
    public string firePointName = "FirePoint";
    public string muzzleVFXPointName = "MuzzleVFXPoint";

    [Header("Audio")]
    public AudioClip shootSound;
}