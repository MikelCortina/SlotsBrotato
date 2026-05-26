using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 18f;
    public float deceleration = 22f;

    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Vector2 _input;
    private Vector2 _currentVelocity;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        float moveSpeed = _stats != null ? _stats.GetMoveSpeed() : 5f;

        Vector2 targetVelocity = _input * moveSpeed;

        float accelRate = (_input.sqrMagnitude > 0.01f)
            ? acceleration
            : deceleration;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            targetVelocity,
            accelRate * Time.fixedDeltaTime
        );

        _rb.linearVelocity = _currentVelocity;
    }
}