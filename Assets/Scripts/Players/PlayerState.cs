using UnityEngine;

[System.Serializable]
public class PlayerState
{
    [SerializeField] private string playerName;
    [SerializeField] private CharacterUnit unit;
    [SerializeField] private int actionsRemaining;
    [SerializeField] private Deck deck;

    public string PlayerName => playerName;
    public CharacterUnit Unit => unit;
    public int ActionsRemaining => actionsRemaining;
    public bool HasActionsRemaining => actionsRemaining > 0;
    public Deck Deck => deck;

    public PlayerState(string playerName, CharacterUnit unit)
    {
        this.playerName = playerName;
        this.unit = unit;
        actionsRemaining = 0;

        deck = new Deck(unit != null ? unit.StartingDeck : null);
    }

    public void StartTurn(int actionsPerTurn)
    {
        actionsRemaining = actionsPerTurn;
    }

    public void ConsumeAction()
    {
        if (actionsRemaining <= 0)
        {
            return;
        }

        actionsRemaining--;
    }
}