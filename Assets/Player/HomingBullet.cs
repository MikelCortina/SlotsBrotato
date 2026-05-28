using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float damage = 25f;
    public float maxLifetime = 4f;

    [Header("Visual")]
    public float rotationOffset = -90f;

    private Vector2 _direction;
    private float _lifetime;
    private bool _fired;

    public void Init(Vector2 dir, float spd, float dmg)
    {
        _direction = dir.normalized;
        speed = spd;
        damage = dmg;
        _fired = true;
    }

    void Update()
    {
        if (!_fired) return;

        transform.position += (Vector3)(_direction * speed * Time.deltaTime);

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        _lifetime += Time.deltaTime;
        if (_lifetime >= maxLifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var health = other.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage, transform.position, DamageSource.Bullet);

        Destroy(gameObject);
    }
}