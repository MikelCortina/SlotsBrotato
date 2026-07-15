using UnityEngine;

public class RotateToMouseRenderTexture : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponPivotAim weaponAim;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Rotación")]
    [SerializeField] private float angleOffset = 0f;
    [SerializeField] private float maxUpAngle = 60f;
    [SerializeField] private float maxDownAngle = 60f;

    [Header("Offset progresivo")]
    [SerializeField] private float upOffsetX = 0.02f;
    [SerializeField] private float upOffsetY = 0.04f;
    [SerializeField] private float downOffsetX = 0.02f;
    [SerializeField] private float downOffsetY = -0.06f;

    [Header("Recoil")]
    [SerializeField] private bool enableRecoil = true;
    [SerializeField] private float recoilDistanceX = -0.08f;
    [SerializeField] private float recoilDistanceY = 0.02f;
    [SerializeField] private float recoilReturnSpeed = 18f;
    [SerializeField] private float recoilSnappiness = 30f;
    [SerializeField] private float recoilAngleKick = 6f;
    [SerializeField] private float recoilAngleReturnSpeed = 16f;
    [SerializeField] private float recoilAngleSnappiness = 28f;

    private Vector3 _initialLocalPosition;
    private Vector3 _currentRecoilOffset;
    private Vector3 _targetRecoilOffset;

    private float _currentRecoilAngle;
    private float _targetRecoilAngle;

    void Awake()
    {
        _initialLocalPosition = transform.localPosition;

        if (playerShooter == null)
            playerShooter = GetComponentInParent<PlayerShooter>();
    }

    void OnEnable()
    {
        if (playerShooter == null)
            playerShooter = GetComponentInParent<PlayerShooter>();

        if (playerShooter != null)
            playerShooter.OnShoot += TriggerRecoil;
    }

    void OnDisable()
    {
        if (playerShooter != null)
            playerShooter.OnShoot -= TriggerRecoil;
    }

    void LateUpdate()
    {
        if (weaponAim == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        bool parentFacingLeft = transform.parent != null && transform.parent.localScale.x < 0f;

        Vector3 targetWorld = mouseWorld;

        if (parentFacingLeft)
        {
            float mirroredX = transform.position.x + (transform.position.x - mouseWorld.x);
            float mirroredY = transform.position.y + (transform.position.y - mouseWorld.y);
            targetWorld = new Vector3(mirroredX, mirroredY, mouseWorld.z);
        }

        Vector2 dir = targetWorld - transform.position;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float finalAngle = Mathf.Clamp(rawAngle, -maxDownAngle, maxUpAngle);

        if (parentFacingLeft)
            finalAngle = -finalAngle;

        UpdateRecoil();

        transform.localRotation = Quaternion.Euler(
        0f,
        0f,
        finalAngle + angleOffset + _currentRecoilAngle
        );

        float upT = 0f;
        float downT = 0f;

        if (finalAngle > 0f)
            upT = Mathf.InverseLerp(0f, maxUpAngle, finalAngle);
        else if (finalAngle < 0f)
            downT = Mathf.InverseLerp(0f, -maxDownAngle, finalAngle);

        Vector3 targetLocalPos = _initialLocalPosition;

        float facingSign = 1f;
        if (transform.parent != null)
            facingSign = Mathf.Sign(transform.parent.localScale.x);

        targetLocalPos.x += upOffsetX * upT * facingSign;
        targetLocalPos.y += upOffsetY * upT;

        targetLocalPos.x += downOffsetX * downT * facingSign;
        targetLocalPos.y += downOffsetY * downT;

        if (enableRecoil)
        {
            targetLocalPos += new Vector3(
                _currentRecoilOffset.x * facingSign,
                _currentRecoilOffset.y,
                0f
            );
        }

        transform.localPosition = targetLocalPos;
    }

    void UpdateRecoil()
    {
        if (!enableRecoil)
            return;

        _targetRecoilOffset = Vector3.Lerp(
        _targetRecoilOffset,
        Vector3.zero,
        recoilReturnSpeed * Time.deltaTime
        );

        _currentRecoilOffset = Vector3.Lerp(
        _currentRecoilOffset,
        _targetRecoilOffset,
        recoilSnappiness * Time.deltaTime
        );

        _targetRecoilAngle = Mathf.Lerp(
        _targetRecoilAngle,
        0f,
        recoilAngleReturnSpeed * Time.deltaTime
        );

        _currentRecoilAngle = Mathf.Lerp(
        _currentRecoilAngle,
        _targetRecoilAngle,
        recoilAngleSnappiness * Time.deltaTime
        );
    }

    public void TriggerRecoil()
    {
        if (!enableRecoil)
            return;

        _targetRecoilOffset += new Vector3(
            recoilDistanceX,
            recoilDistanceY,
            0f
        );

        _targetRecoilAngle += recoilAngleKick;
    }
}