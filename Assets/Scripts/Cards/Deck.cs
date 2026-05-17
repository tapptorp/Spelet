using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck
{
    [SerializeField] private List<CardData> drawPile = new List<CardData>();
    [SerializeField] private List<CardData> hand = new List<CardData>();
    [SerializeField] private List<CardData> discardPile = new List<CardData>();

    public IReadOnlyList<CardData> DrawPile => drawPile;
    public IReadOnlyList<CardData> Hand => hand;
    public IReadOnlyList<CardData> DiscardPile => discardPile;

    public int DrawPileCount => drawPile.Count;
    public int HandCount => hand.Count;
    public int DiscardPileCount => discardPile.Count;

    public Deck(IEnumerable<CardData> startingCards)
    {
        drawPile = startingCards != null
            ? new List<CardData>(startingCards)
            : new List<CardData>();

        hand = new List<CardData>();
        discardPile = new List<CardData>();

        ShuffleDrawPile();
    }

    public bool CanDrawCard()
    {
        return drawPile.Count > 0;
    }

    public CardData DrawCard()
    {
        if (!CanDrawCard())
        {
            // If this returns null, the caller decides what happens.
            // For example, GameManager may apply fatigue damage.
            Debug.LogWarning("Cannot draw card. Draw pile is empty.");
            return null;
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);

        return drawnCard;
    }

    public void AddCardToDiscardPile(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot discard a null card.");
            return;
        }

        discardPile.Add(card);
    }

    public bool RemoveCardFromHand(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot remove a null card from hand.");
            return false;
        }

        return hand.Remove(card);
    }

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex = Random.Range(i, drawPile.Count);

            CardData temporaryCard = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temporaryCard;
        }
    }
}