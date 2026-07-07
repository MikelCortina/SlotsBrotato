using UnityEngine;

public class EnemyDebugSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Enemy Prefabs")]
    public GameObject normalEnemyPrefab;
    public GameObject spitterPrefab;
    public GameObject ringSpitterPrefab;
    public GameObject dasherPrefab;
    public GameObject bossPrefab;

    public void SpawnNormal()
    {
        Spawn(normalEnemyPrefab);
    }

    public void SpawnSpitter()
    {
        Spawn(spitterPrefab);
    }

    public void SpawnRingSpitter()
    {
        Spawn(ringSpitterPrefab);
    }

    public void SpawnDasher()
    {
        Spawn(dasherPrefab);
    }

    public void SpawnBoss()
    {
        Spawn(bossPrefab);
    }

    public void ClearEnemies()
    {
        EnemyHealth[] enemies =
            FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
            Destroy(enemy.gameObject);
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null || spawnPoint == null)
            return;

        Instantiate(prefab, spawnPoint.position, Quaternion.identity);
    }
}