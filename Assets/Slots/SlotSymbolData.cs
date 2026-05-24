using UnityEngine;

[CreateAssetMenu(fileName = "New Slot Symbol", menuName = "Slots/Slot Symbol")]
public class SlotSymbolData : ScriptableObject
{
    public string symbolName;
    public SlotSymbolType symbolType;
    public Sprite icon;

    [Header("Activation")]
    public bool activateInstantly = false;
}