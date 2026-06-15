using UnityEngine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RingSpitterEnemyController : MonoBehaviour

{
    public float moveSpeed = 2f;

    [Header("Shoot")]
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float projectileDamage = 2f;

    [Header("Distance")]
    public float fleeDistance = 4f;

    Transform _player;
    float _nextShot;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            _player = p.transform;
    }

    void Update()
    {
        if (_player == null)
            return;

        float distance =
            Vector2.Distance(transform.position, _player.position);

        if (distance < fleeDistance)
        {
            Vector2 away =
                ((Vector2)transform.position - (Vector2)_player.position).normalized;

            transform.position +=
                (Vector3)(away * moveSpeed * Time.deltaTime);
        }

        if (Time.time >= _nextShot)
        {
            Shoot();
            _nextShot = Time.time + shootInterval;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
            return;

        int projectileCount = 8;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * 360f / projectileCount;

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            Vector3 spawnPos =
                transform.position + (Vector3)(dir * 0.6f);

            GameObject go =
                Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            EnemyProjectile projectile =
                go.GetComponent<EnemyProjectile>();

            if (projectile != null)
                projectile.Init(dir, projectileDamage);
        }
    }
}
