using UnityEngine;

public enum PlayerTurn
{
    PlayerOne,
    PlayerTwo
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private CharacterUnit playerOneUnit;
    [SerializeField] private CharacterUnit playerTwoUnit;

    [Header("Start Positions")]
    [SerializeField] private Vector2Int playerOneStartPosition = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int playerTwoStartPosition = new Vector2Int(-5, 5);

    [Header("Turn Settings")]
    [SerializeField] private int actionsPerTurn = 2;

    [Header("Temporary Debug Combat")]
    [SerializeField] private int basicAttackDamage = 1;

    [Header("Runtime State")]
    [SerializeField] private PlayerTurn activePlayer = PlayerTurn.PlayerOne;
    [SerializeField] private bool isGameOver;

    [Header("Player States")]
    [SerializeField] private PlayerState playerOne;
    [SerializeField] private PlayerState playerTwo;

    public PlayerState PlayerOne => playerOne;
    public PlayerState PlayerTwo => playerTwo;

    public CharacterUnit PlayerOneUnit => playerOneUnit;
    public CharacterUnit PlayerTwoUnit => playerTwoUnit;

    public PlayerTurn ActivePlayer => activePlayer;

    public PlayerState ActivePlayerState => activePlayer == PlayerTurn.PlayerOne
        ? playerOne
        : playerTwo;

    public PlayerState EnemyPlayerState => activePlayer == PlayerTurn.PlayerOne
        ? playerTwo
        : playerOne;

    public CharacterUnit ActiveUnit => ActivePlayerState?.Unit;
    public CharacterUnit EnemyUnit => EnemyPlayerState?.Unit;

    public int ActionsRemainingThisTurn => ActivePlayerState != null
        ? ActivePlayerState.ActionsRemaining
        : 0;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        if (boardManager == null)
        {
            boardManager = FindAnyObjectByType<BoardManager>();
        }

        if (movementManager == null)
        {
            movementManager = FindAnyObjectByType<MovementManager>();
        }

