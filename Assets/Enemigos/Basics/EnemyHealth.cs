using System;
using UnityEngine;

public enum DamageSource
{
    Unknown,
    Bullet,
    Melee,
    Trap
}

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHp = 50f;

    [Header("Visual / Animator")]
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private string deathTrigger = "Die";

    public event Action<float, float> OnDamaged;
    public event Action<Vector2, DamageSource, float, float> OnDamagedFrom;
    public event Action OnDeath;

    private float _hp;
    private bool _isDead;

    public float currentHealth => _hp;
    public bool IsDead => _isDead;

    private void Awake()
    {
        _hp = maxHp;

        if (visualAnimator == null)
            visualAnimator = GetComponentInChildren<Animator>();
    }

    public void ResetHealth()
    {
        _hp = maxHp;
        _isDead = false;
    }

    public void SubscribeOnDeath(Action callback)
    {
        OnDeath += callback;
    }

    public void UnsubscribeOnDeath(Action callback)
    {
        OnDeath -= callback;
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, DamageSource.Unknown);
    }

    public void TakeDamage(float amount, Vector2 hitFromPosition, DamageSource source)
    {
        if (_isDead || _hp <= 0f) return;

        _hp = Mathf.Max(0f, _hp - amount);

        OnDamaged?.Invoke(amount, _hp);
        OnDamagedFrom?.Invoke(hitFromPosition, source, amount, _hp);

        if (_hp <= 0f)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        OnDeath?.Invoke();

        if (visualAnimator != null)
            visualAnimator.SetTrigger(deathTrigger);

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}