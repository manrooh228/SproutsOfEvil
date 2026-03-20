using UnityEngine;


public enum CardRarity { Common, Rare, Epic }
public enum StatusEffectType { None, DamageOverTime, HealOverTime, BlockOverTime, StrengthBuff, AgilityBuff }

[CreateAssetMenu(fileName = "NewCard", menuName = "CardGame/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea] public string description;
    public CardRarity rarity;

    [Header("Базовые параметры")]
    public int energyCost;
    public int damage;
    public int block;
    public int heal;

    [Header("Длительные эффекты (Status Effects)")]
    public StatusEffectType effectType;
    public int effectValue; // Величина эффекта (урон/хилл/сила)
    public int duration;    // На сколько ходов

    [Header("Откат (Cooldown)")]
    public int cooldownTurns;

    public Sprite artwork;
}
