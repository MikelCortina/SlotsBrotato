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
    public int contactDamage = 1;
    public float damageInterval = 0.5f;

    private Rigidbody2D _rb;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private float _nextDamageTime;
    private bool _isKnockedBack;
    private Vector2 _currentVelocity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            _player = p.transform;
            _playerHealth = p.GetComponent<PlayerHealth>();
        }
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

        if (_currentVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_currentVelocity.y, _currentVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (_playerHealth == null)
            _playerHealth = collision.GetComponent<PlayerHealth>();

        if (_playerHealth == null) return;
        if (Time.time < _nextDamageTime) return;

        _nextDamageTime = Time.time + damageInterval;
        _playerHealth.TakeDamage(contactDamage);
    }
}