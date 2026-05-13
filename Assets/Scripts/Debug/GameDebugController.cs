using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebugController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private CharacterUnit unit;

    [Header("Test Toggles")]
    [SerializeField] private bool testUnitMovement;
    [SerializeField] private bool testBoardManager;

    [Header("Test Positions")]
    [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int otherPosition = new Vector2Int(1, 0);

    [Header("Runtime Debug Controls")]
    [SerializeField] private bool enableHotkeys = true;
    [SerializeField] private CharacterUnit controlledUnit;

    //Temporärt före Gamelogic script/gamelogic manager
    [Header("Start Placement")]
    [SerializeField] private bool placeUnitOnStart = true;
    [SerializeField] private Vector2Int unitStartPosition = new Vector2Int(0, 0);

    private void Start()
    {
        if (testUnitMovement)
        {
            TestUnitMovement();
        }

        if (testBoardManager)
        {
            TestBoardManager();
        }

        //Temporärt före Gamelogic script/gamelogic manager
        if (placeUnitOnStart && controlledUnit != null && boardManager != null)
        {
            Tile startTile = boardManager.GetTileAtPosition(unitStartPosition);

            if (startTile != null)
            {
                controlledUnit.PlaceOnTile(startTile);
            }
            else
            {
                Debug.LogWarning($"No tile found at start position {unitStartPosition}.");
            }
        }
    }

    private void TestUnitMovement()
    {
        Debug.Log("=== UNIT MOVEMENT TEST ===");

        Tile startTile = boardManager.GetTileAtPosition(startPosition);
        Tile otherTile = boardManager.GetTileAtPosition(otherPosition);

        if (startTile == null || otherTile == null)
        {
            Debug.LogWarning("Movement test failed: one or both test tiles were not found.");
            return;
        }

        unit.PlaceOnTile(startTile);

        Debug.Log($"Unit placed on: {unit.CurrentTile.name}");
        Debug.Log($"Start tile occupied: {startTile.IsOccupied}");

        unit.PlaceOnTile(otherTile);

        Debug.Log($"Unit moved to: {unit.CurrentTile.name}");
        Debug.Log($"Old tile occupied: {startTile.IsOccupied}");
        Debug.Log($"New tile occupied: {otherTile.IsOccupied}");

        Debug.Log("=== UNIT MOVEMENT TEST END ===");
    }

    private void TestBoardManager()
    {
        Debug.Log("=== BOARD MANAGER TEST ===");

        Tile tile = boardManager.GetTileAtPosition(startPosition);

        Debug.Log($"Tile at {startPosition}: {tile}");

        bool hasTile = boardManager.HasTileAtPosition(startPosition);
        Debug.Log($"Has tile at {startPosition}: {hasTile}");

        bool isOccupiedBefore = boardManager.IsTileOccupied(startPosition);
        Debug.Log($"Is tile occupied before placing unit: {isOccupiedBefore}");

        if (tile != null)
        {
            unit.PlaceOnTile(tile);
        }

        bool isOccupiedAfter = boardManager.IsTileOccupied(startPosition);
        Debug.Log($"Is tile occupied after placing unit: {isOccupiedAfter}");

        Debug.Log("=== BOARD MANAGER TEST END ===");
    }

    private void Update()
    {
        if (!enableHotkeys || controlledUnit == null || boardManager == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            TryMoveControlledUnit(Vector2Int.up);
        }

        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            TryMoveControlledUnit(Vector2Int.down);
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            TryMoveControlledUnit(Vector2Int.left);
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            TryMoveControlledUnit(Vector2Int.right);
        }
    }

    private void TryMoveControlledUnit(Vector2Int direction)
    {
        if (controlledUnit.CurrentTile == null)
        {
            Debug.LogWarning("Controlled unit has no current tile.");
            return;
        }

        Vector2Int currentPosition = controlledUnit.CurrentTile.GridPosition;
        Vector2Int targetPosition = currentPosition + direction;

        Tile targetTile = boardManager.GetTileAtPosition(targetPosition);

        if (targetTile == null)
        {
            Debug.Log($"No tile found at {targetPosition}.");
            return;
        }

        if (targetTile.IsOccupied)
        {
            Debug.Log($"Tile at {targetPosition} is occupied.");
            return;
        }

        controlledUnit.PlaceOnTile(targetTile);

        Debug.Log($"Moved unit to {targetPosition}.");
    }
}