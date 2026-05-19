using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;

    public int CurrentWave { get; private set; } = 1;
    public int Score { get; private set; } = 0;
    public int EnemiesAlive { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
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
        SlotMachine.Instance?.OnEnemyKilled(); // ? añadir esto
        UpdateUI();
    }

    void UpdateUI()
    {
        if (waveText) waveText.text = $"Oleada {CurrentWave}";
        if (scoreText) scoreText.text = $"Puntos: {Score}";
    }
    
}