using UnityEngine;

public class EnemyVisualAnimationRelay : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public void DestroyEnemyFromAnimation()
    {
        if (enemyHealth != null)
            enemyHealth.DestroyEnemy();
    }
}