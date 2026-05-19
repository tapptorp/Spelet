using UnityEngine;

public enum PlayerTurn
{
    PlayerOne,
    PlayerTwo
}

public enum GameInputState
{
    WaitingForActivePlayerAction,
    SelectingAttackTarget,
    DefenderChoosingDefense,
    GameOver
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
    [SerializeField] private GameInputState inputState = GameInputState.WaitingForActivePlayerAction;

    [Header("Player States")]
    [SerializeField] private PlayerState playerOne;
    [SerializeField] private PlayerState playerTwo;

    private CardData selectedAttackCard;

    public PlayerState PlayerOne => playerOne;
    public PlayerState PlayerTwo => playerTwo;

    public CharacterUnit PlayerOneUnit => playerOneUnit;
    public CharacterUnit PlayerTwoUnit => playerTwoUnit;

    public PlayerTurn ActivePlayer => activePlayer;
    public GameInputState InputState => inputState;
    public bool IsGameOver => isGameOver;

    public PlayerState ActivePlayerState => activePlayer == PlayerTurn.PlayerOne
        ? playerOne
        : playerTwo;

    public PlayerState EnemyPlayerState => activePlayer == PlayerTurn.PlayerOne
        ? playerTwo
        : playerOne;

    public CharacterUnit ActiveUnit => ActivePlayerState?.Unit;
    public CharacterUnit EnemyUnit => EnemyPlayerState?.Unit;

    public CardData SelectedAttackCard => selectedAttackCard;

    public int ActionsRemainingThisTurn => ActivePlayerState != null
        ? ActivePlayerState.ActionsRemaining
        : 0;

    private void Awake()
    {
        FindMissingReferences();
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

    private void FindMissingReferences()
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

        ValidateUnitSetup(playerOneUnit, "Player One", ref isValid);
        ValidateUnitSetup(playerTwoUnit, "Player Two", ref isValid);

        if (boardManager != null)
        {
            ValidateStartTiles(ref isValid);
        }

        ValidateCharacterDecks();

        return isValid;
    }

