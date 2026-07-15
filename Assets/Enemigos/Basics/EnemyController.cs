using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 2.5f;
    public float acceleration = 8f;
    public float separationRadius = 1.2f;
    public float separationForce = 6f;
    public float arrivalRadius = 0.7f;
    public float damageInterval = 0.5f;

    [Header("Bounce")]
    public float bounceHeight = 0.4f;
    public float bounceFrequency = 2.5f;
    public Transform visualRoot;

    [Header("Jump Timing")]
    [Range(0f, 1f)] public float idlePortion = 0.35f;
    [Range(0f, 1f)] public float movePortion = 0.65f;

    [Header("Hit Reaction")]
    public float hitGroundLockTime = 0.10f;
    public float hitFallSmoothTime = 0.05f;
    public float maxHitFallSpeed = 20f;

    private float _baseSpeed;
    private float _slowTimer;
    private float _cycleTime;
    private float _cyclePhase;
    private float _bouncePhase;
    private float _hitGroundLockTimer;

    private float _visualY;
    private float _visualYVelocity;
    private bool _recoveringToGround;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private EnemyDamage _enemyDamage;
    private float _nextDamageTime;

    private Vector2 _currentVelocity;

    private bool _isKnockedBack;
    private Vector2 _knockbackVelocity;
    private float _knockbackTimer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _enemyDamage = GetComponent<EnemyDamage>();

        if (visualRoot != null)
            _animator = visualRoot.GetComponent<Animator>();
        else
            _animator = GetComponent<Animator>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            _player = p.transform;
            _playerHealth = p.GetComponent<PlayerHealth>();
        }

        _baseSpeed = speed;

        float cycleDuration = 1f / bounceFrequency;
        _cycleTime = Random.Range(0f, cycleDuration);

        if (visualRoot != null)
            _visualY = visualRoot.localPosition.y;
    }

    void Update()
    {
        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0f)
                speed = _baseSpeed;
        }

        if (_isKnockedBack)
        {
            _knockbackTimer -= Time.deltaTime;
            if (_knockbackTimer <= 0f)
                EndKnockback();
        }

        if (_hitGroundLockTimer > 0f)
            _hitGroundLockTimer -= Time.deltaTime;

        if (_recoveringToGround)
        {
            UpdateRecoverToGround();
            return;
        }

        float cycleDuration = 1f / bounceFrequency;
        _cycleTime += Time.deltaTime;
        _cyclePhase = (_cycleTime / cycleDuration) % 1f;

        bool isMovingPhase = _cyclePhase >= idlePortion && !_isKnockedBack;

        if (isMovingPhase)
        {
            float movePhase = (_cyclePhase - idlePortion) / Mathf.Max(0.0001f, movePortion);
            _bouncePhase = Mathf.Sin(movePhase * Mathf.PI);
        }
        else
        {
            _bouncePhase = 0f;
        }

        _visualY = _bouncePhase * bounceHeight;
        ApplyVisualY();
    }

    private void UpdateRecoverToGround()
    {
        _bouncePhase = 0f;

        _visualY = Mathf.SmoothDamp(
            _visualY,
            0f,
            ref _visualYVelocity,
            hitFallSmoothTime,
            maxHitFallSpeed,
            Time.deltaTime
        );

        if (Mathf.Abs(_visualY) <= 0.01f)
        {
            _visualY = 0f;
            _visualYVelocity = 0f;
            _recoveringToGround = false;
        }

        ApplyVisualY();
    }

    private void ApplyVisualY()
    {
        if (visualRoot != null)
            visualRoot.localPosition = new Vector3(0f, _visualY, 0f);

        if (_animator != null)
            _animator.SetFloat("bouncePhase", bounceHeight > 0.0001f ? _visualY / bounceHeight : 0f);
    }

    public void StartKnockback(Vector2 velocity, float duration)
    {
        _isKnockedBack = true;
        _knockbackVelocity = velocity;
        _knockbackTimer = duration;

        _currentVelocity = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
    }

    private void EndKnockback()
    {
        _isKnockedBack = false;
        _knockbackTimer = 0f;
        _knockbackVelocity = Vector2.zero;
        _currentVelocity = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 pos = _rb.position;

        if (_isKnockedBack)
        {
            _rb.MovePosition(pos + _knockbackVelocity * Time.fixedDeltaTime);
            return;
        }

        bool canMove = _cyclePhase >= idlePortion && _hitGroundLockTimer <= 0f;

        if (!canMove)
        {
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (Vector2)_player.position - pos;
        float dist = toPlayer.magnitude;
        float speedMult = dist < arrivalRadius ? dist / arrivalRadius : 1f;

        Vector2 desiredVelocity = dist > 0.001f
            ? toPlayer.normalized * speed * speedMult
            : Vector2.zero;

        Vector2 sepForce = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(pos, separationRadius);

        foreach (var col in neighbors)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector2 away = pos - (Vector2)col.transform.position;
            float d = away.magnitude;
            if (d < 0.001f) d = 0.001f;

            sepForce += away.normalized * separationForce * (1f - d / separationRadius);
        }

        desiredVelocity += sepForce;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        _rb.MovePosition(pos + _currentVelocity * Time.fixedDeltaTime);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (_playerHealth == null)
            _playerHealth = collision.GetComponent<PlayerHealth>();

        if (_playerHealth == null) return;
        if (Time.time < _nextDamageTime) return;

        float dmg = _enemyDamage != null ? _enemyDamage.damage : 1f;
        _nextDamageTime = Time.time + damageInterval;
        _playerHealth.TakeDamage(dmg);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        speed = _baseSpeed * multiplier;
        _slowTimer = duration;
    }

    public void OnReceiveDamageBounceReset(float customLockTime = -1f)
    {
        _cycleTime = 0f;
        _cyclePhase = 0f;
        _bouncePhase = 0f;

        _hitGroundLockTimer = customLockTime > 0f ? customLockTime : hitGroundLockTime;

        if (visualRoot != null)
            _visualY = visualRoot.localPosition.y;
        else
            _visualY = 0f;

        _visualYVelocity = 0f;
        _recoveringToGround = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, arrivalRadius);
    }
}