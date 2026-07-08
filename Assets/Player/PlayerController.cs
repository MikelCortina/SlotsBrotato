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

    [Header("Local Flip Children")]
    [SerializeField] private List<Transform> childrenToMirrorLocally = new List<Transform>();
    [SerializeField] private bool mirrorChildScaleX = false;
    [SerializeField] private float rightFacingXOffset = 0f;
    [SerializeField] private float leftFacingXOffset = 0f;

    public bool IsMovementLocked { get; set; } = false;

    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Animator _animator;

    private Vector2 _input;
    private Vector2 _currentVelocity;
    private bool _isFacingLeft = false;

    private Dictionary<Transform, Vector3> _initialLocalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> _initialLocalScales = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();

        if (mainSpriteRenderer == null)
            mainSpriteRenderer = GetComponent<SpriteRenderer>();

        CacheChildrenLocalData();
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

    void CacheChildrenLocalData()
    {
        _initialLocalPositions.Clear();
        _initialLocalScales.Clear();

        for (int i = 0; i < childrenToMirrorLocally.Count; i++)
        {
            Transform child = childrenToMirrorLocally[i];
            if (child == null)
                continue;

            _initialLocalPositions[child] = child.localPosition;
            _initialLocalScales[child] = child.localScale;
        }
    }

    void UpdateAnimationAndFlip()
    {
        bool isMoving = _input.sqrMagnitude > 0.01f;

        UpdateFlipFromMouse();
        _animator.SetBool("IsMoving", isMoving);
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

        ApplyLocalMirrorToChildren();
    }

    void ApplyLocalMirrorToChildren()
    {
        float direction = _isFacingLeft ? 1f : -1f;
        float extraOffset = _isFacingLeft ? leftFacingXOffset : rightFacingXOffset;

        foreach (Transform child in childrenToMirrorLocally)
        {
            if (child == null || !_initialLocalPositions.ContainsKey(child))
                continue;

            Vector3 baseLocalPos = _initialLocalPositions[child];

            child.localPosition = new Vector3(
                Mathf.Abs(baseLocalPos.x) * direction + extraOffset,
                baseLocalPos.y,
                baseLocalPos.z
            );

            if (mirrorChildScaleX && _initialLocalScales.ContainsKey(child))
            {
                Vector3 baseScale = _initialLocalScales[child];
                child.localScale = new Vector3(
                    Mathf.Abs(baseScale.x) * direction,
                    baseScale.y,
                    baseScale.z
                );
            }
        }
    }
}