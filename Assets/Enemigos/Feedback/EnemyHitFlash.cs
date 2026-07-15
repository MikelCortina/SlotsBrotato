using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHitFlash : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string hitTriggerName = "Hit";

    [Header("Bounce Reset")]
    [SerializeField] private bool resetBounceOnHit = true;
    [SerializeField] private float groundLockTimeOverride = -1f;

    private EnemyHealth _health;
    private EnemyController _enemyController;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _enemyController = GetComponent<EnemyController>();

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
        if (resetBounceOnHit && _enemyController != null)
            _enemyController.OnReceiveDamageBounceReset(groundLockTimeOverride);

        if (targetAnimator == null)
        {
            Debug.LogWarning($"{name}: targetAnimator es null");
            return;
        }

        targetAnimator.ResetTrigger(hitTriggerName);
        targetAnimator.SetTrigger(hitTriggerName);
    }
}