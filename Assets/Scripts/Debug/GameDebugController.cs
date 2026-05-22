using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebugController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Player One Debug Movement Targets")]
    [SerializeField] private Vector2Int playerOneTargetOne = new Vector2Int(-1, 0);
    [SerializeField] private Vector2Int playerOneTargetTwo = new Vector2Int(-2, 0);
    [SerializeField] private Vector2Int playerOneTargetThree = new Vector2Int(-3, 0);

    [Header("Player Two Debug Movement Targets")]
    [SerializeField] private Vector2Int playerTwoTargetOne = new Vector2Int(-4, 5);
    [SerializeField] private Vector2Int playerTwoTargetTwo = new Vector2Int(-3, 5);
    [SerializeField] private Vector2Int playerTwoTargetThree = new Vector2Int(-2, 5);

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            TryDebugMove(GetTargetOne());
        }

        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            TryDebugMove(GetTargetTwo());
        }

        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            TryDebugMove(GetTargetThree());
        }

        if (keyboard.aKey.wasPressedThisFrame)
        {
            gameManager.TryBasicAttack();
        }

        /*if (keyboard.spaceKey.wasPressedThisFrame)
        {
            LogGameState();
        }
        */
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (gameManager != null)
            {
                gameManager.SkipDefense();
            }
        }
    }

    private Vector2Int GetTargetOne()
    {
        return gameManager.ActivePlayer == PlayerTurn.PlayerOne
            ? playerOneTargetOne
            : playerTwoTargetOne;
    }

    private Vector2Int GetTargetTwo()
    {
        return gameManager.ActivePlayer == PlayerTurn.PlayerOne
            ? playerOneTargetTwo
            : playerTwoTargetTwo;
    }

    private Vector2Int GetTargetThree()
    {
        return gameManager.ActivePlayer == PlayerTurn.PlayerOne
            ? playerOneTargetThree
            : playerTwoTargetThree;
    }

    private void TryDebugMove(Vector2Int targetPosition)
    {
        bool moved = gameManager.TryMoveActiveUnitToPosition(targetPosition);

        if (!moved)
        {
            Debug.Log($"Debug move to {targetPosition} failed.");
        }
    }

    private void LogGameState()
    {
        CharacterUnit activeUnit = gameManager.ActiveUnit;
        CharacterUnit enemyUnit = gameManager.EnemyUnit;

        Debug.Log(
            $"Active player: {gameManager.ActivePlayer}\n" +
            $"Active unit: {activeUnit.CharacterName}\n" +
            $"Active unit tile: {activeUnit.CurrentTile?.GridPosition.ToString() ?? "None"}\n" +
            $"Active unit HP: {activeUnit.CurrentHealth}\n" +
            $"Enemy unit: {enemyUnit.CharacterName}\n" +
            $"Enemy unit tile: {enemyUnit.CurrentTile?.GridPosition.ToString() ?? "None"}\n" +
            $"Enemy unit HP: {enemyUnit.CurrentHealth}\n" +
            $"Actions left: {gameManager.ActionsRemainingThisTurn}\n" +
            $"Game over: {gameManager.IsGameOver}"
        );
    }
}