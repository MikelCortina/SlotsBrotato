using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 18f;
    public float deceleration = 22f;

    public bool IsMovementLocked { get; set; } = false;

    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _input;
    private Vector2 _currentVelocity;

    private int _lastHorizontalDirection = 1; // 1 derecha, -1 izquierda
    private bool _wasMovingLastFrame = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (IsMovementLocked)
        {
            _input = Vector2.zero;
            UpdateAnimationAndFlip();
            return;
        }

        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateAnimationAndFlip();
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

    void UpdateAnimationAndFlip()
    {
        bool isMoving = _input.sqrMagnitude > 0.01f;
        bool startedMovingThisFrame = !_wasMovingLastFrame && isMoving;

        bool hasHorizontal = Mathf.Abs(_input.x) > 0.01f;
        bool isMovingDown = _input.y < -0.01f;
        bool isMovingUp = _input.y > 0.01f;

        if (_input.x > 0.01f)
            _lastHorizontalDirection = 1;
        else if (_input.x < -0.01f)
            _lastHorizontalDirection = -1;

        _spriteRenderer.flipX = (_lastHorizontalDirection == -1);

        bool isMovingSide = isMoving && (
            (hasHorizontal && !isMovingDown && !isMovingUp) ||
            (!hasHorizontal && !isMovingDown && !isMovingUp)
        );

        bool isMovingDownSide = isMoving && isMovingDown;
        bool isMovingUpSide = isMoving && isMovingUp;

        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("IsMovingSide", isMovingSide);
        _animator.SetBool("IsMovingDownSide", isMovingDownSide);
        _animator.SetBool("IsMovingUpSide", isMovingUpSide);

        if (startedMovingThisFrame)
        {
            _animator.ResetTrigger("StartMoveSide");
            _animator.ResetTrigger("StartMoveDownSide");
            _animator.ResetTrigger("StartMoveUpSide");

            if (isMovingDown)
            {
                _animator.SetTrigger("StartMoveDownSide");
            }
            else if (isMovingUp)
            {
                _animator.SetTrigger("StartMoveUpSide");
            }
            else
            {
                _animator.SetTrigger("StartMoveSide");
            }
        }

        _wasMovingLastFrame = isMoving;
    }
}