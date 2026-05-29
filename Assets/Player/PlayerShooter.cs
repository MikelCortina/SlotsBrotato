using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] public Transform weaponPivot;
    [SerializeField] public WeaponPivotAim weaponAim;
    [SerializeField] public bool autoFire = true;

    [Header("Debug / arma inicial")]
    [SerializeField] public WeaponData startWeapon;

    public float _fireTimer;
    public PlayerStats _stats;
    public WeaponData _currentWeapon;

    public WeaponInstance _currentWeaponInstance;
    public Transform _firePoint;
    public Vector3 _weaponBaseScale = Vector3.one;

    public float fireRate;
    public float damage;
    public float bulletSpeed;
    public int bulletsPerShot;
    public GameObject bulletPrefab;
    public float spreadAngle;

    private SpriteRenderer _weaponSpriteRenderer;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (startWeapon != null)
            ApplyWeaponData(startWeapon);
    }

    void Update()
    {
        UpdateWeaponFlip();

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

            _fireTimer = 1f / Mathf.Max(0.01f, finalFireRate);
        }
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

        EquipWeaponPrefab();
    }

    void EquipWeaponPrefab()
    {
        if (_currentWeaponInstance != null)
            Destroy(_currentWeaponInstance.gameObject);

        _firePoint = null;
        _weaponSpriteRenderer = null;

        if (_currentWeapon == null || _currentWeapon.weaponPrefab == null || weaponPivot == null)
            return;

        GameObject weaponGO = Instantiate(_currentWeapon.weaponPrefab, weaponPivot);
        weaponGO.transform.localPosition = new Vector3(_currentWeapon.holdRadius, 0f, 0f);
        weaponGO.transform.localRotation = Quaternion.identity;
        weaponGO.transform.localScale = Vector3.one;

        _currentWeaponInstance = weaponGO.GetComponent<WeaponInstance>();

        if (_currentWeaponInstance != null)
            _firePoint = _currentWeaponInstance.firePoint;

        _weaponSpriteRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        _weaponBaseScale = weaponGO.transform.localScale;
    }

    void UpdateWeaponFlip()
    {
        if (_currentWeaponInstance == null || weaponAim == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        bool mouseOnLeft = mouseWorld.x < weaponPivot.position.x;

        if (_weaponSpriteRenderer != null)
        {
            _weaponSpriteRenderer.flipY = mouseOnLeft;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || _firePoint == null) return;

        Vector2 baseDir = _firePoint.right.normalized;

        if (_currentWeapon != null && _currentWeapon.weaponType == WeaponType.Boomerang)
        {
            ShootBoomerang(baseDir);
            return;
        }

        ShootProjectileWeapon(baseDir);
    }

    void ShootProjectileWeapon(Vector2 baseDir)
    {
        int shots = Mathf.Max(1, bulletsPerShot);

        for (int i = 0; i < shots; i++)
        {
            float angleOffset = 0f;

            if (shots > 1)
                angleOffset = Mathf.Lerp(-spreadAngle, spreadAngle, (float)i / (shots - 1));

            Vector2 dir = Quaternion.Euler(0f, 0f, angleOffset) * baseDir;

            GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);

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
        if (bulletPrefab == null || _firePoint == null) return;

        GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);
        BoomerangProjectile boomerang = go.GetComponent<BoomerangProjectile>();

        if (boomerang == null) return;

        float finalDamage = damage;
        if (_stats != null)
            finalDamage = _stats.GetFinalDamage(damage);

        float distance = _currentWeapon != null ? _currentWeapon.boomerangDistance : 5f;
        boomerang.Init(transform, dir, bulletSpeed, finalDamage, distance);
    }
}