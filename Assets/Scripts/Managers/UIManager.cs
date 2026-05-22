using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI playerOneInfoText;
    [SerializeField] private TextMeshProUGUI playerTwoInfoText;

    [Header("Active Hand UI")]
    [SerializeField] private TextMeshProUGUI activeHandTitleText;
    [SerializeField] private Transform activeHandButtonContainer;
    [SerializeField] private Button cardButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedCardInfoText;

    private string lastHandSignature = "";

    private bool hasKnownActivePlayer = false;
    private PlayerTurn lastKnownActivePlayer;

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
        ClearSelectedCardInfoIfActivePlayerChanged();
        UpdateActivePlayerHandUI();
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
            gameManager.PlayerOne,
            gameManager.PlayerOneUnit,
            gameManager.ActivePlayer == PlayerTurn.PlayerOne
        );

        UpdateSinglePlayerInfo(
            playerTwoInfoText,
            "Player 2",
            gameManager.PlayerTwo,
            gameManager.PlayerTwoUnit,
            gameManager.ActivePlayer == PlayerTurn.PlayerTwo
        );
    }

    private void UpdateSinglePlayerInfo(
        TextMeshProUGUI textElement,
        string playerLabel,
        PlayerState playerState,
        CharacterUnit unit,
        bool isActivePlayer)
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

        string deckText = "Deck not created yet";

        if (playerState != null && playerState.Deck != null)
        {
            deckText =
                $"Draw: {playerState.Deck.DrawPileCount}\n" +
                $"Hand: {playerState.Deck.HandCount}\n" +
                $"Discard: {playerState.Deck.DiscardPileCount}";
        }

        textElement.text =
            $"{playerLabel} {activeMarker}\n" +
            $"Name: {unit.CharacterName}\n" +
            $"Health: {unit.CurrentHealth} / {unit.MaxHealth}\n" +
            $"Range: {unit.CurrentAttackRange}\n" +
            $"Movement: {unit.Movement}\n" +
            $"{actionsText}\n" +
            $"{deckText}";
    }

    private void UpdateActivePlayerHandUI()
    {
        if (gameManager == null)
        {
            return;
        }

        PlayerState displayedHandOwner = gameManager.DisplayedHandOwner;

        if (activeHandTitleText != null)
        {
            if (displayedHandOwner == null)
            {
                activeHandTitleText.text = "Hand";
            }
            else if (gameManager.InputState == GameInputState.DefenderChoosingDefense)
            {
                activeHandTitleText.text = $"{displayedHandOwner.PlayerName} Defense Hand";
            }
            else
            {
                activeHandTitleText.text = $"{displayedHandOwner.PlayerName} Hand";
            }
        }

        if (activeHandButtonContainer == null || cardButtonPrefab == null)
        {
            return;
        }

        string currentSignature = BuildHandSignature(displayedHandOwner);

        if (currentSignature == lastHandSignature)
        {
            return;
        }

        lastHandSignature = currentSignature;
        RebuildActiveHandButtons(displayedHandOwner);
    }

    private string BuildHandSignature(PlayerState playerState)
    {
        if (gameManager == null || playerState == null || playerState.Deck == null)
        {
            return "No hand";
        }

        StringBuilder builder = new StringBuilder();

        builder.Append(gameManager.ActivePlayer);
        builder.Append("|");
        builder.Append(gameManager.InputState);
        builder.Append("|");
        builder.Append(playerState.PlayerName);
        builder.Append("|");

        for (int i = 0; i < playerState.Deck.Hand.Count; i++)
        {
            CardData card = playerState.Deck.Hand[i];

            builder.Append(i);
            builder.Append(":");

            if (card == null)
            {
                builder.Append("null");
            }
            else
            {
                builder.Append(card.CardName);
                builder.Append("-");
                builder.Append(card.CardType);
                builder.Append("-");
                builder.Append(card.Value);
            }

            builder.Append("|");
        }

        return builder.ToString();
    }


    private void RebuildActiveHandButtons(PlayerState activePlayerState)
    {
        ClearActiveHandButtons();

        if (activePlayerState == null || activePlayerState.Deck == null)
        {
            return;
        }

        foreach (CardData card in activePlayerState.Deck.Hand)
        {
            CreateCardButton(card);
        }
    }

    private void ClearActiveHandButtons()
    {
        for (int i = activeHandButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(activeHandButtonContainer.GetChild(i).gameObject);
        }
    }

    private void CreateCardButton(CardData card)
    {
        Button button = Instantiate(cardButtonPrefab, activeHandButtonContainer);

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            if (card == null)
            {
                buttonText.text = "Missing Card";
            }
            else
            {
                buttonText.text =
                    $"{card.CardName}\n" +
                    $"{card.CardType} {card.Value}";
            }
        }

        button.onClick.AddListener(() => OnCardButtonClicked(card));
    }

    private void ClearSelectedCardInfoIfActivePlayerChanged()
    {
        if (gameManager == null)
        {
            return;
        }

        if (!hasKnownActivePlayer)
        {
            lastKnownActivePlayer = gameManager.ActivePlayer;
            hasKnownActivePlayer = true;
            return;
        }

        if (gameManager.ActivePlayer != lastKnownActivePlayer)
        {
            ClearSelectedCardInfo();
            lastKnownActivePlayer = gameManager.ActivePlayer;

            // Forces hand buttons to rebuild when player changes.
            lastHandSignature = "";
        }
    }

    public void ClearSelectedCardInfo()
    {
        if (selectedCardInfoText != null)
        {
            selectedCardInfoText.text = "";
        }
    }

    private void OnCardButtonClicked(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("Clicked a missing card.");
            ClearSelectedCardInfo();
            return;
        }

        if (gameManager == null)
        {
            Debug.LogWarning("Cannot select card. UIManager is missing GameManager reference.");
            ClearSelectedCardInfo();
            return;
        }

        bool selectionAccepted = gameManager.SelectCardFromVisibleHand(card);

        if (!selectionAccepted)
        {
            ClearSelectedCardInfo();
            return;
        }

        if (selectedCardInfoText != null)
        {
            selectedCardInfoText.text =
                $"Selected:\n" +
                $"{card.CardName}\n" +
                $"Type: {card.CardType}\n" +
                $"Value: {card.Value}\n" +
                $"{card.Description}";
        }
    }
}