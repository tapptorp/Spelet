using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Attack,
    Defense,
    Versatile,
    Special
}

public enum CardEffectTiming
{
    Immediately,
    DuringCombat,
    AfterCombat
}

public enum CardEffectType
{
    DrawCards,
    Heal,
    DealDamage,
    BonusAttack,
    BonusDefense
}

public enum CardEffectTarget
{
    Self,
    Opponent,
    Attacker,
    Defender
}

[System.Serializable]
public class CardEffectData
{
    [SerializeField] private CardEffectTiming timing;
    [SerializeField] private CardEffectType effectType;
    [SerializeField] private CardEffectTarget target;
    [SerializeField] private int value = 1;

    public CardEffectTiming Timing => timing;
    public CardEffectType EffectType => effectType;
    public CardEffectTarget Target => target;
    public int Value => value;
}

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string cardName;
    [SerializeField] private CardType cardType;

    [Header("Combat")]
    [SerializeField] private int value;

    [Header("Effects")]
    [SerializeField] private List<CardEffectData> effects = new List<CardEffectData>();

    [Header("Text")]
    [TextArea]
    [SerializeField] private string description;

    public string CardName => cardName;
    public CardType CardType => cardType;
    public int Value => value;
    public IReadOnlyList<CardEffectData> Effects => effects;
    public string Description => description;
}


