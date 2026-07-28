using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class SpitterEnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.6f;
    public float acceleration = 14f;
    public float keepDistance = 4.5f;
    public float distanceTolerance = 1.4f;

    [Header("Separation")]
    public float separationRadius = 1.1f;
    public float separationForce = 4f;

    [Header("Shoot")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootInterval = 2f;
    public float projectileDamage = 1f;

    [Header("Attack Pause")]
    public float attackPauseDuration = 0.25f;
    public string prepareShootTriggerName = "PrepareShoot";
    public string attackTriggerName = "Attack";

    [Header("Bounce")]
    public float bounceHeight = 0.22f;
    public float bounceFrequency = 1.6f;
    [Range(0f, 1f)] public float idlePortion = 0.35f;
    [Range(0f, 1f)] public float movePortion = 0.65f;
    public Transform visualRoot;

    [Header("Animator")]
    public Animator animatorOverride;

    [Header("Animator State Names")]
    public string locomotionStateName = "Locomotion";
    public string hitStateName = "Hit";
    public int animatorLayer = 0;

    [Header("Optional")]
    public bool flipVisualWithMovement = false;

    Transform _player;
    Rigidbody2D _rb;
    Animator _animator;
    EnemyHealth _health;

    float _nextShot;
    bool _wantsToAttack;
    bool _isPreparingAttack;
    bool _isAttacking;
    bool _wasInHitLastFrame;
    Coroutine _attackRoutine;

    Vector2 _currentVelocity;

    float _cycleTime;
    float _cyclePhase;
    float _bouncePhase;
    float _visualY;
    float _baseVisualY;
    float _frozenVisualY;

    int _hitStateHash;
    int _locomotionStateHash;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<EnemyHealth>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        if (animatorOverride != null)
        {
            _animator = animatorOverride;
        }
        else
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);
        }

        _hitStateHash = Animator.StringToHash(hitStateName);
        _locomotionStateHash = Animator.StringToHash(locomotionStateName);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;

        float cycleDuration = 1f / Mathf.Max(0.01f, bounceFrequency);
        _cycleTime = Random.Range(0f, cycleDuration);

        if (visualRoot != null)
        {
            _baseVisualY = visualRoot.localPosition.y;
            _visualY = _baseVisualY;
            _frozenVisualY = _visualY;
        }

        _nextShot = Time.time + shootInterval;
    }

    void Update()
    {
        if (_player == null)
            return;

        if (IsDead())
        {
            StopAllCombatLogic();
            return;
        }

        HandleHitRecovery();
        UpdateBounceVisual();

        if (IsInHitState())
            return;

        if (!_wantsToAttack && !_isPreparingAttack && !_isAttacking && _attackRoutine == null && Time.time >= _nextShot)
            _wantsToAttack = true;

        if (_wantsToAttack && !_isPreparingAttack && !_isAttacking && _attackRoutine == null && IsAtJumpCycleStart())
            _attackRoutine = StartCoroutine(StartAttackRoutine());
    }

    void FixedUpdate()
    {
        if (_player == null)
            return;

        if (IsDead())
        {
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 pos = _rb.position;

        if (IsInHitState())
        {
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        if (_isPreparingAttack || _isAttacking)
        {
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        bool canMoveThisFrame = IsAirborneMovePhase();

        if (!canMoveThisFrame)
        {
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
            UpdateFacing();
            return;
        }

        Vector2 toPlayer = (Vector2)_player.position - pos;
        float distance = toPlayer.magnitude;

        if (distance <= 0.001f)
        {
            _currentVelocity = Vector2.zero;
            return;
        }

        Vector2 dirToPlayer = toPlayer.normalized;
        Vector2 desiredVelocity = GetRangedDesiredVelocity(dirToPlayer, distance);
        desiredVelocity += GetSeparationForce(pos);

        _currentVelocity = Vector2.MoveTowards(
            _currentVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        _rb.MovePosition(pos + _currentVelocity * Time.fixedDeltaTime);

        UpdateFacing();
    }

    bool IsDead()
    {
        return _health != null && _health.IsDead;
    }

    void StopAllCombatLogic()
    {
        _wantsToAttack = false;
        _isPreparingAttack = false;
        _isAttacking = false;
        _wasInHitLastFrame = false;
        _currentVelocity = Vector2.zero;

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
    }

    void HandleHitRecovery()
    {
        if (_animator == null || IsDead())
            return;

        bool isInHit = IsInHitState();

        if (isInHit)
        {
            _wasInHitLastFrame = true;

            _currentVelocity = Vector2.zero;
            _isPreparingAttack = false;
            _isAttacking = false;

            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }

            return;
        }

        if (_wasInHitLastFrame)
        {
            _wasInHitLastFrame = false;

            if (IsDead())
                return;

            _isPreparingAttack = false;
            _isAttacking = false;
            _currentVelocity = Vector2.zero;

            _animator.Play(_locomotionStateHash, animatorLayer, 0f);
            _animator.Update(0f);

            _nextShot = Time.time + 0.15f;
        }
    }

    bool IsInHitState()
    {
        if (_animator == null || IsDead())
            return false;

        if (_animator.IsInTransition(animatorLayer))
            return false;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(animatorLayer);
        return stateInfo.shortNameHash == _hitStateHash;
    }

    bool IsAirborneMovePhase()
    {
        return _cyclePhase >= idlePortion && _bouncePhase > 0.01f;
    }

    bool IsAtJumpCycleStart()
    {
        return _cyclePhase <= 0.02f;
    }

    Vector2 GetRangedDesiredVelocity(Vector2 dirToPlayer, float distance)
    {
        float minDistance = keepDistance - distanceTolerance;
        float maxDistance = keepDistance + distanceTolerance;

        if (distance > maxDistance)
            return dirToPlayer * moveSpeed;

        if (distance < minDistance)
            return -dirToPlayer * moveSpeed;

        Vector2 perpendicular = new Vector2(-dirToPlayer.y, dirToPlayer.x);
        float sideSign = Mathf.Sin(Time.time * 0.8f + GetInstanceID() * 0.01f) >= 0f ? 1f : -1f;

        Vector2 mixedDir = (dirToPlayer * 0.55f + perpendicular * 0.45f * sideSign).normalized;
        return mixedDir * moveSpeed;
    }

    Vector2 GetSeparationForce(Vector2 pos)
    {
        Vector2 total = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(pos, separationRadius);

        foreach (var col in neighbors)
        {
            if (col.gameObject == gameObject)
                continue;

            if (!col.CompareTag("Enemy"))
                continue;

            Vector2 away = pos - (Vector2)col.transform.position;
            float dist = away.magnitude;

            if (dist < 0.001f)
                dist = 0.001f;

            total += away.normalized * separationForce * (1f - dist / separationRadius);
        }

        return total;
    }

    void UpdateBounceVisual()
    {
        if (_isPreparingAttack || _isAttacking)
        {
            _visualY = _frozenVisualY;
            ApplyVisualY(false);
            return;
        }

        float cycleDuration = 1f / Mathf.Max(0.01f, bounceFrequency);
        _cycleTime += Time.deltaTime;
        _cyclePhase = (_cycleTime / cycleDuration) % 1f;

        bool isMovingPhase = _cyclePhase >= idlePortion;

        if (isMovingPhase)
        {
            float normalizedMovePortion = Mathf.Max(0.0001f, movePortion);
            float movePhase = (_cyclePhase - idlePortion) / normalizedMovePortion;
            movePhase = Mathf.Clamp01(movePhase);
            _bouncePhase = Mathf.Sin(movePhase * Mathf.PI);
        }
        else
        {
            _bouncePhase = 0f;
        }

        _visualY = _baseVisualY + (_bouncePhase * bounceHeight);
        _frozenVisualY = _visualY;

        bool shouldDriveAnimatorBounce = !IsInHitState();
        ApplyVisualY(shouldDriveAnimatorBounce);
    }

    void ApplyVisualY(bool updateAnimatorBounce)
    {
        if (visualRoot != null)
        {
            Vector3 local = visualRoot.localPosition;
            local.y = _visualY;
            visualRoot.localPosition = local;
        }

        if (_animator != null && updateAnimatorBounce && !IsDead())
        {
            float normalizedBounce = bounceHeight > 0.0001f
                ? Mathf.Clamp01((_visualY - _baseVisualY) / bounceHeight)
                : 0f;

            _animator.SetFloat("bouncePhase", normalizedBounce);
        }
    }

    void UpdateFacing()
    {
        if (!flipVisualWithMovement || visualRoot == null)
            return;

        if (_currentVelocity.x > 0.05f)
            visualRoot.localScale = new Vector3(1f, 1f, 1f);
        else if (_currentVelocity.x < -0.05f)
            visualRoot.localScale = new Vector3(-1f, 1f, 1f);
    }

    IEnumerator StartAttackRoutine()
    {
        _wantsToAttack = false;
        _isPreparingAttack = true;
        _isAttacking = false;

        _currentVelocity = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
        _frozenVisualY = _visualY;

        if (_animator != null && !string.IsNullOrEmpty(prepareShootTriggerName) && !IsDead())
        {
            _animator.ResetTrigger(prepareShootTriggerName);
            _animator.ResetTrigger(attackTriggerName);
            _animator.SetTrigger(prepareShootTriggerName);
        }

        yield return new WaitForSeconds(attackPauseDuration);

        if (IsInHitState() || IsDead())
        {
            _isPreparingAttack = false;
            _isAttacking = false;
            _attackRoutine = null;
            yield break;
        }

        _isPreparingAttack = false;
        _isAttacking = true;
        _frozenVisualY = _visualY;

        if (_animator != null && !string.IsNullOrEmpty(attackTriggerName) && !IsDead())
        {
            _animator.ResetTrigger(prepareShootTriggerName);
            _animator.SetTrigger(attackTriggerName);
        }

        _attackRoutine = null;
    }

    public void ShootFromAnimation()
    {
        if (IsInHitState() || IsDead())
            return;

        Shoot();
    }

    public void EndAttackAnimation()
    {
        if (IsDead())
            return;

        _isAttacking = false;
        _isPreparingAttack = false;
        _attackRoutine = null;
        _nextShot = Time.time + shootInterval;
    }

    void Shoot()
    {
        if (projectilePrefab == null || _player == null || IsDead())
            return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (firePoint != null)
        {
            spawnPos = firePoint.position;
            spawnRot = firePoint.rotation;
        }
        else
        {
            Vector2 dirFallback = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            spawnPos = transform.position + (Vector3)(dirFallback * 0.6f);
            spawnRot = Quaternion.identity;
        }

        Vector2 dir = ((Vector2)_player.position - (Vector2)spawnPos).normalized;

        GameObject go = Instantiate(projectilePrefab, spawnPos, spawnRot);

        EnemyProjectile projectile = go.GetComponent<EnemyProjectile>();
        if (projectile != null)
            projectile.Init(dir, projectileDamage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, keepDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.08f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 0.4f);
        }
    }
}