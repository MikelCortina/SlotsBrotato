using System.Collections.Generic;
using UnityEngine;

public class ChainLightning : MonoBehaviour
{
    public static ChainLightning Instance;
    [Header("Visual")]
    public LightningEffect lightningPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void Trigger(Vector2 origin, int chains, float radius, float damage)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
            return;

        List<EnemyHealth> hitEnemies = new List<EnemyHealth>();

        EnemyHealth current = FindClosestEnemy(origin, enemies, hitEnemies);

        for (int i = 0; i < chains; i++)
        {
            if (current == null)
                break;

            Vector2 startPos = origin;
            Vector2 endPos = current.transform.position;

            if (lightningPrefab != null)
            {
                LightningEffect fx = Instantiate(lightningPrefab, Vector3.zero, Quaternion.identity);
                fx.Setup(startPos, endPos);
            }

            hitEnemies.Add(current);

            current.TakeDamage(damage, startPos, DamageSource.Trap);

            origin = endPos;

            current = FindClosestEnemy(origin, enemies, hitEnemies, radius);
        }
    }

    EnemyHealth FindClosestEnemy(
        Vector2 pos,
        GameObject[] enemies,
        List<EnemyHealth> ignore,
        float maxDist = 999f)
    {
        EnemyHealth closest = null;
        float bestDist = maxDist;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();

            if (eh == null) continue;
            if (ignore.Contains(eh)) continue;

            float dist = Vector2.Distance(pos, enemy.transform.position);

            if (dist < bestDist)
            {
                bestDist = dist;
                closest = eh;
            }
        }

        return closest;
    }
}