using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverMenu;
    public GameObject shopPanel;

    [Header("Oleadas")]
    public int startingWave = 1;

    [Header("Tiempo tipo Brotato")]
    public float firstWaveDuration = 20f;
    public float waveDurationStep = 5f;
    public float maxNormalWaveDuration = 60f;
    public float finalWaveDuration = 90f;
    public int finalWave = 20;

    [Header("Escalado enemigos")]
    public int baseEnemyCount = 4;
    public int enemyCountIncreasePerWave = 2;

    [Header("Tienda")]
    public float shopDuration = 15f;

    public int CurrentWave { get; private set; }
    public int Score { get; private set; }
    public int EnemiesAlive { get; private set; }
    public float WaveTimeRemaining { get; private set; }
    public bool IsInShop { get; private set; }
    public bool IsWaveRunning { get; private set; }

    public static readonly List<GameObject> LiveEnemies = new List<GameObject>();

    bool _shopClosed;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentWave = startingWave;
        if (gameOverMenu) gameOverMenu.SetActive(false);
        if (shopPanel) shopPanel.SetActive(false);
        UpdateUI();
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    public void BeginRun()
    {
        Time.timeScale = 1f;
        CurrentWave = startingWave;
        Score = 0;
        EnemiesAlive = 0;
        WaveTimeRemaining = GetWaveDuration(CurrentWave);
        IsInShop = false;
        IsWaveRunning = true;

        if (shopPanel) shopPanel.SetActive(false);
        if (gameOverMenu) gameOverMenu.SetActive(false);

        UpdateUI();
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            yield return RunWave(CurrentWave);

            IsWaveRunning = false;
            CleanupWaveEnemies();
            PauseGame();

            yield return RunShop();

            CurrentWave++;
            ResumeGame();
        }
    }

    IEnumerator RunWave(int wave)
    {
        IsWaveRunning = true;
        IsInShop = false;

        WaveTimeRemaining = GetWaveDuration(wave);
        UpdateUI();

        while (WaveTimeRemaining > 0f)
        {
            WaveTimeRemaining -= Time.deltaTime;
            UpdateUI();
            yield return null;
        }

        WaveTimeRemaining = 0f;
        UpdateUI();
    }

    IEnumerator RunShop()
    {
        IsInShop = true;
        _shopClosed = false;

        if (shopPanel) shopPanel.SetActive(true);

        while (!_shopClosed)
            yield return null;

        IsInShop = false;
        if (shopPanel) shopPanel.SetActive(false);
    }

    void CleanupWaveEnemies()
    {
        for (int i = LiveEnemies.Count - 1; i >= 0; i--)
        {
            if (LiveEnemies[i] != null)
                Destroy(LiveEnemies[i]);
        }

        LiveEnemies.Clear();
        EnemiesAlive = 0;
        UpdateUI();
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void ContinueFromShop()
    {
        _shopClosed = true;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        LiveEnemies.Add(enemy);
        EnemiesAlive++;
        UpdateUI();
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        LiveEnemies.Remove(enemy);
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        UpdateUI();
    }

    public int GetEnemyCountForWave(int wave)
    {
        return baseEnemyCount + (wave - 1) * enemyCountIncreasePerWave;
    }

    public float GetWaveDuration(int wave)
    {
        if (wave >= finalWave)
            return finalWaveDuration;

        float duration = firstWaveDuration + (wave - 1) * waveDurationStep;
        return Mathf.Min(duration, maxNormalWaveDuration);
    }

    public void OnEnemyKilled()
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        Score += 10 + CurrentWave * 5;
        UpdateUI();
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if (gameOverMenu) gameOverMenu.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateUI()
    {
        if (waveText) waveText.text = $"Oleada {CurrentWave}";
        if (scoreText) scoreText.text = $"Puntos: {Score}";
        if (timerText)
        {
            if (IsWaveRunning) timerText.text = $"Tiempo: {Mathf.CeilToInt(WaveTimeRemaining)}";
            else if (IsInShop) timerText.text = "Tienda";
            else timerText.text = "";
        }
    }
}