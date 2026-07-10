using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 18f;
    public float deceleration = 22f;

    [Header("Mouse from RawImage")]
    [SerializeField] private RawImage gameplayRawImage;
    [SerializeField] private Camera renderTextureCamera;
    [SerializeField] private Canvas canvas;

    public bool IsMovementLocked { get; set; } = false;
    public bool IsFacingLeft { get; private set; }

    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Animator _animator;

    private Vector2 _input;
    private Vector2 _currentVelocity;
    private Vector3 _initialScale;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();
        _initialScale = transform.localScale;
    }

    void Update()
    {
        if (IsMovementLocked)
        {
            _input = Vector2.zero;
            UpdateAnimation();
            UpdateFlipByMouse();
            return;
        }

        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateAnimation();
        UpdateFlipByMouse();
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

    void UpdateAnimation()
    {
        bool isMoving = _input.sqrMagnitude > 0.01f;
        _animator.SetBool("IsMoving", isMoving);
    }

    void UpdateFlipByMouse()
    {
        if (!TryGetMouseWorldPositionFromRawImage(out Vector3 mouseWorld))
            return;

        IsFacingLeft = mouseWorld.x < transform.position.x;

        Vector3 scale = transform.localScale;
        scale.x = IsFacingLeft
            ? -Mathf.Abs(_initialScale.x)
            : Mathf.Abs(_initialScale.x);

        transform.localScale = scale;
    }

    bool TryGetMouseWorldPositionFromRawImage(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        if (gameplayRawImage == null || renderTextureCamera == null)
            return false;

        RectTransform rt = gameplayRawImage.rectTransform;

        Camera uiCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt,
                Input.mousePosition,
                uiCamera,
                out Vector2 localPoint))
            return false;

        if (!rt.rect.Contains(localPoint))
            return false;

        Vector2 point01 = localPoint - rt.rect.min;
        float u = point01.x / rt.rect.width;
        float v = point01.y / rt.rect.height;

        Rect uv = gameplayRawImage.uvRect;
        u = uv.x + u * uv.width;
        v = uv.y + v * uv.height;

        worldPos = renderTextureCamera.ViewportToWorldPoint(new Vector3(
            u,
            v,
            Mathf.Abs(renderTextureCamera.transform.position.z)
        ));

        return true;
    }
}