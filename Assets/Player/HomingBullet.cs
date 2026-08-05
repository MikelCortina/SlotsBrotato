using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 12f;
    public float damage = 25f;
    public bool isCritical;
    public float maxLifetime = 4f;
    public float maxDistance = 8f;

    [Header("Slowdown")]
    [Range(0.01f, 1f)] public float slowDownStartPercent = 0.8f;

    [Header("Visual")]
    public float rotationOffset = -90f;

    [Header("Audio")]
    [SerializeField] private AudioClip hitEnemySfx;
    [SerializeField, Range(0f, 1.5f)] private float hitVolume = 0.9f;

    private Vector2 _direction;
    private float _lifetime;
    private bool _fired;
    private Vector3 _startPosition;
    private float _baseSpeed;
    private Vector3 _originalPrefabScale;
    private Vector3 _initialScale;

    private TrailRenderer _trailRenderer;
    private float _baseTrailWidth;

    private bool _hasHit;

    void Awake()
    {
        _originalPrefabScale = transform.localScale;

        _trailRenderer = GetComponent<TrailRenderer>();
        if (_trailRenderer != null)
            _baseTrailWidth = _trailRenderer.widthMultiplier;
    }

    public void Init(
        Vector2 dir,
        float spd,
        float dmg,
        float distance,
        float sizeMultiplier,
        bool critical
    )
    {
        _direction = dir.normalized;

        speed = spd;
        _baseSpeed = spd;
        damage = dmg;
        maxDistance = distance;
        isCritical = critical;

        transform.localScale =
            _originalPrefabScale * sizeMultiplier;

        _initialScale = transform.localScale;

        if (_trailRenderer != null)
        {
            _trailRenderer.widthMultiplier =
                _baseTrailWidth * sizeMultiplier;

            _trailRenderer.Clear();
        }

        _startPosition = transform.position;
        _lifetime = 0f;
        _fired = true;
        _hasHit = false;
    }

    void Update()
    {
        if (!_fired || _hasHit) return;

        _lifetime += Time.deltaTime;
        if (_lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        float travelledDistance = Vector2.Distance(_startPosition, transform.position);
        float remainingDistance = maxDistance - travelledDistance;

        if (remainingDistance <= 0.05f)
        {
            transform.position = _startPosition + (Vector3)(_direction * maxDistance);
            transform.localScale = Vector3.zero;

            if (_trailRenderer != null)
                _trailRenderer.widthMultiplier = 0f;

            Destroy(gameObject);
            return;
        }

        float currentSpeed = _baseSpeed;
        float travelPercent = maxDistance > 0f ? travelledDistance / maxDistance : 1f;

        if (travelPercent >= slowDownStartPercent)
        {
            float slowRange = 1f - slowDownStartPercent;
            float t = slowRange > 0f
                ? (travelPercent - slowDownStartPercent) / slowRange
                : 1f;

            t = Mathf.Clamp01(t);

            currentSpeed = Mathf.Lerp(_baseSpeed, 0f, t);
            transform.localScale = Vector3.Lerp(_initialScale, Vector3.zero, t);

            if (_trailRenderer != null)
            {
                float currentSizeFactor = 1f - t;
                _trailRenderer.widthMultiplier = (_baseTrailWidth * _initialScale.x) * currentSizeFactor;
            }
        }
        else
        {
            transform.localScale = _initialScale;

            if (_trailRenderer != null)
                _trailRenderer.widthMultiplier = _baseTrailWidth * _initialScale.x;
        }

        float moveThisFrame = currentSpeed * Time.deltaTime;

        if (moveThisFrame >= remainingDistance)
        {
            transform.position = _startPosition + (Vector3)(_direction * maxDistance);
            transform.localScale = Vector3.zero;

            if (_trailRenderer != null)
                _trailRenderer.widthMultiplier = 0f;

            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(_direction * moveThisFrame);

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;

        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();
        if (health == null) return;

        _hasHit = true;

        health.TakeDamage(
            damage,
            transform.position,
            DamageSource.Bullet,
            isCritical
        );

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.StunningImpact))
        {
            if (UnityEngine.Random.value <= 0.20f)
            {
                EnemyController controller = other.GetComponentInParent<EnemyController>();
                if (controller != null)
                    controller.ApplySlow(0.5f, 2f);
            }
        }

        if (MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.DamageCharge))
        {
            SlotMachine.Instance?.AddCharge(0.25f);
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