using UnityEngine;

[CreateAssetMenu(fileName = "NewBooster", menuName = "Boosters/BoosterData")]
public class BoosterData : ScriptableObject
{
    [Header("Info")]
    public string boosterName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Cost")]
    public int voucherCost = 5;

    [Header("Colors")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;
    public Color equippedColor = Color.green;

    [Header("ID (único, no cambiar)")]
    public string id; // ej: "health_boost_10"

    [Header("Effect")]
    public int bonusMaxHealth = 0;
    // aquí irán más efectos en el futuro (daño, velocidad, etc.)
}