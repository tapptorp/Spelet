using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI playerOneInfoText;
    [SerializeField] private TextMeshProUGUI playerTwoInfoText;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void Update()
    {
        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (gameManager == null)
        {
            return;
        }

        UpdateSinglePlayerInfo(
            playerOneInfoText,
            "Player 1",
            gameManager.PlayerOneUnit,
            gameManager.ActivePlayer == PlayerTurn.PlayerOne
        );

        UpdateSinglePlayerInfo(
            playerTwoInfoText,
            "Player 2",
            gameManager.PlayerTwoUnit,
            gameManager.ActivePlayer == PlayerTurn.PlayerTwo
        );
    }

    private void UpdateSinglePlayerInfo(TextMeshProUGUI textElement, string playerLabel, CharacterUnit unit, bool isActivePlayer)
    {
        if (textElement == null)
        {
            return;
        }

        if (unit == null)
        {
            textElement.text = $"{playerLabel}\nNo unit assigned";
            return;
        }

        string activeMarker = isActivePlayer ? "ACTIVE" : "";
        string actionsText = isActivePlayer ? $"Actions: {gameManager.ActionsRemainingThisTurn}" : "Actions: -";

        textElement.text =
            $"{playerLabel} {activeMarker}\n" +
            $"Name: {unit.CharacterName}\n" +
            $"Health: {unit.CurrentHealth} / {unit.MaxHealth}\n" +
            $"Range: {unit.CurrentAttackRange}\n" +
            $"Movement: {unit.Movement}\n" +
            actionsText;
    }
}