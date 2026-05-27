using UnityEngine;

[CreateAssetMenu(fileName = "New Passive", menuName = "Passives/Passive")]
public class PassiveData : ScriptableObject
{
    public string passiveName;
    public Sprite icon;

    [Header("Stats")]
    public float bonusDamage;
    public float bonusFireRate;
    public float bonusMoveSpeed;
    public int bonusMaxHealth;
}