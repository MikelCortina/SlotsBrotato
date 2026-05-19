using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TMPro.TextMeshProUGUI waveText;
    public TMPro.TextMeshProUGUI scoreText;
    public GameObject gameOverMenu;

    public int CurrentWave { get; private set; } = 1;
    public int Score { get; private set; } = 0;
    public int EnemiesAlive { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        if (gameOverMenu) gameOverMenu.SetActive(false);
    }

    public void ReportWaveStart(int wave)
    {
        CurrentWave = wave;
        UpdateUI();
    }

    public void ReportEnemySpawned()
    {
        EnemiesAlive++;
        UpdateUI();
    }

    public void OnEnemyKilled()
    {
        EnemiesAlive = Mathf.Max(0, EnemiesAlive - 1);
        Score += 10 + CurrentWave * 5;
        SlotMachine.Instance?.OnEnemyKilled();
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

    private void UpdateUI()
    {
        if (waveText) waveText.text = $"Oleada {CurrentWave}";
        if (scoreText) scoreText.text = $"Puntos: {Score}";
    }
}