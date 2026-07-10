using System;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] public Transform weaponPivot;
    [SerializeField] private float weaponOrbitRadius = 1.2f;
    [SerializeField] public WeaponPivotAim weaponAim;
    [SerializeField] public bool autoFire = true;

    [Header("Melee Visual")]
    [SerializeField] private GameObject meleeSlashPrefab;
    [SerializeField] private float meleeSlashDistance = 1.2f;

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
    public float bulletRange;
    public int bulletsPerShot;
    public GameObject bulletPrefab;
    public float spreadAngle;
    public float singleShotBloomAngle;
    public float bulletSize = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _defaultShootSound;
    [SerializeField, Range(0f, 1f)] private float _shootSoundVolume = 0.5f;

    public float LastShootTime { get; private set; } = -999f;

    public event Action OnShoot;

    private float _lastSingleShotBloomAngle = float.NaN;

    private bool _boomerangInFlight = false;
    private SpriteRenderer _weaponSpriteRenderer;
    private Sprite _defaultWeaponSprite;

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
        _fireTimer -= Time.deltaTime;

        bool shootPressed = autoFire
            ? Input.GetMouseButton(0)
            : Input.GetMouseButtonDown(0);

        if (_boomerangInFlight)
            return;

        if (shootPressed && _fireTimer <= 0f)
        {
            Shoot();
            PlayShootSound();
            LastShootTime = Time.time;
            OnShoot?.Invoke();

            float finalFireRate = GetFinalWeaponFireRate();
            _fireTimer = 1f / Mathf.Max(0.01f, finalFireRate);
        }
    }

    void PlayWeaponParticles(Vector2 shootDir)
    {
        if (_currentWeaponInstance == null)
            return;

        _currentWeaponInstance.PlayShootParticles(shootDir);
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
        bulletRange = weapon.bulletRange;
        bulletsPerShot = weapon.bulletsPerShot;
        bulletPrefab = weapon.bulletPrefab;
        spreadAngle = weapon.spreadAngle;
        singleShotBloomAngle = weapon.singleShotBloomAngle;
        bulletSize = weapon.bulletSize;

        _lastSingleShotBloomAngle = float.NaN;
        _boomerangInFlight = false;

        EquipWeaponPrefab();
    }

    float GetFinalWeaponDamage()
    {
        float finalDamage = damage;

        if (_stats != null)
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
        if (_currentWeapon == null)
            return;

        if (weaponAim == null)
            return;

        if (!weaponAim.TryGetMouseWorldPosition(out Vector3 mouseWorld))
            return;

        if (_currentWeapon.weaponType != WeaponType.Melee)
        {
            if (bulletPrefab == null || _firePoint == null)
                return;
        }

        Vector2 baseDir;

        if (_firePoint != null)
            baseDir = ((Vector2)(mouseWorld - _firePoint.position)).normalized;
        else if (weaponPivot != null)
            baseDir = ((Vector2)(mouseWorld - weaponPivot.position)).normalized;
        else
            return;

        PlayWeaponParticles(baseDir);

        if (_currentWeapon.weaponType == WeaponType.Boomerang)
        {
            ShootBoomerang(baseDir);
            return;
        }

        if (_currentWeapon.weaponType == WeaponType.Melee)
        {
            MeleeAttack(baseDir);
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
            float shotAngleOffset = 0f;

            if (shots > 1)
            {
                shotAngleOffset = Mathf.Lerp(
                    -finalSpreadAngle,
                    finalSpreadAngle,
                    (float)i / (shots - 1)
                );
            }
            else if (singleShotBloomAngle > 0f)
            {
                float newAngle;
                float minAngleDifference = 0.25f;
                int safety = 0;

                do
                {
                    newAngle = UnityEngine.Random.Range(
                        -singleShotBloomAngle,
                        singleShotBloomAngle
                    );

                    safety++;
                }
                while (
                    !float.IsNaN(_lastSingleShotBloomAngle) &&
                    Mathf.Abs(newAngle - _lastSingleShotBloomAngle) < minAngleDifference &&
                    safety < 10
                );

                shotAngleOffset = newAngle;
                _lastSingleShotBloomAngle = shotAngleOffset;
            }

            Vector2 dir = Quaternion.Euler(0f, 0f, shotAngleOffset) * baseDir;

            GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);

            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet == null) continue;

            float finalDamage = GetFinalWeaponDamage();
            bullet.Init(dir, bulletSpeed, finalDamage, bulletRange, bulletSize);
        }
    }

    void ShootBoomerang(Vector2 dir)
    {
        if (_boomerangInFlight)
            return;

        if (bulletPrefab == null || _firePoint == null)
            return;

        GameObject go = Instantiate(bulletPrefab, _firePoint.position, Quaternion.identity);
        go.transform.localScale *= bulletSize;

        BoomerangProjectile boomerang = go.GetComponent<BoomerangProjectile>();
        if (boomerang == null) return;

        float finalDamage = GetFinalWeaponDamage();

        float distance = _currentWeapon != null
            ? _currentWeapon.boomerangDistance
            : 5f;

        _boomerangInFlight = true;
        SetBoomerangWeaponThrownVisual(true);

        boomerang.OnBoomerangFinished += HandleBoomerangFinished;
        boomerang.Init(transform, dir, bulletSpeed, finalDamage, distance);
    }

    void HandleBoomerangFinished(BoomerangProjectile boomerang, bool returnedToOwner)
    {
        if (boomerang != null)
            boomerang.OnBoomerangFinished -= HandleBoomerangFinished;

        _boomerangInFlight = false;
        SetBoomerangWeaponThrownVisual(false);
    }

    void SetBoomerangWeaponThrownVisual(bool thrown)
    {
        if (_weaponSpriteRenderer == null)
            return;

        if (!thrown)
        {
            _weaponSpriteRenderer.sprite = _defaultWeaponSprite;
            return;
        }

        if (_currentWeapon != null && _currentWeapon.boomerangThrownWeaponSprite != null)
            _weaponSpriteRenderer.sprite = _currentWeapon.boomerangThrownWeaponSprite;
    }

    void MeleeAttack(Vector2 baseDir)
    {
        SpawnMeleeSlash(baseDir);

        float attackRange = 2f;
        float attackAngle = 90f;
        float finalDamage = GetFinalWeaponDamage();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null)
                continue;

            Vector2 dirToEnemy = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(baseDir, dirToEnemy);

            if (angle <= attackAngle * 0.5f)
                enemy.TakeDamage(finalDamage);
        }
    }

    void EquipWeaponPrefab()
    {
        if (_currentWeaponInstance != null)
            Destroy(_currentWeaponInstance.gameObject);

        _firePoint = null;
        _weaponSpriteRenderer = null;
        _defaultWeaponSprite = null;
        _boomerangInFlight = false;

        if (_currentWeapon == null || _currentWeapon.weaponPrefab == null || weaponPivot == null)
            return;

        GameObject weaponGO = Instantiate(_currentWeapon.weaponPrefab, weaponPivot);
        weaponGO.transform.localPosition = new Vector3(weaponOrbitRadius, 0f, 0f);
        weaponGO.transform.localRotation = Quaternion.identity;

        _currentWeaponInstance = weaponGO.GetComponent<WeaponInstance>();

        if (_currentWeaponInstance != null)
            _firePoint = _currentWeaponInstance.firePoint;

        _weaponBaseScale = weaponGO.transform.localScale;

        _weaponSpriteRenderer = weaponGO.GetComponentInChildren<SpriteRenderer>();
        if (_weaponSpriteRenderer != null)
            _defaultWeaponSprite = _weaponSpriteRenderer.sprite;
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

    void SpawnMeleeSlash(Vector2 baseDir)
    {
        if (meleeSlashPrefab == null)
            return;

        Vector3 spawnPos = transform.position + (Vector3)(baseDir.normalized * meleeSlashDistance);

        Instantiate(
            meleeSlashPrefab,
            spawnPos,
            Quaternion.identity
        );
    }
}