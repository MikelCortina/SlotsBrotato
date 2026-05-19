using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHp = 50f;
    private float _hp;
    private Action _onDeath;

    void Awake() => _hp = maxHp;

    public void SubscribeOnDeath(Action callback) => _onDeath += callback;

    public void TakeDamage(float amount)
    {
        _hp -= amount;
        if (_hp <= 0f) Die();
    }

    void Die()
    {
        _onDeath?.Invoke();
        Destroy(gameObject);
    }
}