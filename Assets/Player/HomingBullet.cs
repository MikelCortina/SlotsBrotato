using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float damage = 25f;
    public float maxLifetime = 4f;

    [Header("Visual")]
    public float rotationOffset = -90f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitEnemySfx;
    [SerializeField, Range(0f, 1.5f)] private float hitVolume = 0.9f;

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
        {
            health.TakeDamage(damage, transform.position, DamageSource.Bullet);
            if (MechanicModifierManager.Instance != null &&
    MechanicModifierManager.Instance.HasModifier(
        MechanicModifierType.StunningImpact))
            {
                if (Random.value <= 0.20f)
                {
                    EnemyController controller =
                        other.GetComponent<EnemyController>();

                    if (controller != null)
                        controller.ApplySlow(0.5f, 2f);
                }
            }

            if (MechanicModifierManager.Instance != null &&
                MechanicModifierManager.Instance.HasModifier(
                    MechanicModifierType.DamageCharge))
            {
                SlotMachine.Instance?.AddCharge(0.25f);
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                clip: hitEnemySfx,
                eventKey: "bullet_hit_enemy",
                volume: hitVolume,
                priority: SfxPriority.Low,
                cooldown: 0.03f,
                maxVoicesForThisClip: 2
            );
        }

        Destroy(gameObject);
    }
}