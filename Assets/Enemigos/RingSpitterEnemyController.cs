using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class RingSpitterEnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("Shoot")]
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float projectileDamage = 2f;
    public string attackTriggerName = "Attack";

    [Header("Distance")]
    public float fleeDistance = 4f;

    Transform _player;
    Animator _animator;
    Rigidbody2D _rb;
    float _nextShot;
    bool _isAttacking;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
    }

    void FixedUpdate()
    {
        if (_player == null)
            return;

        if (_isAttacking)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(_rb.position, _player.position);

        if (distance < fleeDistance)
        {
            Vector2 away = (_rb.position - (Vector2)_player.position).normalized;
            _rb.MovePosition(_rb.position + away * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (_player == null)
            return;

        if (!_isAttacking && Time.time >= _nextShot)
            StartAttack();
    }

    void StartAttack()
    {
        _isAttacking = true;
        _nextShot = Time.time + shootInterval;

        if (_animator != null)
            _animator.SetTrigger(attackTriggerName);
        else
            Shoot();
    }

    public void AnimationEvent_Shoot()
    {
        Shoot();
    }

    public void AnimationEvent_EndAttack()
    {
        _isAttacking = false;
    }

    void Shoot()
    {
        if (projectilePrefab == null)
            return;

        int projectileCount = 8;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * 360f / projectileCount;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);

            GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            SpitterProjectile projectile = go.GetComponent<SpitterProjectile>();
            if (projectile != null)
                projectile.Init(dir, projectileDamage);
        }
    }
}