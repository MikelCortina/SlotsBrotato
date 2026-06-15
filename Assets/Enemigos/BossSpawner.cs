using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss")]
    public GameObject bossPrefab;

    [Header("Spawn")]
    public int bossWaveInterval = 5;
    public Transform spawnPoint;

    public void TrySpawnBoss(int wave)
    {
        if (bossPrefab == null) return;
        if (bossWaveInterval <= 0) return;
        if (wave % bossWaveInterval != 0) return;

        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : Vector3.zero;

        GameObject boss = Instantiate(bossPrefab, position, Quaternion.identity);

        GameManager.Instance?.RegisterEnemy(boss);

        Debug.Log($"Boss spawneado en oleada {wave}");
    }
}