        if (movementManager == null)
        {
            movementManager = gameObject.AddComponent<MovementManager>();
            Debug.Log("GameManager created a MovementManager automatically.");
        }
    }

    private void Start()
    {
        if (!ValidateSetup())
        {
            Debug.LogError("Game could not start because setup validation failed.");
            enabled = false;
            return;
        }

        StartGame();
    }

    private bool ValidateSetup()
    {
        bool isValid = true;

        if (boardManager == null)
        {
            Debug.LogError("GameManager setup error: BoardManager is missing.");
            isValid = false;
        }

        if (movementManager == null)
        {
            Debug.LogError("GameManager setup error: MovementManager is missing.");
            isValid = false;
        }

        if (playerOneUnit == null)
        {
            Debug.LogError("GameManager setup error: Player One Unit is missing.");
            isValid = false;
        }
        else if (!playerOneUnit.HasCharacterData)
        {
            Debug.LogError("GameManager setup error: Player One Unit is missing CharacterData.");
            isValid = false;
        }

        if (playerTwoUnit == null)
        {
            Debug.LogError("GameManager setup error: Player Two Unit is missing.");
            isValid = false;
        }
        else if (!playerTwoUnit.HasCharacterData)
        {
            Debug.LogError("GameManager setup error: Player Two Unit is missing CharacterData.");
            isValid = false;
        }

        if (boardManager != null)
        {
            ValidateStartTiles(ref isValid);
        }

        ValidateCharacterDecks();

        return isValid;
    }

    private void ValidateCharacterDecks()
    {
        if (playerOneUnit != null && playerOneUnit.StartingDeck.Count == 0)
        {
            Debug.LogWarning($"{playerOneUnit.CharacterName} has an empty starting deck. This is okay for now, but Maneuver will not be able to draw cards.");
        }

        if (playerTwoUnit != null && playerTwoUnit.StartingDeck.Count == 0)
        {
            Debug.LogWarning($"{playerTwoUnit.CharacterName} has an empty starting deck. This is okay for now, but Maneuver will not be able to draw cards.");
        }
    }

    private void ValidateStartTiles(ref bool isValid)
    {
        Tile playerOneStartTile = boardManager.GetTileAtPosition(playerOneStartPosition);
        Tile playerTwoStartTile = boardManager.GetTileAtPosition(playerTwoStartPosition);

        if (playerOneStartTile == null)
        {
            Debug.LogError($"GameManager setup error: No tile found at Player One start position {playerOneStartPosition}.");
            isValid = false;
        }

        if (playerTwoStartTile == null)
        {
            Debug.LogError($"GameManager setup error: No tile found at Player Two start position {playerTwoStartPosition}.");
            isValid = false;
        }

        if (playerOneStartPosition == playerTwoStartPosition)
        {
            Debug.LogError("GameManager setup error: Player One and Player Two have the same start position.");
            isValid = false;
        }
    }

    private void StartGame()
    {
        bool playerOnePlaced = PlaceUnitAtStartPosition(playerOneUnit, playerOneStartPosition);
        bool playerTwoPlaced = PlaceUnitAtStartPosition(playerTwoUnit, playerTwoStartPosition);

        if (!playerOnePlaced || !playerTwoPlaced)
        {
            Debug.LogWarning("Game could not start. One or more units could not be placed.");
            return;
        }

        playerOne = new PlayerState("Player One", playerOneUnit);
        playerTwo = new PlayerState("Player Two", playerTwoUnit);

        Debug.Log($"Player One deck created from {playerOneUnit.CharacterName}. Draw pile: {playerOne.Deck.DrawPileCount}");
        Debug.Log($"Player Two deck created from {playerTwoUnit.CharacterName}. Draw pile: {playerTwo.Deck.DrawPileCount}");

        activePlayer = PlayerTurn.PlayerOne;
        isGameOver = false;

        StartTurn();
    }

    private bool PlaceUnitAtStartPosition(CharacterUnit unit, Vector2Int startPosition)
    {
        Tile startTile = boardManager.GetTileAtPosition(startPosition);

        if (startTile == null)
        {
            Debug.LogWarning($"Could not place {unit.CharacterName}. No tile found at {startPosition}.");
            return false;
        }

        if (startTile.IsOccupied)
        {
            Debug.LogWarning($"Could not place {unit.CharacterName}. Tile at {startPosition} is occupied.");
            return false;
        }

        unit.PlaceOnTile(startTile);
        return true;
    }

    private void StartTurn()
    {
        if (isGameOver)
        {
            return;
        }

        ActivePlayerState.StartTurn(actionsPerTurn);

        Debug.Log($"Starting turn for {ActivePlayerState.PlayerName}. Active unit: {ActiveUnit.CharacterName}. Actions: {ActivePlayerState.ActionsRemaining}");
    }

    private void SwitchTurn()
    {
        if (isGameOver)
        {
            return;
        }

        activePlayer = activePlayer == PlayerTurn.PlayerOne
            ? PlayerTurn.PlayerTwo
            : PlayerTurn.PlayerOne;

        StartTurn();
    }

    private void ConsumeAction()
    {
        ActivePlayerState.ConsumeAction();

        if (!ActivePlayerState.HasActionsRemaining)
        {
            Debug.Log($"{ActiveUnit.CharacterName} has used all actions.");
            SwitchTurn();
        }
    }

    public bool TryMoveActiveUnitToPosition(Vector2Int targetPosition)
    {
        // Temporary wrapper:
        // Existing click/debug code still calls "move",
        // but the action is now treated as a Maneuver.
        return TryManeuverActiveUnitToPosition(targetPosition);
    }

    public bool TryMoveActiveUnitToTile(Tile targetTile)
    {
        // Temporary wrapper:
        // Existing click/debug code still calls "move",
        // but the action is now treated as a Maneuver.
        return TryManeuverActiveUnitToTile(targetTile);
    }

    public bool TryManeuverActiveUnitToPosition(Vector2Int targetPosition)
    {
        Tile targetTile = boardManager.GetTileAtPosition(targetPosition);

        if (targetTile == null)
        {
            Debug.Log($"No tile found at {targetPosition}.");
            return false;
        }

        return TryManeuverActiveUnitToTile(targetTile);
    }

    public bool TryManeuverActiveUnitToTile(Tile targetTile)
    {
        string validationError = GetActiveUnitManeuverValidationError(targetTile);

        if (validationError != null)
        {
            Debug.Log(validationError);
            return false;
        }

        // Important:
        // The target has been validated before drawing a card.
        // This prevents accidental invalid clicks from drawing cards.
        CardData drawnCard = ActivePlayerState.Deck.DrawCard();

        if (drawnCard != null)
        {
            Debug.Log($"{ActivePlayerState.PlayerName} maneuvered and drew {drawnCard.CardName}.");
        }
        else
        {
            // Fatigue damage should be added here later.
            // For now, the maneuver is still allowed, but no card was drawn.
            Debug.Log($"{ActivePlayerState.PlayerName} maneuvered, but the draw pile was empty. Fatigue damage is not implemented yet.");
        }

        bool moved = movementManager.TryMoveUnit(ActiveUnit, targetTile, out validationError);

        if (!moved)
        {
            Debug.Log(validationError);
            return false;
        }

        Debug.Log($"{ActiveUnit.CharacterName} maneuvered to {targetTile.GridPosition}.");

        ConsumeAction();

        return true;
    }

    public bool CanActiveUnitMoveToTile(Tile targetTile)
    {
        return GetActiveUnitManeuverValidationError(targetTile) == null;
    }

    public bool CanActiveUnitManeuverToTile(Tile targetTile)
    {
        return GetActiveUnitManeuverValidationError(targetTile) == null;
    }

    private string GetActiveUnitManeuverValidationError(Tile targetTile)
    {
        if (isGameOver)
        {
            return "Cannot maneuver. Game is over.";
        }

        if (ActivePlayerState == null)
        {
            return "Cannot maneuver. Active player state is missing.";
        }

        if (ActiveUnit == null)
        {
            return "Cannot maneuver. Active unit is missing.";
        }

        if (ActivePlayerState.Deck == null)
        {
            return "Cannot maneuver. Active player deck is missing.";
        }

        if (!ActivePlayerState.HasActionsRemaining)
        {
            return $"{ActiveUnit.CharacterName} has no actions left.";
        }

        return movementManager.GetMoveValidationError(ActiveUnit, targetTile);
    }

    public bool TryBasicAttack()
    {
        if (isGameOver)
        {
            Debug.Log("Cannot attack. Game is over.");
            return false;
        }

        if (ActivePlayerState == null)
        {
            Debug.LogWarning("Cannot attack. Active player state is missing.");
            return false;
        }

        if (!ActivePlayerState.HasActionsRemaining)
        {
            Debug.Log($"{ActiveUnit.CharacterName} has no actions left.");
            return false;
        }

        if (ActiveUnit.CurrentTile == null || EnemyUnit.CurrentTile == null)
        {
            Debug.LogWarning("Cannot attack. One or both units are not placed on tiles.");
            return false;
        }

        if (!IsEnemyInRange())
        {
            Debug.Log($"{EnemyUnit.CharacterName} is out of range.");
            return false;
        }

        EnemyUnit.TakeDamage(basicAttackDamage);

        Debug.Log($"{ActiveUnit.CharacterName} attacked {EnemyUnit.CharacterName} for {basicAttackDamage} damage.");

        CheckGameOver();

        if (!isGameOver)
        {
            ConsumeAction();
        }

        return true;
    }

    private bool IsEnemyInRange()
    {
        Vector2Int activePosition = ActiveUnit.CurrentTile.GridPosition;
        Vector2Int enemyPosition = EnemyUnit.CurrentTile.GridPosition;

        int distance = Mathf.Abs(activePosition.x - enemyPosition.x) +
                       Mathf.Abs(activePosition.y - enemyPosition.y);

        return distance <= ActiveUnit.CurrentAttackRange;
    }

    private void CheckGameOver()
    {
        if (playerOne.Unit.IsDead)
        {
            isGameOver = true;
            Debug.Log("Game over. Player Two wins!");
            return;
        }

        if (playerTwo.Unit.IsDead)
        {
            isGameOver = true;
            Debug.Log("Game over. Player One wins!");
        }
    }
}