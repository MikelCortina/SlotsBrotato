using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightning : MonoBehaviour
{
    public static ChainLightning Instance;

    [Header("Visual")]
    public LightningEffect lightningPrefab;
    public float jumpDelay = 0.08f;

    void Awake()
    {
        Instance = this;
    }

    public void Trigger(Transform player, int chains, float radius, float damage)
    {
        StartCoroutine(TriggerRoutine(player, chains, radius, damage));
    }

    IEnumerator TriggerRoutine(Transform player, int chains, float radius, float damage)
    {
        Vector2 origin = player.position;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            Debug.Log("[ChainLightning] No hay enemigos con tag 'Enemy'");
            yield break;
        }

        List<EnemyHealth> hitEnemies = new List<EnemyHealth>();
        EnemyHealth current = FindClosestEnemy(origin, enemies, hitEnemies, float.MaxValue);

        if (current == null)
        {
            Debug.Log("[ChainLightning] No se encontró ningún enemigo inicial (current == null)");
            yield break;
        }

        Debug.Log($"[ChainLightning] Inicio: chains={chains}, enemigos totales con tag 'Enemy'={enemies.Length}");

        for (int i = 0; i < chains; i++)
        {
            if (current == null)
            {
                Debug.Log($"[ChainLightning] Se detuvo en salto {i}: current == null (no hay más enemigos vivos)");
                yield break;
            }

            Vector2 startPos = origin;
            Vector2 endPos = current.transform.position;

            if (lightningPrefab != null)
            {
                LightningEffect fx = Instantiate(lightningPrefab, new Vector3(startPos.x, startPos.y, 0f), Quaternion.identity);
                fx.Setup(startPos, endPos);
            }

            hitEnemies.Add(current);
            current.TakeDamage(damage, startPos, DamageSource.Trap);

            Debug.Log($"[ChainLightning] Salto {i}: golpeó a {current.gameObject.name}, hitEnemies.Count={hitEnemies.Count}");

            origin = endPos;
            current = FindClosestEnemy(origin, enemies, hitEnemies, float.MaxValue);

            if (i < chains - 1)
                yield return new WaitForSeconds(jumpDelay);
        }

        Debug.Log($"[ChainLightning] Terminó: total de saltos = {hitEnemies.Count}");
    }

    EnemyHealth FindClosestEnemy(
        Vector2 pos,
        GameObject[] enemies,
        List<EnemyHealth> ignore,
        float maxDist = float.MaxValue)
    {
        EnemyHealth closest = null;
        float bestDist = maxDist;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (!enemy.activeInHierarchy) continue;

            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh == null) continue;
            if (ignore.Contains(eh)) continue;

            // Solo enemigos vivos
            if (eh.currentHealth <= 0) continue;

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