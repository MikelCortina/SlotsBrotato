using UnityEngine;
using UnityEngine.UI;

public class WeaponPivotAim : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private RectTransform renderTextureRect;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Canvas uiCanvas;

    void LateUpdate()
    {
        if (weaponPivot == null || gameCamera == null || renderTextureRect == null)
            return;

        if (!TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)weaponPivot.position);
        if (dir.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public bool TryGetMouseWorldPosition(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        Vector2 screenMouse = Input.mousePosition;

        Camera uiCam = null;
        if (uiCanvas != null && uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCam = uiCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                renderTextureRect,
                screenMouse,
                uiCam,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = renderTextureRect.rect;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        Vector3 viewportPoint = new Vector3(u, v, gameCamera.nearClipPlane);
        worldPos = gameCamera.ViewportToWorldPoint(viewportPoint);
        worldPos.z = 0f;

        return true;
    }
}