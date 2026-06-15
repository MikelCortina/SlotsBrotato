using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] public Transform weaponPivot;
    [SerializeField] public WeaponPivotAim weaponAim;
    [SerializeField] public bool autoFire = true;

    [Header("Debug / arma inicial")]
    [SerializeField] public WeaponData startWeapon;

    [Header("Runtime")]
    public float _fireTimer;
    public PlayerStats _stats;
    public WeaponData _currentWeapon;

    public WeaponInstance _currentWeaponInstance;
    public Transform _firePoint;
    public Vector3 _weaponBaseScale = Vector3.one;

    [Header("Weapon Runtime Stats")]
    public float fireRate;
    public float fireRateScalingFactor;
    public float damage;
    public float damageScalingFactor;
    public float bulletSpeed;
    public int bulletsPerShot;
    public GameObject bulletPrefab;
    public float spreadAngle;

    private SpriteRenderer _weaponSpriteRenderer;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _defaultShootSound;
    [SerializeField, Range(0f, 1f)] private float _shootSoundVolume = 0.5f;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
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
            PlayShootSound();

            float finalFireRate = GetFinalWeaponFireRate();
            _fireTimer = 1f / Mathf.Max(0.01f, finalFireRate);
        }
    }

    public void ApplyWeaponData(WeaponData weapon)
    {
        if (weapon == null) return;

        _currentWeapon = weapon;

        fireRate = weapon.fireRate;
        fireRateScalingFactor = weapon.fireRateScalingFactor;

        damage = weapon.damage;
        damageScalingFactor = weapon.damageScalingFactor;

        bulletSpeed = weapon.bulletSpeed;
        bulletsPerShot = weapon.bulletsPerShot;
        bulletPrefab = weapon.bulletPrefab;
        spreadAngle = weapon.spreadAngle;

        EquipWeaponPrefab();
    }

    float GetFinalWeaponDamage()
    {
        float finalDamage = damage;

        if (_stats != null)
            // ? FIX: GetScaledDamage ? GetFinalDamage (nombre correcto)
            finalDamage = _stats.GetFinalDamage(damage, damageScalingFactor, true);

        if (WeaponLevelSystem.Instance != null && _currentWeapon != null)
        {
            float weaponMultiplier =
                WeaponLevelSystem.Instance.GetWeaponScalingMultiplier(_currentWeapon);

            finalDamage *= weaponMultiplier;
        }

        return finalDamage;
    }

    float GetFinalWeaponFireRate()
    {
        float finalFireRate = fireRate;

        if (_stats != null)
            finalFireRate = _stats.GetScaledFireRate(fireRate, fireRateScalingFactor);

        return Mathf.Max(0.01f, finalFireRate);
    }

    void Shoot()
    {
        if (bulletPrefab == null || _firePoint == null || weaponPivot == null)
            return;

        Vector2 baseDir = weaponPivot.right.normalized;

        if (weaponPivot.localScale.x < 0f)
            baseDir = -baseDir;

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

        bool hasDoubleShot =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.DoubleShot);

        if (hasDoubleShot)
            shots += 1;

        float finalSpreadAngle = spreadAngle;

        if (hasDoubleShot && finalSpreadAngle < 8f)
            finalSpreadAngle = 8f;

        for (int i = 0; i < shots; i++)
        {
            float angleOffset = 0f;

            if (shots > 1)
                angleOffset = Mathf.Lerp(-finalSpreadAngle, finalSpreadAngle, (float)i / (shots - 1));

            Vector2 dir = Quaternion.Euler(0f, 0f, angleOffset) * baseDir;

            GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);

            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet == null) continue;

            float finalDamage = GetFinalWeaponDamage();
            bullet.Init(dir, bulletSpeed, finalDamage);
        }
    }

    void ShootBoomerang(Vector2 dir)
    {
        if (bulletPrefab == null || _firePoint == null)
            return;

        GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);

        BoomerangProjectile boomerang = go.GetComponent<BoomerangProjectile>();
        if (boomerang == null) return;

        float finalDamage = GetFinalWeaponDamage();

        float distance = _currentWeapon != null
            ? _currentWeapon.boomerangDistance
            : 5f;

        boomerang.Init(transform, dir, bulletSpeed, finalDamage, distance);
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
        weaponGO.transform.localPosition = Vector3.zero;
        weaponGO.transform.localRotation = Quaternion.identity;
        weaponGO.transform.localScale = Vector3.one * 0.5f;

        _currentWeaponInstance = weaponGO.GetComponent<WeaponInstance>();

        if (_currentWeaponInstance != null)
            _firePoint = _currentWeaponInstance.firePoint;

        _weaponSpriteRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        _weaponBaseScale = weaponGO.transform.localScale;
    }

    void UpdateWeaponFlip()
    {
        if (_currentWeaponInstance == null || weaponAim == null || weaponPivot == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        bool mouseOnLeft = mouseWorld.x < weaponPivot.position.x;

        if (_weaponSpriteRenderer != null)
            _weaponSpriteRenderer.flipY = mouseOnLeft;
    }

    void PlayShootSound()
    {
        if (_audioSource == null) return;

        AudioClip soundToPlay = _currentWeapon != null && _currentWeapon.shootSound != null
            ? _currentWeapon.shootSound
            : _defaultShootSound;

        if (soundToPlay != null)
            _audioSource.PlayOneShot(soundToPlay, _shootSoundVolume);
    }

    public WeaponData GetCurrentWeapon()
    {
        return _currentWeapon;
    }

    public float GetCurrentWeaponDamage()
    {
        return GetFinalWeaponDamage();
    }

    public float GetCurrentWeaponFireRate()
    {
        return GetFinalWeaponFireRate();
    }
}