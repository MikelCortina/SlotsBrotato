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

    [Header("Shotgun")]
    public float spreadAngle = 15f;

    private float _fireTimer;
    private PlayerStats _stats;
    private WeaponData _currentWeapon;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public void ApplyWeaponData(WeaponData weapon)
    {
        if (weapon == null) return;
        _currentWeapon = weapon;
        fireRate = weapon.fireRate;
        damage = weapon.damage;
        bulletSpeed = weapon.bulletSpeed;
        bulletsPerShot = weapon.bulletsPerShot;
        bulletPrefab = weapon.bulletPrefab;
        spreadAngle = weapon.spreadAngle;

        Debug.Log($"Arma equipada: {weapon.weaponName}");
    }

    void Update()
    {
        _fireTimer -= Time.deltaTime;

        if (_fireTimer <= 0f)
        {
            Shoot();
            float finalFireRate = fireRate;

            if (_stats != null)
                finalFireRate = _stats.GetFireRate(fireRate);

            _fireTimer = 1f / finalFireRate;
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

        Transform target = enemies[0].transform;

        if (_currentWeapon != null &&
            _currentWeapon.weaponType == WeaponType.Boomerang)
        {
            ShootBoomerang(target);
            return;
        }

        ShootProjectileWeapon(target);
    }

    void ShootProjectileWeapon(Transform target)
    {
        int shots = bulletsPerShot;

        Vector2 baseDir =
            (target.position - transform.position).normalized;

        for (int i = 0; i < shots; i++)
        {
            float angleOffset = 0f;

            if (shots > 1)
            {
                angleOffset = Mathf.Lerp(
                    -spreadAngle,
                    spreadAngle,
                    (float)i / (shots - 1)
                );
            }

            Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

            Vector3 spawnPos =
                transform.position + (Vector3)(dir * 0.5f);

            GameObject go = Instantiate(
                bulletPrefab,
                spawnPos,
                Quaternion.identity
            );

            HomingBullet bullet = go.GetComponent<HomingBullet>();

            if (bullet == null) continue;

            float finalDamage = damage;

            if (_stats != null)
                finalDamage = _stats.GetFinalDamage(damage);

            bullet.Init(target, bulletSpeed, finalDamage);
        }
    }


    void ShootBoomerang(Transform target)
    {
        if (target == null || bulletPrefab == null) return;

        Vector2 dir = (target.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BoomerangProjectile boomerang = go.GetComponent<BoomerangProjectile>();

        if (boomerang == null) return;

        float finalDamage = damage;

        if (_stats != null)
            finalDamage = _stats.GetFinalDamage(damage);

        float distance = 5f;

        if (_currentWeapon != null)
            distance = _currentWeapon.boomerangDistance;

        boomerang.Init(transform, dir, bulletSpeed, finalDamage, distance);
    }
}