using UnityEngine;

public class CharacterUnit : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterData characterData;

    [Header("Runtime State")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentAttackRange;

    public Tile CurrentTile { get; private set; }
    public bool IsPlacedOnTile => CurrentTile != null;
    public bool HasCharacterData => characterData != null;

    // ===== DATA FRÅN CHARACTER DATA =====

    public string CharacterName => characterData != null ? characterData.CharacterName : gameObject.name;

    public int MaxHealth => characterData != null ? characterData.MaxHealth : 0;

    public int Movement => characterData != null ? characterData.Movement : 0;

    public int AttackRange => characterData != null ? characterData.AttackRange : 0;

    // ===== RUNTIME VALUES =====

    public int CurrentHealth => currentHealth;

    public bool IsDead => currentHealth <= 0;

    public int CurrentAttackRange => currentAttackRange;



    private void Awake()
    {
        if (characterData == null)
        {
            Debug.LogWarning($"{gameObject.name} is missing CharacterData.");
            currentHealth = 0;
            currentAttackRange = 0;
            return;
        }

        currentHealth = MaxHealth;
        currentAttackRange = AttackRange;
    }

    public void PlaceOnTile(Tile newTile)
    {
        if (newTile == null)
        {
            Debug.LogWarning($"{CharacterName} cannot be placed on a null tile.");
            return;
        }

        if (newTile.IsOccupied && newTile.OccupyingUnit != this)
        {
            Debug.LogWarning($"{CharacterName} cannot be placed on {newTile.name}, because it is already occupied.");
            return;
        }

        // Rensa gamla tile
        if (CurrentTile != null)
        {
            CurrentTile.ClearOccupyingUnit();
        }

        // Sätt nya
        CurrentTile = newTile;
        CurrentTile.SetOccupyingUnit(this);

        // Flytta objektet visuellt
        transform.position = newTile.transform.position + Vector3.up * 1f;
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

