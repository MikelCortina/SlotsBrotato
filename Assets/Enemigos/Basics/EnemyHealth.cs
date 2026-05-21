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

    public event Action<float, float> OnDamaged;
    public event Action<Vector2, DamageSource, float, float> OnDamagedFrom;
    public event Action OnDeath;

    private float _hp;

    public float currentHealth => _hp; // Propiedad pública de solo lectura

    void Awake()
    {
        _hp = maxHp;
    }

    public void SubscribeOnDeath(Action callback) => OnDeath += callback;

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position, DamageSource.Unknown);
    }

    public void TakeDamage(float amount, Vector2 hitFromPosition, DamageSource source)
    {
        if (_hp <= 0f) return;

        _hp = Mathf.Max(0f, _hp - amount);
        OnDamaged?.Invoke(amount, _hp);
        OnDamagedFrom?.Invoke(hitFromPosition, source, amount, _hp);

        if (_hp <= 0f)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}