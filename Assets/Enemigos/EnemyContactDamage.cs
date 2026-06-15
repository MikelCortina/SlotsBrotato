using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public float damageInterval = 0.5f;

    private EnemyDamage _enemyDamage;
    private float _nextDamageTime;

    void Awake()
    {
        _enemyDamage = GetComponent<EnemyDamage>();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (Time.time < _nextDamageTime) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        float damage = _enemyDamage != null ? _enemyDamage.damage : 1f;

        _nextDamageTime = Time.time + damageInterval;
        playerHealth.TakeDamage(damage);
    }
}