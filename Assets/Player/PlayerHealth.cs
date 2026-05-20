
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    [SerializeField] private float damageCooldown = 0.5f;
    private PlayerShield _shield;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject gameOverMenu;

    private float _lastDamageTime;
    private bool _isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => _isDead;

    void Awake()
    {
        currentHealth = maxHealth;
        if (gameOverMenu) gameOverMenu.SetActive(false);
        _shield = GetComponent<PlayerShield>();
        UpdateUI();
    }

    public void TakeDamage(int damage)
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

        SlotMachine.Instance?.OnPlayerHit();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        Time.timeScale = 0f;
        if (gameOverMenu) gameOverMenu.SetActive(true);
    }



    private void UpdateUI()
    {
        if (healthText) healthText.text = $"Vida: {currentHealth}/{maxHealth}";
    }
}