using UnityEngine;

public class DashAnimationEvents : MonoBehaviour
{
    [SerializeField] DasherEnemyController controller;

    public void AnimationEvent_BeginDash()
    {
        controller.AnimationEvent_BeginDash();
    }

   
}