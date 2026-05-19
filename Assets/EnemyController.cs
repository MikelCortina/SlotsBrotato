using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 2.5f;
    public float separationRadius = 1.2f;
    public float separationForce = 6f;
    public float arrivalRadius = 0.7f;
    public int contactDamage = 1;
    public float damageInterval = 0.5f;

    private Rigidbody2D _rb;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private float _nextDamageTime;

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

    void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 pos = _rb.position;
        Vector2 toPlayer = (Vector2)_player.position - pos;
        float dist = toPlayer.magnitude;
        float speedMult = dist < arrivalRadius ? dist / arrivalRadius : 1f;
        Vector2 seekForce = toPlayer.normalized * speed * speedMult;

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

        Vector2 finalVel = seekForce + sepForce;
        _rb.MovePosition(pos + finalVel * Time.fixedDeltaTime);

        if (finalVel.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(finalVel.y, finalVel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        if (_playerHealth == null)
            _playerHealth = collision.collider.GetComponent<PlayerHealth>();

        if (_playerHealth == null) return;
        if (Time.time < _nextDamageTime) return;

        _nextDamageTime = Time.time + damageInterval;
        _playerHealth.TakeDamage(contactDamage);
    }
}