using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string characterName;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int movement = 2;
    [SerializeField] private int attackRange = 1;

    [Header("Deck")]
    [SerializeField] private List<CardData> startingDeck = new List<CardData>();

    // ===== PUBLIC GETTERS =====

    public string CharacterName => characterName;
    public int MaxHealth => maxHealth;
    public int Movement => movement;
    public int AttackRange => attackRange;
    public IReadOnlyList<CardData> StartingDeck => startingDeck;
}