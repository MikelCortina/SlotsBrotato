using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Suavizado")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.1f;

    [Header("Offset")]
    public Vector2 offset = Vector2.zero;

    [Header("Límites del mapa")]
    public Transform boundLeft;
    public Transform boundRight;
    public Transform boundTop;
    public Transform boundBottom;

    [Header("Zoom transición")]
    [SerializeField] private bool transitionZoomActive;

    private Camera cam;

    private float baseOrthographicSize;
    private float transitionStartSize;
    private float transitionTargetSize;
    private float transitionDuration;
    private float transitionElapsed;
    private Vector3 frozenPosition;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam != null)
            baseOrthographicSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        if (transitionZoomActive)
        {
            UpdateTransitionZoomOnly();
            transform.position = frozenPosition;
            return;
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed
        );

        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.orthographicSize * cam.aspect;

        if (boundLeft != null)
            smoothedPosition.x = Mathf.Max(smoothedPosition.x, boundLeft.position.x + halfWidth);

        if (boundRight != null)
            smoothedPosition.x = Mathf.Min(smoothedPosition.x, boundRight.position.x - halfWidth);

        if (boundBottom != null)
            smoothedPosition.y = Mathf.Max(smoothedPosition.y, boundBottom.position.y + halfHeight);

        if (boundTop != null)
            smoothedPosition.y = Mathf.Min(smoothedPosition.y, boundTop.position.y - halfHeight);

        transform.position = smoothedPosition;
    }

    void UpdateTransitionZoomOnly()
    {
        if (!transitionZoomActive || cam == null)
            return;

        if (transitionDuration <= 0f)
        {
            cam.orthographicSize = transitionTargetSize;
            transitionZoomActive = false;
            return;
        }

        transitionElapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(transitionElapsed / transitionDuration);
        float easedT = 1f - Mathf.Pow(1f - t, 3f);

        cam.orthographicSize = Mathf.Lerp(transitionStartSize, transitionTargetSize, easedT);

        if (t >= 1f)
            transitionZoomActive = false;
    }

    public void StartTransitionZoomOut(float extraSize, float duration)
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null)
            return;

        frozenPosition = transform.position;
        transitionStartSize = baseOrthographicSize;
        transitionTargetSize = baseOrthographicSize + extraSize;
        transitionDuration = duration;
        transitionElapsed = 0f;
        transitionZoomActive = true;

        cam.orthographicSize = baseOrthographicSize;
    }

    public void ResetZoomImmediate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam == null)
            return;

        transitionZoomActive = false;
        cam.orthographicSize = baseOrthographicSize;
    }

    public bool IsTransitionZoomActive()
    {
        return transitionZoomActive;
    }

    void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();

        Gizmos.color = Color.cyan;

        if (boundLeft)
            Gizmos.DrawLine(new Vector3(boundLeft.position.x, -500, 0), new Vector3(boundLeft.position.x, 500, 0));

        if (boundRight)
            Gizmos.DrawLine(new Vector3(boundRight.position.x, -500, 0), new Vector3(boundRight.position.x, 500, 0));

        if (boundTop)
            Gizmos.DrawLine(new Vector3(-500, boundTop.position.y, 0), new Vector3(500, boundTop.position.y, 0));

        if (boundBottom)
            Gizmos.DrawLine(new Vector3(-500, boundBottom.position.y, 0), new Vector3(500, boundBottom.position.y, 0));
    }
}