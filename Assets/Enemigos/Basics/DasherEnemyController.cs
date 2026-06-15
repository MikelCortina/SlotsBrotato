using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DasherEnemyController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float acceleration = 8f;

    [Header("Dash")]
    public float dashTriggerDistance = 5f;
    public float chargeTime = 1f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float recoveryTime = 1f;

    Rigidbody2D _rb;
    Transform _player;
    Vector2 _currentVelocity;
    bool _isDashing;
    bool _isBusy;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            _player = p.transform;
    }

    void FixedUpdate()
    {
        if (_player == null) return;
        if (_isBusy || _isDashing) return;

        Vector2 pos = _rb.position;
        Vector2 toPlayer = (Vector2)_player.position - pos;
        float distance = toPlayer.magnitude;

        if (distance <= dashTriggerDistance)
        {
            StartCoroutine(DashRoutine(toPlayer.normalized));
            return;
        }

        Vector2 desiredVelocity = toPlayer.normalized * moveSpeed;

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        _rb.MovePosition(pos + _currentVelocity * Time.fixedDeltaTime);
    }

    IEnumerator DashRoutine(Vector2 dashDirection)
    {
        _isBusy = true;
        _currentVelocity = Vector2.zero;

        yield return new WaitForSeconds(chargeTime);

        _isDashing = true;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            _rb.MovePosition(
                _rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _isDashing = false;

        yield return new WaitForSeconds(recoveryTime);

        _isBusy = false;
    }
}