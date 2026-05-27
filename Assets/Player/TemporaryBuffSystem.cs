using System.Collections;
using UnityEngine;

public class TemporaryBuffSystem : MonoBehaviour
{
    private PlayerStats _stats;

    void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    public void ApplyDamageBuff(float amount, float duration)
    {
        StartCoroutine(DamageBuffRoutine(amount, duration));
    }

    IEnumerator DamageBuffRoutine(float amount, float duration)
    {
        if (_stats == null) yield break;

        _stats.damage += amount;

        Debug.Log($"+{amount} damage durante {duration}s");

        yield return new WaitForSeconds(duration);

        _stats.damage -= amount;

        Debug.Log($"Buff terminado");
    }
}