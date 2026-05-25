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

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null) return;

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

        // Calcular el semitamaño de la cámara ortográfica
        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.orthographicSize * cam.aspect;

        // Aplicar límites usando la posición de cada Transform
        if (boundLeft != null) smoothedPosition.x = Mathf.Max(smoothedPosition.x, boundLeft.position.x + halfWidth);
        if (boundRight != null) smoothedPosition.x = Mathf.Min(smoothedPosition.x, boundRight.position.x - halfWidth);
        if (boundBottom != null) smoothedPosition.y = Mathf.Max(smoothedPosition.y, boundBottom.position.y + halfHeight);
        if (boundTop != null) smoothedPosition.y = Mathf.Min(smoothedPosition.y, boundTop.position.y - halfHeight);

        transform.position = smoothedPosition;
    }

    // Dibuja los límites en el editor para facilitar el posicionado
    void OnDrawGizmos()
    {
        if (cam == null) cam = GetComponent<Camera>();
        float halfHeight = cam != null ? cam.orthographicSize : 5f;
        float halfWidth = cam != null ? cam.orthographicSize * cam.aspect : 8f;

        Gizmos.color = Color.cyan;

        if (boundLeft) Gizmos.DrawLine(new Vector3(boundLeft.position.x, -500, 0), new Vector3(boundLeft.position.x, 500, 0));
        if (boundRight) Gizmos.DrawLine(new Vector3(boundRight.position.x, -500, 0), new Vector3(boundRight.position.x, 500, 0));
        if (boundTop) Gizmos.DrawLine(new Vector3(-500, boundTop.position.y, 0), new Vector3(500, boundTop.position.y, 0));
        if (boundBottom) Gizmos.DrawLine(new Vector3(-500, boundBottom.position.y, 0), new Vector3(500, boundBottom.position.y, 0));
    }
}