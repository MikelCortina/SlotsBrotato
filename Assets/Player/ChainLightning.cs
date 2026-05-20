using System.Collections.Generic;
using UnityEngine;

public class ChainLightning : MonoBehaviour
{
    public static ChainLightning Instance;

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

            current.TakeDamage(damage, origin, DamageSource.Trap);

            hitEnemies.Add(current);

            current = FindClosestEnemy(
                current.transform.position,
                enemies,
                hitEnemies,
                radius
            );
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