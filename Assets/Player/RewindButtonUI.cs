using UnityEngine;
using UnityEngine.UI;

public class RewindButtonUI : MonoBehaviour
{
    public Button button;

    void Update()
    {
        bool hasRewind =
            MechanicModifierManager.Instance != null &&
            MechanicModifierManager.Instance.HasModifier(MechanicModifierType.Rewind);

        bool canUse =
            SlotMachine.Instance != null &&
            SlotMachine.Instance.CanRewind();

        if (button != null)
            button.gameObject.SetActive(hasRewind);

        if (button != null)
            button.interactable = hasRewind && canUse;
    }

    public void UseRewind()
    {
        if (MechanicModifierManager.Instance == null) return;

        if (!MechanicModifierManager.Instance.HasModifier(
            MechanicModifierType.Rewind))
            return;

        SlotMachine.Instance?.RewindSpin();
    }
}