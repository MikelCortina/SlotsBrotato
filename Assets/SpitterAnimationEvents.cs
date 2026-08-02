using UnityEngine;

public class SpitterAnimationEvents : MonoBehaviour
{
    public SpitterEnemyController controller;

    public void ShootFromAnimation()
    {
        if (controller != null)
            controller.ShootFromAnimation();
    }

    public void EndAttackAnimation()
    {
        if (controller != null)
            controller.EndAttackAnimation();
    }
}