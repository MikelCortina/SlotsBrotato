using UnityEngine;

[CreateAssetMenu(fileName = "New Slot Symbol", menuName = "Slots/Slot Symbol")]
public class SlotSymbolData : ScriptableObject
{
    public string symbolName;
    public SlotSymbolType symbolType;
    public Sprite icon;

    [Header("Activation")]
    public bool activateInstantly = false;

    [Header("Effect Values")]
    public float baseEffectValue = 1f;
    public float valuePerLevel = 1f;
    public float jackpotMultiplier = 5f;
}