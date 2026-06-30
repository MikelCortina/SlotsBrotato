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
    public Transform visualRoot; // Arrastra aquí el hijo con el SpriteRenderer y Animator

    float _baseSpeed;
    float _slowTimer;
    float _bounceTime;

    Rigidbody2D _rb;
    Animator _animator;
    Transform _player;
    PlayerHealth _playerHealth;
    EnemyDamage _enemyDamage;
    float _nextDamageTime;
    bool _isKnockedBack;
    Vector2 _currentVelocity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _enemyDamage = GetComponent<EnemyDamage>();

        // Si no asignas visualRoot en el Inspector, busca el Animator en el hijo
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
    }

    void Update()
    {
        // Slow timer
        if (_slowTimer > 0f)
        {
            _slowTimer -= Time.deltaTime;
            if (_slowTimer <= 0f)
                speed = _baseSpeed;
        }

        // Rebote visual — solo mueve el hijo, el padre (física) no se toca
        _bounceTime += Time.deltaTime * bounceFrequency * Mathf.PI * 2f;
        float normalizedSine = (Mathf.Sin(_bounceTime) + 1f) * 0.5f; // 0 = suelo, 1 = cima

        if (visualRoot != null)
            visualRoot.localPosition = new Vector3(0f, normalizedSine * bounceHeight, 0f);

        if (_animator != null)
            _animator.SetFloat("bouncePhase", normalizedSine);
    }

    public void SetKnockback(bool value)
    {
        _isKnockedBack = value;
        if (value)
            _currentVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (_isKnockedBack) return;
        if (_player == null) return;

        Vector2 pos = _rb.position;
        Vector2 toPlayer = (Vector2)_player.position - pos;
        float dist = toPlayer.magnitude;
        float speedMult = dist < arrivalRadius ? dist / arrivalRadius : 1f;

        Vector2 desiredVelocity = toPlayer.normalized * speed * speedMult;

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
        Debug.Log($"Damage: {dmg} by {gameObject.name}");
    }

    public void ApplySlow(float multiplier, float duration)
    {
        speed = _baseSpeed * multiplier;
        _slowTimer = duration;
    }
}