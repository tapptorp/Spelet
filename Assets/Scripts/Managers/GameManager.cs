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
    [SerializeField] private int actionsRemainingThisTurn;
    [SerializeField] private bool isGameOver;

    public PlayerTurn ActivePlayer => activePlayer;

    public CharacterUnit ActiveUnit => activePlayer == PlayerTurn.PlayerOne
        ? playerOneUnit
        : playerTwoUnit;

    public CharacterUnit EnemyUnit => activePlayer == PlayerTurn.PlayerOne
        ? playerTwoUnit
        : playerOneUnit;

    public int ActionsRemainingThisTurn => actionsRemainingThisTurn;
    public bool IsGameOver => isGameOver;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        if (!CanStartGame())
        {
            Debug.LogWarning("Game could not start. Missing one or more required references.");
            return;
        }

        bool playerOnePlaced = PlaceUnitAtStartPosition(playerOneUnit, playerOneStartPosition);
        bool playerTwoPlaced = PlaceUnitAtStartPosition(playerTwoUnit, playerTwoStartPosition);

        if (!playerOnePlaced || !playerTwoPlaced)
        {
            Debug.LogWarning("Game could not start. One or more units could not be placed.");
            return;
        }

        activePlayer = PlayerTurn.PlayerOne;
        isGameOver = false;

        StartTurn();
    }

    private bool CanStartGame()
    {
        return boardManager != null &&
               playerOneUnit != null &&
               playerTwoUnit != null;
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

        actionsRemainingThisTurn = actionsPerTurn;

        Debug.Log($"Starting turn for {activePlayer}. Active unit: {ActiveUnit.CharacterName}. Actions: {actionsRemainingThisTurn}");
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
        actionsRemainingThisTurn--;

        if (actionsRemainingThisTurn <= 0)
        {
            Debug.Log($"{ActiveUnit.CharacterName} has used all actions.");
            SwitchTurn();
        }
    }

    public bool TryMoveActiveUnitToPosition(Vector2Int targetPosition)
    {
        Tile targetTile = boardManager.GetTileAtPosition(targetPosition);

        if (targetTile == null)
        {
            Debug.Log($"No tile found at {targetPosition}.");
            return false;
        }

        return TryMoveActiveUnitToTile(targetTile);
    }

    public bool TryMoveActiveUnitToTile(Tile targetTile)
    {
        string validationError = GetMoveValidationError(targetTile);

        if (validationError != null)
        {
            Debug.Log(validationError);
            return false;
        }

        ActiveUnit.PlaceOnTile(targetTile);

        Debug.Log($"{ActiveUnit.CharacterName} moved to {targetTile.GridPosition}.");

        ConsumeAction();

        return true;
    }

    public bool CanActiveUnitMoveToTile(Tile targetTile)
    {
        return GetMoveValidationError(targetTile) == null;
    }

    private string GetMoveValidationError(Tile targetTile)
    {
        if (isGameOver)
        {
            return "Cannot move. Game is over.";
        }

        if (actionsRemainingThisTurn <= 0)
        {
            return $"{ActiveUnit.CharacterName} has no actions left.";
        }

        if (ActiveUnit.CurrentTile == null)
        {
            return $"{ActiveUnit.CharacterName} has no current tile.";
        }

        if (targetTile == null)
        {
            return "Cannot move. Target tile is null.";
        }

        if (targetTile.IsOccupied)
        {
            return $"Tile at {targetTile.GridPosition} is occupied.";
        }

        if (!IsTileWithinMovementRange(targetTile))
        {
            return $"{targetTile.GridPosition} is outside movement range.";
        }

        return null;
    }

    private bool IsTileWithinMovementRange(Tile targetTile)
    {
        Vector2Int activePosition = ActiveUnit.CurrentTile.GridPosition;
        Vector2Int targetPosition = targetTile.GridPosition;

        int distance = Mathf.Abs(activePosition.x - targetPosition.x) +
                       Mathf.Abs(activePosition.y - targetPosition.y);

        return distance <= ActiveUnit.Movement;
    }

    public bool TryBasicAttack()
    {
        if (isGameOver)
        {
            Debug.Log("Cannot attack. Game is over.");
            return false;
        }

        if (actionsRemainingThisTurn <= 0)
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
        if (playerOneUnit.IsDead)
        {
            isGameOver = true;
            Debug.Log("Game over. Player Two wins!");
            return;
        }

        if (playerTwoUnit.IsDead)
        {
            isGameOver = true;
            Debug.Log("Game over. Player One wins!");
        }
    }
}