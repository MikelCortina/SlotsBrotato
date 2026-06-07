using TMPro;
using UnityEngine;

public class ActiveModifiersUI : MonoBehaviour
{
    public TextMeshProUGUI modifiersText;

    void Update()
    {
        if (modifiersText == null) return;

        if (MechanicModifierManager.Instance == null)
        {
            modifiersText.text = "Modificadores: ninguno";
            return;
        }

        string text = "Modificadores activos:\n";
        bool hasAny = false;

        foreach (var modifier in MechanicModifierManager.Instance.GetActiveModifiers())
        {
            text += $"- {modifier}\n";
            hasAny = true;
        }

        if (!hasAny)
            text = "Modificadores activos:\nNinguno";

        modifiersText.text = text;
    }
}