using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Data")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI specialWaveText;
    public GameUIFlowController uiFlow;

    [Header("Fallback panel directo")]
    [SerializeField] private GameObject directGameOverPanel;

    [Header("Cursor en partida")]
    [SerializeField] private Texture2D gameplayCursor;
    [SerializeField] private Vector2 gameplayCursorHotspot = Vector2.zero;
    [SerializeField] private CursorMode gameplayCursorMode = CursorMode.Auto;

    [Header("Oleadas")]
    public int startingWave = 1;



    [Header("Special Waves")]
    public int specialWaveInterval = 5;
    public SpecialWaveType CurrentSpecialWaveType { get; private set; }

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

    private bool _shopClosed;
    private bool _isGameOver;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        CurrentWave = startingWave;
        Score = 0;
        EnemiesAlive = 0;
        WaveTimeRemaining = 0f;

        IsWaveRunning = false;
        IsInShop = false;
        _isGameOver = false;
        _shopClosed = false;

        if (uiFlow != null)
            uiFlow.Initialize(this);

        if (directGameOverPanel != null)
            directGameOverPanel.SetActive(false);

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Time.timeScale = 1f;
    }

    void ApplyTimeScale()
    {
        bool isActuallyPlaying = IsWaveRunning && !IsInShop && !_isGameOver;
        Time.timeScale = isActuallyPlaying ? 1f : 0f;
    }

    void ApplyCursorState()
    {
        bool isActuallyPlaying = IsWaveRunning && !IsInShop && !_isGameOver;

        if (isActuallyPlaying && gameplayCursor != null)
        {
            Cursor.SetCursor(gameplayCursor, gameplayCursorHotspot, gameplayCursorMode);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void BeginRun()
    {
        Time.timeScale = 1f;

        _isGameOver = false;
        _shopClosed = false;

        CurrentWave = startingWave;
        Score = 0;
        EnemiesAlive = 0;
        WaveTimeRemaining = GetWaveDuration(CurrentWave);

        IsInShop = false;
        IsWaveRunning = true;

        if (directGameOverPanel != null)
            directGameOverPanel.SetActive(false);

        if (uiFlow != null)
        {
            uiFlow.Initialize(this);
            uiFlow.ShowGameplayImmediate();
        }

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => IsWaveRunning || _isGameOver);

            if (_isGameOver)
                yield break;

            yield return RunWave(CurrentWave);

            if (_isGameOver)
                yield break;

            CleanupWaveEnemies();

            if (uiFlow != null)
                yield return uiFlow.PlayWaveEndTransition(CurrentWave);

            if (_isGameOver)
                yield break;

            yield return RunShop();

            if (_isGameOver)
                yield break;

            CurrentWave++;
            IsWaveRunning = true;

            if (uiFlow != null)
                uiFlow.ShowGameplayImmediate();

            ApplyTimeScale();
            ApplyCursorState();
            UpdateUI();
        }
    }

    IEnumerator RunWave(int wave)
    {
        IsWaveRunning = true;
        IsInShop = false;
        WaveTimeRemaining = GetWaveDuration(wave);

        CurrentSpecialWaveType = SpecialWaveType.None;

        if (specialWaveInterval > 0 && wave % specialWaveInterval == 0)
        {
            CurrentSpecialWaveType = GetRandomSpecialWaveType();
            Debug.Log($"Oleada especial: {CurrentSpecialWaveType}");
        }

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();

        if (IsSpecialWave(wave))
            StartCoroutine(ShowSpecialWaveMessage());
        while (WaveTimeRemaining > 0f && !_isGameOver)
        {
            WaveTimeRemaining -= Time.deltaTime;
            UpdateUI();
            yield return null;
        }

        WaveTimeRemaining = 0f;
        IsWaveRunning = false;

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();
    }

    IEnumerator RunShop()
    {
        IsInShop = true;
        _shopClosed = false;

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();

        if (uiFlow != null)
            uiFlow.ShowShopImmediate();

        while (!_shopClosed && !_isGameOver)
            yield return null;

        IsInShop = false;

        if (!_isGameOver && uiFlow != null)
            uiFlow.ShowGameplayImmediate();

        ApplyTimeScale();
        ApplyCursorState();
        UpdateUI();
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

    public void ContinueFromShop()
    {
        _shopClosed = true;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        if (!LiveEnemies.Contains(enemy))
            LiveEnemies.Add(enemy);

        EnemiesAlive = Mathf.Max(0, LiveEnemies.Count);
        UpdateUI();
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        LiveEnemies.Remove(enemy);
        EnemiesAlive = Mathf.Max(0, LiveEnemies.Count);
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
        Score += 10 + CurrentWave * 5;
        EnemiesAlive = Mathf.Max(0, LiveEnemies.Count);
        UpdateUI();
    }

    public void GameOver()
    {
        if (_isGameOver)
            return;

        Debug.Log("GameManager -> GameOver()");

        _isGameOver = true;
        IsWaveRunning = false;
        IsInShop = false;
        _shopClosed = true;

        StopAllCoroutines();
        CleanupWaveEnemies();
        ApplyCursorState();
        UpdateUI();

        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        Debug.Log("ShowGameOverRoutine iniciado");

        if (uiFlow != null)
        {
            uiFlow.ShowGameOverImmediate();
            Debug.Log("GameOver panel configured: " + uiFlow.IsGameOverPanelConfigured());
        }
        else
        {
            Debug.LogWarning("GameManager -> uiFlow es null en ShowGameOverRoutine");
        }

        if (directGameOverPanel != null)
        {
            directGameOverPanel.SetActive(true);

            CanvasGroup cg = directGameOverPanel.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = directGameOverPanel.AddComponent<CanvasGroup>();

            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            Debug.Log("directGameOverPanel activado");
        }

        yield return null;
        Time.timeScale = 0f;

        Debug.Log("Time.timeScale puesto a 0 - juego pausado");
    }

    public void RestartGame()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    IEnumerator ShowSpecialWaveMessage()
    {
        if (specialWaveText == null)
            yield break;

        specialWaveText.gameObject.SetActive(true);
        string message = "OLEADA ESPECIAL";

        switch (CurrentSpecialWaveType)
        {
            case SpecialWaveType.Swarm:
                message = "OLEADA ESPECIAL - ENJAMBRE";
                break;

            case SpecialWaveType.Tank:
                message = "OLEADA ESPECIAL - TANQUES";
                break;

            case SpecialWaveType.Frenzy:
                message = "OLEADA ESPECIAL - FRENESI";
                break;
        }

        specialWaveText.text = message;

        yield return new WaitForSeconds(2f);

        specialWaveText.text = "";
        specialWaveText.gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        if (waveText) waveText.text = $"Oleada {CurrentWave}";
        if (scoreText) scoreText.text = $"Puntos: {Score}";

        if (timerText)
        {
            if (_isGameOver)
                timerText.text = "";
            else if (IsWaveRunning)
                timerText.text = $"Tiempo: {Mathf.CeilToInt(WaveTimeRemaining)}";
            else if (IsInShop)
                timerText.text = "Tienda";
            else
                timerText.text = "";
        }
    }

    bool IsSpecialWave(int wave)
    {
        return specialWaveInterval > 0 && wave % specialWaveInterval == 0;
    }

    SpecialWaveType GetRandomSpecialWaveType()
    {
        int random = Random.Range(1, 4);

        return (SpecialWaveType)random;
    }
}