using UnityEngine;


public enum CardRarity { Common, Rare, Epic }
[CreateAssetMenu(fileName = "NewCard", menuName = "CardGame/Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea] public string description;
    public CardRarity rarity;
    public int energyCost;
    public int damage;
    public int block;
    public Sprite artwork;
}
