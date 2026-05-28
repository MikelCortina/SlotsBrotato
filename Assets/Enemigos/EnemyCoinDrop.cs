using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyCoinDrop : MonoBehaviour
{
    [Header("Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 1;
    [SerializeField] private int maxCoins = 3;
    [SerializeField] private float dropRadius = 0.35f;
    [SerializeField] private float spawnOffsetY = 0.25f;

    private EnemyHealth _health;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (coinPrefab == null) return;

        int count = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset + Vector3.up * spawnOffsetY;
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }
}