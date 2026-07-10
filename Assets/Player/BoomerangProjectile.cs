using System;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 20f;
    public float maxDistance = 5f;
    public float returnDistance = 0.4f;

    [Header("Rotation")]
    public float spinSpeed = 720f;

    [Header("Chain Boomerang")]
    public float chainSearchRadius = 5f;
    public int maxChainTargets = 2;

    private Transform _owner;
    private Vector2 _direction;
    private Vector2 _startPosition;
    private bool _returning;
    private int _chainCount;
    private bool _hasReturnedToOwner;

    private readonly HashSet<EnemyHealth> _hitEnemies = new HashSet<EnemyHealth>();

    public event Action<BoomerangProjectile, bool> OnBoomerangFinished;

    public void Init(Transform owner, Vector2 direction, float spd, float dmg, float distance)
    {
        _owner = owner;
        _direction = direction.normalized;
        speed = spd;
        damage = dmg;
        maxDistance = distance;
        _startPosition = transform.position;
        _returning = false;
        _chainCount = 0;
        _hasReturnedToOwner = false;
        _hitEnemies.Clear();
    }

    void Update()
    {
        if (_owner == null)
        {
            NotifyAndDestroy(false);
            return;
        }

        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);

        if (!_returning)
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            if (Vector2.Distance(_startPosition, transform.position) >= maxDistance)
                _returning = true;
        }
        else
        {
            Vector2 toOwner = ((Vector2)_owner.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(toOwner * speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, _owner.position) <= returnDistance)
            {
                _hasReturnedToOwner = true;
                NotifyAndDestroy(true);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();
        if (health == null) return;
        if (_hitEnemies.Contains(health)) return;

        _hitEnemies.Add(health);
        health.TakeDamage(damage, transform.position, DamageSource.Bullet);

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.DamageCharge))
        {
            SlotMachine.Instance?.AddCharge(0.25f);
        }

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.ChainBoomerang))
        {
            TryChainToNextEnemy();
        }
    }

    void TryChainToNextEnemy()
    {
        if (_chainCount >= maxChainTargets)
        {
            _returning = true;
            return;
        }

        EnemyHealth nextEnemy = FindNextEnemy();

        if (nextEnemy == null)
        {
            _returning = true;
            return;
        }

        Vector2 targetPosition = nextEnemy.transform.position;
        Vector2 toEnemy = (targetPosition - (Vector2)transform.position).normalized;

        _direction = toEnemy;
        _startPosition = transform.position;
        _chainCount++;
        _returning = false;
    }

    EnemyHealth FindNextEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, chainSearchRadius);

        EnemyHealth bestEnemy = null;
        float bestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null) continue;
            if (_hitEnemies.Contains(enemy)) continue;
            if (enemy.currentHealth <= 0) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
    }

    void NotifyAndDestroy(bool returnedToOwner)
    {
        OnBoomerangFinished?.Invoke(this, returnedToOwner);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!_hasReturnedToOwner && _owner != null)
        {
            OnBoomerangFinished?.Invoke(this, false);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chainSearchRadius);
    }
}