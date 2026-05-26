using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyComboChargeReporter : MonoBehaviour
{
    private EnemyHealth _health;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    void OnEnable()
    {
        if (_health != null)
            _health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        if (_health != null)
            _health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(float amount, float currentHp)
    {
       // SlotMachine.Instance?.OnEnemyDamaged(amount);
    }
}