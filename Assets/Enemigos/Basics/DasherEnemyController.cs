using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class DasherEnemyController : MonoBehaviour
{
    [Header("Dash")]
    public float dashTriggerDistance = 5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float recoveryTime = 1f;

    [Header("Animation")]
    public string dashStartTrigger = "DashStart";
    public string isDashingBool = "IsDashing";

    Rigidbody2D _rb;
    Animator _animator;
    Transform _player;

    bool _isBusy;
    bool _isDashing;
    bool _dashRoutineRunning;
    bool _dashStartedByAnimation;

    Vector2 _storedDashDirection;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

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
        if (_isBusy || _dashRoutineRunning) return;

        float distance = Vector2.Distance(_rb.position, _player.position);

        if (distance <= dashTriggerDistance)
        {
            _storedDashDirection = ((Vector2)_player.position - _rb.position).normalized;
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        _dashRoutineRunning = true;
        _isBusy = true;
        _isDashing = false;
        _dashStartedByAnimation = false;

        _rb.linearVelocity = Vector2.zero;

        if (_animator != null)
        {
            _animator.ResetTrigger(dashStartTrigger);
            _animator.SetTrigger(dashStartTrigger);
            _animator.SetBool(isDashingBool, false);
        }

        while (!_dashStartedByAnimation)
            yield return null;

        _isDashing = true;

        if (_animator != null)
            _animator.SetBool(isDashingBool, true);

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            _rb.MovePosition(
                _rb.position + _storedDashDirection * dashSpeed * Time.fixedDeltaTime
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;

        if (_animator != null)
            _animator.SetBool(isDashingBool, false);

        yield return new WaitForSeconds(recoveryTime);

        _isBusy = false;
        _dashRoutineRunning = false;
    }

    public void AnimationEvent_BeginDash()
    {
        _dashStartedByAnimation = true;
    }
}