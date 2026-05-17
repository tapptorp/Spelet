using UnityEngine;

public enum CardType
{
    Attack,
    Defense,
    Versatile,
    Special
}

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string cardName;
    [SerializeField] private CardType cardType;

    [Header("Combat")]
    [SerializeField] private int value;

    [Header("Text")]
    [TextArea]
    [SerializeField] private string description;

    public string CardName => cardName;
    public CardType CardType => cardType;
    public int Value => value;
    public string Description => description;
}