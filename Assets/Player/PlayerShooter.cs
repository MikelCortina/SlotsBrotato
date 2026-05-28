using UnityEngine;
using UnityEngine.UI;

public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo")]
    public float fireRate = 2f;
    public float damage = 25f;
    public float bulletSpeed = 12f;
    public GameObject bulletPrefab;

    [Header("Multi-disparo")]
    public int bulletsPerShot = 1;

    [Header("Shotgun")]
    public float spreadAngle = 15f;

    [Header("RenderTexture Aim")]
    public RectTransform renderTextureRect;
    public Camera gameCamera;
    public Canvas uiCanvas;

    [Header("Control")]
    public bool autoFire = true;

    private float _fireTimer;
    private PlayerStats _stats;
    private WeaponData _currentWeapon;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public void ApplyWeaponData(WeaponData weapon)
    {
        if (weapon == null) return;

        _currentWeapon = weapon;
        fireRate = weapon.fireRate;
        damage = weapon.damage;
        bulletSpeed = weapon.bulletSpeed;
        bulletsPerShot = weapon.bulletsPerShot;
        bulletPrefab = weapon.bulletPrefab;
        spreadAngle = weapon.spreadAngle;

        Debug.Log($"Arma equipada: {weapon.weaponName}");
    }

    void Update()
    {
        _fireTimer -= Time.deltaTime;

        bool shootPressed = autoFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (shootPressed && _fireTimer <= 0f)
        {
            Shoot();

            float finalFireRate = fireRate;
            if (_stats != null)
                finalFireRate = _stats.GetFireRate(fireRate);

            _fireTimer = 1f / finalFireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || renderTextureRect == null || gameCamera == null) return;

        if (!TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        Vector2 baseDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = Vector2.right;

        if (_currentWeapon != null &&
            _currentWeapon.weaponType == WeaponType.Boomerang)
        {
            ShootBoomerang(baseDir);
            return;
        }

        ShootProjectileWeapon(baseDir);
    }

    bool TryGetMouseWorldPosition(out Vector3 worldPos)
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

    void ShootProjectileWeapon(Vector2 baseDir)
    {
        int shots = Mathf.Max(1, bulletsPerShot);

        for (int i = 0; i < shots; i++)
        {
            float angleOffset = 0f;

            if (shots > 1)
            {
                angleOffset = Mathf.Lerp(-spreadAngle, spreadAngle, (float)i / (shots - 1));
            }

            Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

            GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet == null) continue;

            float finalDamage = damage;
            if (_stats != null)
                finalDamage = _stats.GetFinalDamage(damage);

            bullet.Init(dir, bulletSpeed, finalDamage);
        }
    }

    void ShootBoomerang(Vector2 dir)
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        BoomerangProjectile boomerang = go.GetComponent<BoomerangProjectile>();

        if (boomerang == null) return;

        float finalDamage = damage;
        if (_stats != null)
            finalDamage = _stats.GetFinalDamage(damage);

        float distance = 5f;
        if (_currentWeapon != null)
            distance = _currentWeapon.boomerangDistance;

        boomerang.Init(transform, dir, bulletSpeed, finalDamage, distance);
    }
}