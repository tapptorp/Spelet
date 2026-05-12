using UnityEngine;

public class CharacterUnit : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterData characterData;

    [Header("Runtime State")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentAttackRange;

    public Tile CurrentTile { get; private set; }

    // ===== DATA FRÅN CHARACTER DATA =====

    public string CharacterName => characterData.CharacterName;

    public int MaxHealth => characterData.MaxHealth;

    public int Movement => characterData.Movement;

    public int AttackRange => characterData.AttackRange;

    // ===== RUNTIME VALUES =====

    public int CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0;

    public int CurrentAttackRange => currentAttackRange;

    private void Awake()
    {
        currentHealth = MaxHealth;
        currentAttackRange = characterData.AttackRange;
    }

    public void PlaceOnTile(Tile newTile)
    {
        // Rensa gamla tile
        if (CurrentTile != null)
        {
            CurrentTile.SetOccupyingUnit(null);
        }

        // Sätt nya
        CurrentTile = newTile;
        CurrentTile.SetOccupyingUnit(this);

        // Flytta objektet visuellt
        transform.position = newTile.transform.position + Vector3.up * 0.5f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"{CharacterName} took {amount} damage.");

        if (IsDead)
        {
            Debug.Log($"{CharacterName} died.");
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        currentHealth = Mathf.Min(currentHealth, MaxHealth);
    }
}