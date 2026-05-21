using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo")]
    public float fireRate = 2f;
    public float damage = 25f;
    public float bulletSpeed = 12f;
    public GameObject bulletPrefab;

    [Header("Multi-disparo")]
    public int bulletsPerShot = 1;

    private float _fireTimer;

    public void ApplyWeaponData(WeaponData weapon)
    {
        if (weapon == null) return;

        fireRate = weapon.fireRate;
        damage = weapon.damage;
        bulletSpeed = weapon.bulletSpeed;
        bulletsPerShot = weapon.bulletsPerShot;
        bulletPrefab = weapon.bulletPrefab;

        Debug.Log($"Arma equipada: {weapon.weaponName}");
    }

    void Update()
    {
        _fireTimer -= Time.deltaTime;

        if (_fireTimer <= 0f)
        {
            Shoot();
            _fireTimer = 1f / fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return;

        System.Array.Sort(enemies, (a, b) =>
            Vector2.Distance(transform.position, a.transform.position)
            .CompareTo(Vector2.Distance(transform.position, b.transform.position))
        );

        int shots = Mathf.Min(bulletsPerShot, enemies.Length);

        for (int i = 0; i < shots; i++)
        {
            Vector2 dir = (enemies[i].transform.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

            GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            HomingBullet bullet = go.GetComponent<HomingBullet>();

            if (bullet != null)
                bullet.Init(enemies[i].transform, bulletSpeed, damage);
        }
    }
}