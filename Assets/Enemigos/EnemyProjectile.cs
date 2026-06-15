using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 1f;
    public float lifetime = 4f;

    private Vector2 _direction;
    private bool _initialized;

    public void Init(Vector2 direction, float projectileDamage)
    {
        _direction = direction.normalized;
        damage = projectileDamage;
        _initialized = true;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!_initialized)
            return;

        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}