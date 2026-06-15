using UnityEngine;

[System.Serializable]
public class MechanicModifierOfferData
{
    public MechanicModifierType modifier;
    public string displayName;

    [TextArea]
    public string description;

    public int cost = 50;

    [Header("Unlock")]
    public int unlockWave = 1;
}