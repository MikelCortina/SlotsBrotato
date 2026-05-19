using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 2.5f;
    public float separationRadius = 1.2f;
    public float separationForce = 6f;
    public float arrivalRadius = 0.7f;

    private Rigidbody2D _rb;
    private Transform _player;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    void Start()
    {
        // Cache player
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void FixedUpdate()
    {
        if (_player == null) return;

        Vector2 pos = _rb.position;

        // 1. Seek hacia el jugador
        Vector2 toPlayer = (Vector2)_player.position - pos;
        float dist = toPlayer.magnitude;
        float speedMult = dist < arrivalRadius ? dist / arrivalRadius : 1f;
        Vector2 seekForce = toPlayer.normalized * speed * speedMult;

        // 2. Separación con otros enemigos
        Vector2 sepForce = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(pos, separationRadius);
        foreach (var col in neighbors)
        {
            if (col.gameObject == gameObject) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector2 away = pos - (Vector2)col.transform.position;
            float d = away.magnitude;
            if (d < 0.001f) d = 0.001f;
            // Fuerza inversamente proporcional a la distancia
            sepForce += away.normalized * separationForce * (1f - d / separationRadius);
        }

        // 3. Combinar fuerzas y mover
        Vector2 finalVel = seekForce + sepForce;
        _rb.MovePosition(pos + finalVel * Time.fixedDeltaTime);

        // 4. Rotar hacia dirección de movimiento
        if (finalVel.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(finalVel.y, finalVel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}