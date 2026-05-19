using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo")]
    public float fireRate = 2f;
    public float damage = 25f;
    public float bulletSpeed = 12f;
    public GameObject bulletPrefab; // prefab con HomingBullet.cs

    [Header("Multi-disparo (opcional)")]
    [Tooltip("Cuántas balas se disparan a la vez hacia distintos enemigos")]
    public int bulletsPerShot = 1;

    private float _fireTimer;

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

        // Obtener enemigos más cercanos ordenados por distancia
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
            float spawnOffset = 0.5f;
            Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

            var go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            var bullet = go.GetComponent<HomingBullet>();
            bullet.Init(enemies[i].transform, bulletSpeed, damage);
        }
    }
}