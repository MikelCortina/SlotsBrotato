using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 18f;
    public float deceleration = 22f;

    [Header("Aim")]
    [SerializeField] private WeaponPivotAim weaponAim;

    [Header("Flip")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private List<SpriteRenderer> spritesToFlip = new List<SpriteRenderer>();
    [SerializeField] private float flipDeadZone = 0.15f;

    public bool IsMovementLocked { get; set; } = false;

    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Animator _animator;

    private Vector2 _input;
    private Vector2 _currentVelocity;
    private bool _wasMovingLastFrame = false;
    private bool _isFacingLeft = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();

        if (mainSpriteRenderer == null)
            mainSpriteRenderer = GetComponent<SpriteRenderer>();
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

        UpdateFlipFromMouse();

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
                _animator.SetTrigger("StartMoveDownSide");
            else if (isMovingUp)
                _animator.SetTrigger("StartMoveUpSide");
            else
                _animator.SetTrigger("StartMoveSide");
        }

        _wasMovingLastFrame = isMoving;
    }

    void UpdateFlipFromMouse()
    {
        if (weaponAim == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        float deltaX = mouseWorld.x - transform.position.x;

        if (deltaX < -flipDeadZone)
            _isFacingLeft = true;
        else if (deltaX > flipDeadZone)
            _isFacingLeft = false;

        if (mainSpriteRenderer != null)
            mainSpriteRenderer.flipX = _isFacingLeft;

        for (int i = 0; i < spritesToFlip.Count; i++)
        {
            if (spritesToFlip[i] != null)
                spritesToFlip[i].flipX = _isFacingLeft;
        }
    }
}