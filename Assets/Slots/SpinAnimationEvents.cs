using UnityEngine;

public class SpinAnimationEvents : MonoBehaviour
{
    public void OnSpinAnimationReady()
    {
        SlotMachine.Instance?.OnSpinAnimationReady();
    }
}