
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

    private float _lastDamageTime;
    private bool _isDead;

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
        if (_isDead) return;
        if (_shield != null && _shield.TryBlockDamage())
        {
            return;
        }
        if (Time.time < _lastDamageTime + damageCooldown) return;

        _lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateUI();


        if (currentHealth <= 0)
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
}