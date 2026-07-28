using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
    [SerializeField] Animator _animator;

    Rigidbody2D _rb;
    Transform _player;
    EnemyHealth _enemyHealth;

    bool _isBusy;
    bool _isDashing;
    bool _dashRoutineRunning;
    bool _dashStartedByAnimation;
    bool _hitSomething;
    bool _isDead;

    Vector2 _storedDashDirection;
    Coroutine _dashCoroutine;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemyHealth = GetComponent<EnemyHealth>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnEnable()
    {
        if (_enemyHealth != null)
            _enemyHealth.SubscribeOnDeath(HandleDeath);
    }

    void OnDisable()
    {
        if (_enemyHealth != null)
            _enemyHealth.UnsubscribeOnDeath(HandleDeath);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
    }

    void FixedUpdate()
    {
        if (_isDead) return;
        if (_player == null) return;
        if (_isBusy || _dashRoutineRunning) return;

        float distance = Vector2.Distance(_rb.position, _player.position);

        if (distance <= dashTriggerDistance)
        {
            _storedDashDirection = ((Vector2)_player.position - _rb.position).normalized;
            _dashCoroutine = StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        _dashRoutineRunning = true;
        _isBusy = true;
        _isDashing = false;
        _dashStartedByAnimation = false;
        _hitSomething = false;

        _rb.linearVelocity = Vector2.zero;

        if (_animator != null)
        {
            _animator.ResetTrigger(dashStartTrigger);
            _animator.SetTrigger(dashStartTrigger);
            _animator.SetBool(isDashingBool, false);
        }

        while (!_dashStartedByAnimation)
        {
            if (_isDead) yield break;
            yield return null;
        }

        if (_isDead) yield break;

        _isDashing = true;

        if (_animator != null)
            _animator.SetBool(isDashingBool, true);

        float elapsed = 0f;

        while (elapsed < dashDuration && !_hitSomething && !_isDead)
        {
            _rb.MovePosition(
                _rb.position + _storedDashDirection * dashSpeed * Time.fixedDeltaTime
            );

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StopDash();

        if (_isDead)
        {
            _dashRoutineRunning = false;
            yield break;
        }

        yield return new WaitForSeconds(recoveryTime);

        _isBusy = false;
        _dashRoutineRunning = false;
        _dashCoroutine = null;
    }

    void StopDash()
    {
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;

        if (_animator != null)
            _animator.SetBool(isDashingBool, false);
    }

    void HandleDeath()
    {
        _isDead = true;
        _isBusy = true;
        _hitSomething = true;
        _dashStartedByAnimation = true;

        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        StopDash();
        _dashRoutineRunning = false;
    }

    public void AnimationEvent_BeginDash()
    {
        if (_isDead) return;
        _dashStartedByAnimation = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isDashing || _isDead) return;

        if (collision.collider.CompareTag("Player"))
        {
            _hitSomething = true;
        }
    }
}