using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public Sprite icon;

    [Header("Prefab visual")]
    public GameObject weaponPrefab;

    [Header("Stats")]
    public float fireRate = 2f;
    public float damage = 25f;
    public float bulletSpeed = 12f;
    public int bulletsPerShot = 1;
    public GameObject bulletPrefab;
    public float spreadAngle = 0f;
    public float boomerangDistance = 5f;

    [Header("Posición en mano")]
    public float holdRadius = 0.35f;
}