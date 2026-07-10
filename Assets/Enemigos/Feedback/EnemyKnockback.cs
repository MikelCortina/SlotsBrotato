using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyController))]
public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3.5f;
    [SerializeField] private float knockbackDuration = 0.08f;

    private EnemyHealth _health;
    private EnemyController _controller;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _controller = GetComponent<EnemyController>();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDamagedFrom += HandleDamagedFrom;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDamagedFrom -= HandleDamagedFrom;
    }

    private void HandleDamagedFrom(Vector2 hitFromPosition, DamageSource source, float amount, float currentHp)
    {
        if (source != DamageSource.Bullet) return;
        Play(hitFromPosition);
    }

    public void Play(Vector2 hitFromPosition)
    {
        Vector2 dir = ((Vector2)transform.position - hitFromPosition).normalized;

        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;

        Vector2 knockbackVelocity = dir * knockbackForce;

        if (_controller != null)
            _controller.StartKnockback(knockbackVelocity, knockbackDuration);
    }
}