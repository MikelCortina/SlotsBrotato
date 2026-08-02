
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float currentHealth;
    [SerializeField] private float damageCooldown = 0.5f;
    private PlayerShield _shield;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject gameOverMenu;


    [Header("Damage Shockwave")]
    [SerializeField] private float shockwaveRadius = 3f;
    [SerializeField] private float shockwaveForce = 12f;
    [SerializeField] private float shockwaveDuration = 0.20f;
    private float _lastDamageTime;
    private bool _isDead;


    [Header("Invulnerability Visual")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private float blinkInterval = 0.08f;
    [SerializeField, Range(0f, 1f)] private float blinkAlpha = 0.3f;
    [SerializeField] private float hitFlashDuration = 0.06f;

    private Coroutine _invulnerabilityVisualCoroutine;
    private Color _originalSpriteColor;
    public float CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    void Awake()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
            maxHealth = stats.maxHealth;

        BoosterData active = BoosterManager.Instance?.GetActiveBooster();
        Debug.Log("BoosterManager existe: " + (BoosterManager.Instance != null));
        Debug.Log("Booster activo: " + (active != null ? active.boosterName : "ninguno"));
        if (active != null)
            maxHealth += active.bonusMaxHealth;

        currentHealth = maxHealth;
        if (gameOverMenu) gameOverMenu.SetActive(false);
        _shield = GetComponent<PlayerShield>();
        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (playerSprite != null)
            _originalSpriteColor = playerSprite.color;
        UpdateUI();
    }

    public void RefreshMaxHealthFromStats()
    {
        PlayerStats stats = GetComponent<PlayerStats>();

        if (stats != null)
            maxHealth = stats.maxHealth;

        BoosterData active = BoosterManager.Instance?.GetActiveBooster();

        if (active != null)
            maxHealth += active.bonusMaxHealth;

        currentHealth = maxHealth;
        UpdateUI();
    }
    public void TakeDamage(float damage)
    {
        if (_isDead)
            return;

        if (Time.time < _lastDamageTime + damageCooldown)
            return;

        _lastDamageTime = Time.time;
        StartInvulnerabilityVisual();

        if (_shield != null && _shield.TryBlockDamage())
        {
            OnPlayerHit();
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateUI();

        OnPlayerHit();

        if (currentHealth <= 0f)
            Die();
    }
    private void Die()
    {
        _isDead = true;

        // Deshabilitar inputs del jugador inmediatamente
        var playerController = GetComponent<PlayerController>();
        if (playerController != null) playerController.enabled = false;

        // Delegar todo al GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        else
        {
            // Fallback si no hay GameManager
            if (gameOverMenu)
            {
                gameOverMenu.SetActive(true);
                CanvasGroup cg = gameOverMenu.GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }
            }
            Time.timeScale = 0f;
        }
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateUI();
    }

    public void ResetHealth()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        maxHealth = stats != null ? stats.maxHealth : 5;
        currentHealth = maxHealth;
        UpdateUI();
    }


    private void UpdateUI()
    {
        if (healthText)
        {
            healthText.text = $"Vida: {currentHealth}/{maxHealth}";
            Debug.Log("UI actualizada: " + healthText.text);
        }
        else
        {
            Debug.Log("healthText es NULL, no hay referencia al texto");
        }
    }

    private void PushNearbyEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            shockwaveRadius
        );

        foreach (Collider2D hit in hits)
        {
            EnemyController enemy = hit.GetComponentInParent<EnemyController>();

            if (enemy == null)
                continue;

            Vector2 direction =
                (enemy.transform.position - transform.position).normalized;

            // Por seguridad, si ambos están exactamente en la misma posición.
            if (direction.sqrMagnitude < 0.001f)
                direction = Random.insideUnitCircle.normalized;

            enemy.StartKnockback(
                direction * shockwaveForce,
                shockwaveDuration
            );

            enemy.OnReceiveDamageBounceReset();
        }
    }

    private void OnPlayerHit()
    {
        PushNearbyEnemies();
    }

    private void StartInvulnerabilityVisual()
    {
        if (playerSprite == null)
            return;

        if (_invulnerabilityVisualCoroutine != null)
            StopCoroutine(_invulnerabilityVisualCoroutine);

        _invulnerabilityVisualCoroutine =
            StartCoroutine(InvulnerabilityVisualRoutine());
    }

    private System.Collections.IEnumerator InvulnerabilityVisualRoutine()
    {
        // Flash blanco inicial
        playerSprite.color = Color.white;
        yield return new WaitForSeconds(hitFlashDuration);

        float elapsed = hitFlashDuration;
        bool transparent = false;

        while (elapsed < damageCooldown)
        {
            transparent = !transparent;

            Color color = _originalSpriteColor;
            color.a = transparent ? blinkAlpha : _originalSpriteColor.a;
            playerSprite.color = color;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        playerSprite.color = _originalSpriteColor;
        _invulnerabilityVisualCoroutine = null;
    }
}