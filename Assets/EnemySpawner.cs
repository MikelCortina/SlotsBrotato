using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ?? Oleadas ??????????????????????????????????????????????????????
    [Header("Configuración de Oleadas")]
    public GameObject enemyPrefab;
    public int baseEnemies = 5;
    public int enemiesIncreasePerWave = 3;
    public float spawnInterval = 0.5f;
    public float pauseBetweenWaves = 2f;

    // ?? Escalado de dificultad ???????????????????????????????????????
    [Header("Escalado por Oleada")]
    public float baseEnemyHp = 30f;
    public float hpIncreasePerWave = 15f;
    public float baseEnemySpeed = 2f;
    public float speedIncreasePerWave = 0.3f;

    // ?? Zona de Spawn ????????????????????????????????????????????????
    [Header("Zona de Spawn")]
    public SpawnAreaMode areaMode = SpawnAreaMode.Donut;

    [Tooltip("Sólo en modo Donut: distancia mínima del centro")]
    public float innerRadius = 7f;

    [Tooltip("Sólo en modo Donut: distancia máxima del centro")]
    public float outerRadius = 12f;

    [Tooltip("Sólo en modo Rectangle: tamaño del rectángulo de spawn")]
    public Vector2 rectSize = new Vector2(20f, 12f);

    [Tooltip("Sólo en modo Rectangle: radio interior a excluir (evita spawn encima del jugador)")]
    public float rectInnerExclusion = 5f;

    [Tooltip("Punto central de la zona (vacío = sigue al jugador automáticamente)")]
    public Transform spawnCenter;

    // ?? Gizmos ???????????????????????????????????????????????????????
    [Header("Gizmos")]
    public Color gizmoColorOuter = new Color(1f, 0.4f, 0.1f, 0.25f);
    public Color gizmoColorInner = new Color(1f, 1f, 0f, 0.15f);
    public bool showGizmos = true;

    // ?? Privado ??????????????????????????????????????????????????????
    private int _currentWave = 1;
    private int _enemiesAlive = 0;
    private Transform _playerTransform;

    // ?????????????????????????????????????????????????????????????????
    public enum SpawnAreaMode { Donut, Rectangle }

    // ?????????????????????????????????????????????????????????????????

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        StartCoroutine(WaveLoop());
    }

    // ?? Wave Loop ????????????????????????????????????????????????????

    IEnumerator WaveLoop()
    {
        while (true)
        {
            yield return StartCoroutine(RunWave(_currentWave));
            yield return new WaitForSeconds(pauseBetweenWaves);
            _currentWave++;
        }
    }

    IEnumerator RunWave(int wave)
    {
        GameManager.Instance?.ReportWaveStart(wave);

        int count = baseEnemies + (wave - 1) * enemiesIncreasePerWave;
        _enemiesAlive = count;

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(wave);
            GameManager.Instance?.ReportEnemySpawned();
            yield return new WaitForSeconds(spawnInterval);
        }

        // Esperar hasta que mueran todos
        yield return new WaitUntil(() => _enemiesAlive <= 0);
    }

    // ?? Spawn ????????????????????????????????????????????????????????

    void SpawnEnemy(int wave)
    {
        if (enemyPrefab == null) return;

        Vector2 center = GetCenter();
        Vector2 pos = areaMode == SpawnAreaMode.Donut
            ? GetDonutPosition(center)
            : GetRectPosition(center);

        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);

        var health = go.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.maxHp = baseEnemyHp + (wave - 1) * hpIncreasePerWave;
        }

        var ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            ctrl.speed = baseEnemySpeed + (wave - 1) * speedIncreasePerWave;
        }

        // Suscribir muerte para el contador
        go.GetComponent<EnemyHealth>()?.SubscribeOnDeath(OnEnemyDied);
    }

    void OnEnemyDied()
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
        GameManager.Instance?.OnEnemyKilled();
    }

    // ?? Posición Donut ???????????????????????????????????????????????

    Vector2 GetDonutPosition(Vector2 center)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(innerRadius, outerRadius);
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
    }

    // ?? Posición Rectángulo ??????????????????????????????????????????

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

    // ?? Centro dinámico ??????????????????????????????????????????????

    Vector2 GetCenter()
    {
        if (spawnCenter != null) return spawnCenter.position;
        if (_playerTransform != null) return _playerTransform.position;
        return Vector2.zero;
    }

    // ?? Gizmos ???????????????????????????????????????????????????????

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Vector2 center = GetCenter();

        if (areaMode == SpawnAreaMode.Donut)
        {
            DrawGizmosDonut(center);
        }
        else
        {
            DrawGizmosRect(center);
        }
    }

    void DrawGizmosDonut(Vector2 center)
    {
        // Zona exterior rellena
        Gizmos.color = gizmoColorOuter;
        DrawFilledCircleGizmo(center, outerRadius);

        // Zona interior excluida (sobre la exterior para "perforarla" visualmente)
        Gizmos.color = gizmoColorInner;
        DrawFilledCircleGizmo(center, innerRadius);

        // Bordes nítidos
        Gizmos.color = new Color(gizmoColorOuter.r, gizmoColorOuter.g, gizmoColorOuter.b, 0.9f);
        DrawWireCircleGizmo(center, outerRadius);

        Gizmos.color = new Color(gizmoColorInner.r, gizmoColorInner.g, gizmoColorInner.b, 0.9f);
        DrawWireCircleGizmo(center, innerRadius);

        // Label radios
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            new Vector3(center.x + outerRadius + 0.2f, center.y, 0),
            $"outer: {outerRadius:F1}");
        UnityEditor.Handles.Label(
            new Vector3(center.x + innerRadius + 0.2f, center.y, 0),
            $"inner: {innerRadius:F1}");
#endif
    }

    void DrawGizmosRect(Vector2 center)
    {
        // Rectángulo exterior
        Gizmos.color = gizmoColorOuter;
        Gizmos.DrawCube(center, new Vector3(rectSize.x, rectSize.y, 0));

        Gizmos.color = new Color(gizmoColorOuter.r, gizmoColorOuter.g, gizmoColorOuter.b, 0.9f);
        Gizmos.DrawWireCube(center, new Vector3(rectSize.x, rectSize.y, 0));

        // Zona de exclusión interior
        Gizmos.color = gizmoColorInner;
        DrawFilledCircleGizmo(center, rectInnerExclusion);

        Gizmos.color = new Color(gizmoColorInner.r, gizmoColorInner.g, gizmoColorInner.b, 0.9f);
        DrawWireCircleGizmo(center, rectInnerExclusion);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            new Vector3(center.x + rectSize.x / 2f + 0.2f, center.y, 0),
            $"{rectSize.x:F1} x {rectSize.y:F1}");
        UnityEditor.Handles.Label(
            new Vector3(center.x + rectInnerExclusion + 0.2f, center.y - 0.4f, 0),
            $"exclusion: {rectInnerExclusion:F1}");
#endif
    }

    // ?? Helpers Gizmo ????????????????????????????????????????????????

    void DrawWireCircleGizmo(Vector2 center, float radius, int segments = 64)
    {
        float step = Mathf.PI * 2f / segments;
        Vector3 prev = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float a = i * step;
            Vector3 next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

    void DrawFilledCircleGizmo(Vector2 center, float radius, int segments = 64)
    {
        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a1 = i * step, a2 = (i + 1) * step;
            Vector3 v1 = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            Vector3 v2 = center + new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius;
            Gizmos.DrawLine((Vector3)(Vector2)center, v1);
            Gizmos.DrawLine(v1, v2);
        }
    }
}