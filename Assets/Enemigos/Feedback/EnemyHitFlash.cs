using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFlash : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string hitTriggerName = "Hit";

    private EnemyHealth _health;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();

        if (targetAnimator == null)
            targetAnimator = GetComponentInChildren<Animator>();
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
        if (targetAnimator == null)
        {
            Debug.LogWarning($"{name}: targetAnimator es null");
            return;
        }

        Debug.Log($"{name}: Trigger lanzado -> {hitTriggerName}");
        targetAnimator.ResetTrigger(hitTriggerName);
        targetAnimator.SetTrigger(hitTriggerName);
    }


}