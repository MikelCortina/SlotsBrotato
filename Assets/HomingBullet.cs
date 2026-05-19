using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float damage = 25f;
    public float maxLifetime = 4f;

    private Transform _target;
    private Vector2 _lastKnownPos;
    private float _lifetime;
    private bool _fired;

    public void Init(Transform target, float spd, float dmg)
    {
        _target = target;
        _lastKnownPos = target.position;
        speed = spd;
        damage = dmg;
        _fired = true;
    }

    void Update()
    {
        if (!_fired) return;

        // Actualizar última posición conocida mientras el objetivo exista
        if (_target != null)
            _lastKnownPos = _target.position;

        // Mover hacia el objetivo (o su última posición)
        Vector2 dir = (_lastKnownPos - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        // Rotar el sprite en la dirección de movimiento
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Destruir si llega al destino (cuando el enemigo ya está muerto)
        if (_target == null)
        {
            float dist = Vector2.Distance(transform.position, _lastKnownPos);
            if (dist < 0.15f) Destroy(gameObject);
        }

        // Timeout de seguridad
        _lifetime += Time.deltaTime;
        if (_lifetime >= maxLifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        other.GetComponent<EnemyHealth>()?.TakeDamage(damage);

        // Aquí puedes instanciar un efecto de impacto
        Destroy(gameObject);
    }
}