    private void ValidateUnitSetup(CharacterUnit unit, string playerLabel, ref bool isValid)
    {
        if (unit == null)
        {
            Debug.LogError($"GameManager setup error: {playerLabel} Unit is missing.");
            isValid = false;
            return;
        }

        if (!unit.HasCharacterData)
        {
            Debug.LogError($"GameManager setup error: {playerLabel} Unit is missing CharacterData.");
            isValid = false;
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
        inputState = GameInputState.WaitingForActivePlayerAction;
        selectedAttackCard = null;

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

        inputState = GameInputState.WaitingForActivePlayerAction;
        selectedAttackCard = null;

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

    public bool HandleTileClicked(Tile clickedTile)
    {
        if (clickedTile == null)
        {
            Debug.LogWarning("Cannot handle tile click. Clicked tile is null.");
            return false;
        }

        if (isGameOver)
        {
            Debug.Log("Cannot handle tile click. Game is over.");
            return false;
        }

        switch (inputState)
        {
            case GameInputState.WaitingForActivePlayerAction:
                return TryManeuverActiveUnitToTile(clickedTile);

            case GameInputState.SelectingAttackTarget:
                return TrySelectAttackTarget(clickedTile);

            case GameInputState.DefenderChoosingDefense:
                Debug.Log("Cannot handle tile click. Waiting for defender to choose a defense card.");
                return false;

            case GameInputState.GameOver:
                Debug.Log("Cannot handle tile click. Game is over.");
                return false;

            default:
                Debug.Log($"Unhandled input state: {inputState}");
                return false;
        }
    }

    public bool HandleCharacterClicked(CharacterUnit clickedUnit)
    {
        if (clickedUnit == null)
        {
            Debug.LogWarning("Cannot handle character click. Clicked unit is null.");
            return false;
        }

        if (isGameOver)
        {
            Debug.Log("Cannot handle character click. Game is over.");
            return false;
        }

        switch (inputState)
        {
            case GameInputState.SelectingAttackTarget:
                return TrySelectAttackTarget(clickedUnit);

            case GameInputState.WaitingForActivePlayerAction:
                return HandleCharacterClickedDuringNormalAction(clickedUnit);

            case GameInputState.DefenderChoosingDefense:
                Debug.Log("Cannot handle character click. Waiting for defender to choose a defense card.");
                return false;

            case GameInputState.GameOver:
                Debug.Log("Cannot handle character click. Game is over.");
                return false;

            default:
                Debug.Log($"Unhandled input state: {inputState}");
                return false;
        }
    }

    private bool HandleCharacterClickedDuringNormalAction(CharacterUnit clickedUnit)
    {
        // During normal action state, clicking a character behaves like clicking its tile.
        // This is useful if the unit visually blocks the tile collider.
        if (clickedUnit.CurrentTile == null)
        {
            Debug.LogWarning($"{clickedUnit.CharacterName} has no current tile.");
            return false;
        }

        return HandleTileClicked(clickedUnit.CurrentTile);
    }

    public void CancelCurrentSelection()
    {
        if (inputState != GameInputState.SelectingAttackTarget)
        {
            return;
        }

        string cardName = selectedAttackCard != null
            ? selectedAttackCard.CardName
            : "unknown card";

        Debug.Log($"Cancelled attack with {cardName}.");

        selectedAttackCard = null;
        inputState = GameInputState.WaitingForActivePlayerAction;
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
        return CanActiveUnitManeuverToTile(targetTile);
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

        if (inputState != GameInputState.WaitingForActivePlayerAction)
        {
            return $"Cannot maneuver right now. Current input state is {inputState}.";
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

    public bool SelectCardFromActiveHand(CardData selectedCard)
    {
        if (!CanSelectCardFromActiveHand(selectedCard))
        {
            return false;
        }

        if (IsCurrentlySelectingThisAttackCard(selectedCard))
        {
            CancelCurrentSelection();
            return false;
        }

        if (!CanUseCardAsAttack(selectedCard))
        {
            Debug.Log($"{selectedCard.CardName} cannot be used as an attack card right now.");
            return false;
        }

        selectedAttackCard = selectedCard;
        inputState = GameInputState.SelectingAttackTarget;

        Debug.Log($"{ActivePlayerState.PlayerName} selected attack card {selectedCard.CardName}. Choose an enemy target.");

        return true;
    }

    private bool CanSelectCardFromActiveHand(CardData selectedCard)
    {
        if (isGameOver)
        {
            Debug.Log("Cannot select card. Game is over.");
            return false;
        }

        if (selectedCard == null)
        {
            Debug.LogWarning("Cannot select a null card.");
            return false;
        }

        if (ActivePlayerState == null || ActivePlayerState.Deck == null)
        {
            Debug.LogWarning("Cannot select card. Active player or deck is missing.");
            return false;
        }

        if (!IsCardInActivePlayerHand(selectedCard))
        {
            Debug.LogWarning($"{selectedCard.CardName} is not in {ActivePlayerState.PlayerName}'s hand.");
            return false;
        }

        if (IsCurrentlySelectingThisAttackCard(selectedCard))
        {
            return true;
        }

        if (!ActivePlayerState.HasActionsRemaining)
        {
            Debug.Log($"{ActiveUnit.CharacterName} has no actions left.");
            return false;
        }

        if (inputState != GameInputState.WaitingForActivePlayerAction)
        {
            Debug.Log($"Cannot select a new card right now. Current input state is {inputState}.");
            return false;
        }

        return true;
    }

    public bool IsCurrentlySelectingThisAttackCard(CardData card)
    {
        return inputState == GameInputState.SelectingAttackTarget &&
               selectedAttackCard == card;
    }

    private bool IsCardInActivePlayerHand(CardData card)
    {
        if (card == null || ActivePlayerState == null || ActivePlayerState.Deck == null)
        {
            return false;
        }

        foreach (CardData handCard in ActivePlayerState.Deck.Hand)
        {
            if (handCard == card)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanUseCardAsAttack(CardData card)
    {
        return card != null &&
               (card.CardType == CardType.Attack || card.CardType == CardType.Versatile);
    }

    private bool TrySelectAttackTarget(Tile targetTile)
    {
        if (targetTile == null)
        {
            Debug.LogWarning("Cannot select attack target. Target tile is null.");
            return false;
        }

        if (targetTile.OccupyingUnit == null)
        {
            Debug.Log("Attack target must be an enemy character.");
            return false;
        }

        return TrySelectAttackTarget(targetTile.OccupyingUnit);
    }

    private bool TrySelectAttackTarget(CharacterUnit targetUnit)
    {
        string validationError = GetAttackTargetValidationError(targetUnit);

        if (validationError != null)
        {
            Debug.Log(validationError);
            return false;
        }

        return ResolveAttackWithoutDefenseForNow(targetUnit);
    }

    private string GetAttackTargetValidationError(CharacterUnit targetUnit)
    {
        if (selectedAttackCard == null)
        {
            inputState = GameInputState.WaitingForActivePlayerAction;
            return "Cannot select attack target. No attack card is selected.";
        }

        if (targetUnit == null)
        {
            return "Cannot select attack target. Target unit is null.";
        }

        if (targetUnit == ActiveUnit)
        {
            return "You cannot attack your own active unit.";
        }

        if (targetUnit != EnemyUnit)
        {
            return "For now, you can only attack the enemy unit.";
        }

        if (targetUnit.CurrentTile == null)
        {
            return $"{targetUnit.CharacterName} has no current tile.";
        }

        if (!IsUnitInActiveUnitAttackRange(targetUnit))
        {
            return $"{targetUnit.CharacterName} is out of range.";
        }

        return null;
    }

    private bool ResolveAttackWithoutDefenseForNow(CharacterUnit targetUnit)
    {
        if (targetUnit == null)
        {
            Debug.LogWarning("Cannot resolve attack. Target unit is null.");
            return false;
        }

        if (selectedAttackCard == null)
        {
            Debug.LogWarning("Cannot resolve attack. No attack card is selected.");
            return false;
        }

        CardData attackCard = selectedAttackCard;

        bool removedFromHand = ActivePlayerState.Deck.RemoveCardFromHand(attackCard);

        if (!removedFromHand)
        {
            Debug.LogWarning($"Could not play {attackCard.CardName}. It was not found in hand.");
            return false;
        }

        ActivePlayerState.Deck.AddCardToDiscardPile(attackCard);

        targetUnit.TakeDamage(attackCard.Value);

        Debug.Log(
            $"{ActiveUnit.CharacterName} attacked {targetUnit.CharacterName} with {attackCard.CardName} " +
            $"for {attackCard.Value} damage. Defense is not implemented yet."
        );

        selectedAttackCard = null;

        CheckGameOver();

        if (isGameOver)
        {
            return true;
        }

        inputState = GameInputState.WaitingForActivePlayerAction;
        ConsumeAction();

        return true;
    }

    private bool IsUnitInActiveUnitAttackRange(CharacterUnit targetUnit)
    {
        if (ActiveUnit == null || ActiveUnit.CurrentTile == null || targetUnit == null || targetUnit.CurrentTile == null)
        {
            return false;
        }

        Vector2Int activePosition = ActiveUnit.CurrentTile.GridPosition;
        Vector2Int targetPosition = targetUnit.CurrentTile.GridPosition;

        int distance = Mathf.Abs(activePosition.x - targetPosition.x) +
                       Mathf.Abs(activePosition.y - targetPosition.y);

        return distance <= ActiveUnit.CurrentAttackRange;
    }

    public bool TryBasicAttack()
    {
        if (isGameOver)
        {
            Debug.Log("Cannot attack. Game is over.");
            return false;
        }

        if (inputState != GameInputState.WaitingForActivePlayerAction)
        {
            Debug.Log($"Cannot basic attack right now. Current input state is {inputState}.");
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
        return IsUnitInActiveUnitAttackRange(EnemyUnit);
    }

    private void CheckGameOver()
    {
        if (playerOne.Unit.IsDead)
        {
            EndGame("Player Two");
            return;
        }

        if (playerTwo.Unit.IsDead)
        {
            EndGame("Player One");
        }
    }

    private void EndGame(string winningPlayerName)
    {
        isGameOver = true;
        inputState = GameInputState.GameOver;
        selectedAttackCard = null;

        Debug.Log($"Game over. {winningPlayerName} wins!");
    }
}