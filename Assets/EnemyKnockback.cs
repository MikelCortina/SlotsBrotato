using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyController))]
public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3.5f;
    [SerializeField] private float knockbackDuration = 0.08f;

    private Rigidbody2D _rb;
    private EnemyHealth _health;
    private EnemyController _controller;
    private Coroutine _routine;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(KnockbackRoutine(hitFromPosition));
    }

    private IEnumerator KnockbackRoutine(Vector2 hitFromPosition)
    {
        if (_controller != null)
            _controller.SetKnockback(true);

        Vector2 dir = ((Vector2)transform.position - hitFromPosition).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.up;

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        if (_controller != null)
            _controller.SetKnockback(false);

        _routine = null;
    }
}