using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Enemy Types")]
    public GameObject spitterEnemyPrefab;
    public GameObject ringSpitterEnemyPrefab;
    public GameObject dasherEnemyPrefab;
    [Header("Spawn")]
    public float minSpawnInterval = 0.2f;
    public float maxSpawnInterval = 0.8f;
    public float spawnGraceTime = 1f;

    [Header("Escalado tipo Brotato")]
    public float baseEnemyHp = 30f;
    public float hpIncreasePerWave = 15f;
    public float baseEnemySpeed = 2f;
    public float baseEnemyDamage = 4f;
    public float damageIncreasePerWave = 1f;

    [Header("Warning Spawn")]
    public GameObject spawnWarningPrefab;
    public float spawnWarningTime = 0.8f;
    public float blinkInterval = 0.1f;

    [Header("Special Waves")]
    public int specialWaveInterval = 5;
    public float specialWaveSpawnMultiplier = 2f;

    [Header("Zona de Spawn")]
    public SpawnAreaMode areaMode = SpawnAreaMode.Donut;
    public float innerRadius = 7f;
    public float outerRadius = 12f;
    public Vector2 rectSize = new Vector2(20f, 12f);
    public float rectInnerExclusion = 5f;
    public Transform spawnCenter;

    Transform _playerTransform;
    Coroutine _spawnRoutine;
    bool _running;

    public float spawnBudgetPerSecond = 1.5f;
    public float spawnBudgetIncreasePerWave = 0.25f;
    float _spawnBudget;

    public enum SpawnAreaMode { Donut, Rectangle }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    public void BeginSpawning()
    {
        if (_running) return;
        _running = true;
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        _running = false;
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }
    IEnumerator SpawnLoop()
    {
        while (_running)
        {
            if (GameManager.Instance == null ||
                !GameManager.Instance.IsWaveRunning)
            {
                yield return null;
                continue;
            }

            int wave = GameManager.Instance.CurrentWave;

            _spawnBudget =
                spawnBudgetPerSecond +
                (wave - 1) * spawnBudgetIncreasePerWave;

            if (GameManager.Instance.CurrentSpecialWaveType ==
                SpecialWaveType.Swarm)
            {
                _spawnBudget *= specialWaveSpawnMultiplier;
            }

            while (_running &&
                   GameManager.Instance != null &&
                   GameManager.Instance.IsWaveRunning)
            {
                // Si el debug cambia de oleada, salir y recalcular todo.
                if (GameManager.Instance.CurrentWave != wave)
                    break;

                _spawnBudget += Time.deltaTime;

                while (_spawnBudget >= 1f)
                {
                    _spawnBudget -= 1f;
                    StartCoroutine(SpawnEnemyWithWarning());
                }

                yield return null;
            }
        }
    }

    IEnumerator SpawnEnemyWithWarning()
    {
        if (enemyPrefab == null) yield break;

        Vector2 center = GetCenter();
        Vector2 pos = areaMode == SpawnAreaMode.Donut ? GetDonutPosition(center) : GetRectPosition(center);

        if (spawnWarningPrefab != null)
        {
            GameObject warning = Instantiate(spawnWarningPrefab, pos, Quaternion.identity);
            yield return StartCoroutine(BlinkWarning(warning, spawnWarningTime, blinkInterval));
            Destroy(warning);
        }
        else
        {
            yield return new WaitForSeconds(spawnWarningTime);
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsWaveRunning)
            yield break;

        // Leer la oleada actual justo antes de crear el enemigo.
        // Así el botón Start Wave aplica inmediatamente la nueva oleada.
        int wave = GameManager.Instance.CurrentWave;

        GameObject prefabToSpawn = enemyPrefab;

        float roll = Random.value;

        if (wave >= 12 &&
            dasherEnemyPrefab != null &&
            roll < 0.08f)
        {
            prefabToSpawn = dasherEnemyPrefab;
        }
        else if (wave >= 10 &&
                 ringSpitterEnemyPrefab != null &&
                 roll < 0.18f)
        {
            prefabToSpawn = ringSpitterEnemyPrefab;
        }
        else if (wave >= 5 &&
                 spitterEnemyPrefab != null &&
                 roll < 0.38f)
        {
            prefabToSpawn = spitterEnemyPrefab;
        }

        GameObject go = Instantiate(prefabToSpawn, pos, Quaternion.identity);
        GameManager.Instance.RegisterEnemy(go);

        var health = go.GetComponent<EnemyHealth>();
        if (health != null)
        {
            float hp = baseEnemyHp + (wave - 1) * hpIncreasePerWave;

            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentSpecialWaveType == SpecialWaveType.Tank)
            {
                hp *= 2f;
            }

            health.maxHp = hp;
            health.ResetHealth();
            health.SubscribeOnDeath(() =>
            {
                GameManager.Instance?.OnEnemyKilled();
                GameManager.Instance?.UnregisterEnemy(go);
            });
        }

        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            float speed = baseEnemySpeed;

            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentSpecialWaveType == SpecialWaveType.Frenzy)
            {
                speed *= 2f;
            }

            ctrl.speed = speed;
        }

        var dmg = go.GetComponent<EnemyDamage>();
        if (dmg != null)
        {
            dmg.damage = baseEnemyDamage + (wave - 1) * damageIncreasePerWave;
        }
    }

    IEnumerator BlinkWarning(GameObject warning, float duration, float interval)
    {
        SpriteRenderer sr = warning.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < duration)
        {
            sr.enabled = visible;
            visible = !visible;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        sr.enabled = true;
    }

    Vector2 GetDonutPosition(Vector2 center)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(innerRadius, outerRadius);
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
    }

    Vector2 GetRectPosition(Vector2 center)
    {
        Vector2 pos;
        int maxAttempts = 30;
        do
        {
            pos = center + new Vector2(
                Random.Range(-rectSize.x / 2f, rectSize.x / 2f),
                Random.Range(-rectSize.y / 2f, rectSize.y / 2f)
            );
            maxAttempts--;
        }
        while (Vector2.Distance(pos, center) < rectInnerExclusion && maxAttempts > 0);

        return pos;
    }

    Vector2 GetCenter()
    {
        if (spawnCenter != null) return spawnCenter.position;
        if (_playerTransform != null) return _playerTransform.position;
        return Vector2.zero;
    }
    bool IsSpecialWave(int wave)
    {
        return specialWaveInterval > 0 && wave % specialWaveInterval == 0;
    }

    public void PrintWavePreview(int wave)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No existe GameManager para calcular la oleada.");
            return;
        }

        float duration = GameManager.Instance.GetWaveDuration(wave);

        float budgetPerSecond =
            spawnBudgetPerSecond +
            (wave - 1) * spawnBudgetIncreasePerWave;

        bool isSwarm =
            GameManager.Instance.CurrentSpecialWaveType ==
            SpecialWaveType.Swarm;

        if (isSwarm)
            budgetPerSecond *= specialWaveSpawnMultiplier;

        int estimatedTotal =
            Mathf.Max(1, Mathf.RoundToInt(duration * budgetPerSecond));

        float normalChance = 1f;
        float spitterChance = 0f;
        float ringSpitterChance = 0f;
        float dasherChance = 0f;

        if (wave >= 12)
        {
            dasherChance = 0.08f;
            ringSpitterChance = 0.10f;
            spitterChance = 0.20f;
            normalChance = 0.62f;
        }
        else if (wave >= 10)
        {
            ringSpitterChance = 0.18f;
            spitterChance = 0.20f;
            normalChance = 0.62f;
        }
        else if (wave >= 5)
        {
            spitterChance = 0.38f;
            normalChance = 0.62f;
        }

        int estimatedDashers =
            Mathf.RoundToInt(estimatedTotal * dasherChance);

        int estimatedRingSpitters =
            Mathf.RoundToInt(estimatedTotal * ringSpitterChance);

        int estimatedSpitters =
            Mathf.RoundToInt(estimatedTotal * spitterChance);

        int estimatedNormals =
            estimatedTotal -
            estimatedDashers -
            estimatedRingSpitters -
            estimatedSpitters;

        string specialType =
            GameManager.Instance.CurrentSpecialWaveType.ToString();

        Debug.Log(
            $"\n========== PREVISIÓN OLEADA {wave} ==========\n" +
            $"Duración: {duration:0} segundos\n" +
            $"Presupuesto por segundo: {budgetPerSecond:0.00}\n" +
            $"Tipo especial: {specialType}\n" +
            $"Total aproximado: {estimatedTotal}\n\n" +
            $"Normales: {estimatedNormals} ({normalChance * 100f:0}%)\n" +
            $"Spitters: {estimatedSpitters} ({spitterChance * 100f:0}%)\n" +
            $"Ring Spitters: {estimatedRingSpitters} ({ringSpitterChance * 100f:0}%)\n" +
            $"Dashers: {estimatedDashers} ({dasherChance * 100f:0}%)\n" +
            $"============================================"
        );
    }
}