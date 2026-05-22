using UnityEngine;

public class CombatResolutionResult
{
    public PlayerState AttackingPlayer { get; private set; }
    public PlayerState DefendingPlayer { get; private set; }

    public CharacterUnit Attacker { get; private set; }
    public CharacterUnit Defender { get; private set; }

    public CardData AttackCard { get; private set; }
    public CardData DefenseCard { get; private set; }

    public int AttackValue { get; private set; }
    public int DefenseValue { get; private set; }
    public int DamageDealt { get; private set; }

    public CombatResolutionResult(
        PlayerState attackingPlayer,
        PlayerState defendingPlayer,
        CharacterUnit attacker,
        CharacterUnit defender,
        CardData attackCard,
        CardData defenseCard,
        int attackValue,
        int defenseValue,
        int damageDealt)
    {
        AttackingPlayer = attackingPlayer;
        DefendingPlayer = defendingPlayer;
        Attacker = attacker;
        Defender = defender;
        AttackCard = attackCard;
        DefenseCard = defenseCard;
        AttackValue = attackValue;
        DefenseValue = defenseValue;
        DamageDealt = damageDealt;
    }
}

public class CombatManager : MonoBehaviour
{
    private CardData selectedAttackCard;
    private PlayerState attackingPlayerState;
    private PlayerState defendingPlayerState;
    private CharacterUnit pendingAttackTarget;

    public CardData SelectedAttackCard => selectedAttackCard;
    public PlayerState AttackingPlayerState => attackingPlayerState;
    public PlayerState DefendingPlayerState => defendingPlayerState;
    public CharacterUnit PendingAttackTarget => pendingAttackTarget;

    public bool HasSelectedAttackCard => selectedAttackCard != null;

    public bool BeginAttackSelection(PlayerState attackerPlayerState, CardData attackCard)
    {
        if (attackerPlayerState == null)
        {
            Debug.LogWarning("Cannot begin attack selection. Attacking player is missing.");
            return false;
        }

        if (attackerPlayerState.Unit == null)
        {
            Debug.LogWarning("Cannot begin attack selection. Attacking unit is missing.");
            return false;
        }

        if (attackCard == null)
        {
            Debug.LogWarning("Cannot begin attack selection. Attack card is null.");
            return false;
        }

        if (attackerPlayerState.Deck == null)
        {
            Debug.LogWarning("Cannot begin attack selection. Attacking player's deck is missing.");
            return false;
        }

        if (!IsCardInPlayerHand(attackerPlayerState, attackCard))
        {
            Debug.LogWarning($"{attackCard.CardName} is not in {attackerPlayerState.PlayerName}'s hand.");
            return false;
        }

        if (!CanUseCardAsAttack(attackCard))
        {
            Debug.Log($"{attackCard.CardName} cannot be used as an attack card right now.");
            return false;
        }

        selectedAttackCard = attackCard;
        attackingPlayerState = attackerPlayerState;
        defendingPlayerState = null;
        pendingAttackTarget = null;

        Debug.Log($"{attackingPlayerState.PlayerName} selected attack card {attackCard.CardName}. Choose an enemy target.");

        return true;
    }

    public bool BeginDefenseResponse(PlayerState defenderPlayerState, CharacterUnit targetUnit)
    {
        if (selectedAttackCard == null)
        {
            Debug.LogWarning("Cannot begin defense response. No attack card is selected.");
            return false;
        }

        if (attackingPlayerState == null)
        {
            Debug.LogWarning("Cannot begin defense response. Attacking player is missing.");
            return false;
        }

        if (targetUnit == null)
        {
            Debug.LogWarning("Cannot begin defense response. Target unit is null.");
            return false;
        }

        if (defenderPlayerState == null)
        {
            Debug.LogWarning("Cannot begin defense response. Defending player is missing.");
            return false;
        }

        pendingAttackTarget = targetUnit;
        defendingPlayerState = defenderPlayerState;

        Debug.Log(
            $"{defendingPlayerState.PlayerName} is being attacked. " +
            $"Choose a Defense/Versatile card, or press Space to skip defense. " +
            $"The attack card is hidden until defense is chosen."
        );

        return true;
    }

    public bool SelectDefenseCardFromDefenderHand(CardData selectedCard, out CombatResolutionResult result)
    {
        result = null;

        if (selectedCard == null)
        {
            Debug.LogWarning("Cannot select a null defense card.");
            return false;
        }

        if (defendingPlayerState == null || defendingPlayerState.Deck == null)
        {
            Debug.LogWarning("Cannot select defense card. Defending player or deck is missing.");
            return false;
        }

        if (!IsCardInPlayerHand(defendingPlayerState, selectedCard))
        {
            Debug.LogWarning($"{selectedCard.CardName} is not in {defendingPlayerState.PlayerName}'s hand.");
            return false;
        }

        if (!CanUseCardAsDefense(selectedCard))
        {
            Debug.Log($"{selectedCard.CardName} cannot be used as a defense card.");
            return false;
        }

        return ResolveAttackWithDefense(selectedCard, out result);
    }

    public bool SkipDefense(out CombatResolutionResult result)
    {
        return ResolveAttackWithDefense(null, out result);
    }

    public bool IsCurrentlySelectingThisAttackCard(CardData card)
    {
        return selectedAttackCard == card && selectedAttackCard != null;
    }

    public bool CanUseCardAsAttack(CardData card)
    {
        return card != null &&
               (card.CardType == CardType.Attack || card.CardType == CardType.Versatile);
    }

    public bool CanUseCardAsDefense(CardData card)
    {
        return card != null &&
               (card.CardType == CardType.Defense || card.CardType == CardType.Versatile);
    }

    public void ClearCombatState()
    {
        selectedAttackCard = null;
        attackingPlayerState = null;
        defendingPlayerState = null;
        pendingAttackTarget = null;
    }

    private bool ResolveAttackWithDefense(CardData defenseCard, out CombatResolutionResult result)
    {
        result = null;

        if (selectedAttackCard == null)
        {
            Debug.LogWarning("Cannot resolve combat. No attack card is selected.");
            return false;
        }

        if (attackingPlayerState == null || attackingPlayerState.Deck == null)
        {
            Debug.LogWarning("Cannot resolve combat. Attacking player or deck is missing.");
            return false;
        }

        if (attackingPlayerState.Unit == null)
        {
            Debug.LogWarning("Cannot resolve combat. Attacking unit is missing.");
            return false;
        }

        if (pendingAttackTarget == null)
        {
            Debug.LogWarning("Cannot resolve combat. No attack target is stored.");
            return false;
        }

        if (defendingPlayerState == null || defendingPlayerState.Deck == null)
        {
            Debug.LogWarning("Cannot resolve combat. Defending player or deck is missing.");
            return false;
        }

        if (!IsCardInPlayerHand(attackingPlayerState, selectedAttackCard))
        {
            Debug.LogWarning($"Could not play {selectedAttackCard.CardName}. It was not found in attacker's hand.");
            return false;
        }

        if (defenseCard != null && !IsCardInPlayerHand(defendingPlayerState, defenseCard))
        {
            Debug.LogWarning($"Could not play {defenseCard.CardName}. It was not found in defender's hand.");
            return false;
        }

        CardData attackCard = selectedAttackCard;
        PlayerState resolvedAttackingPlayer = attackingPlayerState;
        PlayerState resolvedDefendingPlayer = defendingPlayerState;
        CharacterUnit attacker = attackingPlayerState.Unit;
        CharacterUnit defender = pendingAttackTarget;

        bool removedAttackCard = resolvedAttackingPlayer.Deck.RemoveCardFromHand(attackCard);

        if (!removedAttackCard)
        {
            Debug.LogWarning($"Could not play {attackCard.CardName}. It was not found in attacker's hand.");
            return false;
        }

        resolvedAttackingPlayer.Deck.AddCardToDiscardPile(attackCard);

        int defenseValue = 0;

        if (defenseCard != null)
        {
            bool removedDefenseCard = resolvedDefendingPlayer.Deck.RemoveCardFromHand(defenseCard);

            if (!removedDefenseCard)
            {
                Debug.LogWarning($"Could not play {defenseCard.CardName}. It was not found in defender's hand.");
                return false;
            }

            resolvedDefendingPlayer.Deck.AddCardToDiscardPile(defenseCard);
            defenseValue = defenseCard.Value;
        }

        int attackValue = attackCard.Value;
        int damage = Mathf.Max(attackValue - defenseValue, 0);

        defender.TakeDamage(damage);

        string defenseText = defenseCard != null
            ? $"{defenseCard.CardName} ({defenseCard.Value})"
            : "no defense";

        Debug.Log(
            $"Combat revealed: {attacker.CharacterName} attacked {defender.CharacterName} with " +
            $"{attackCard.CardName} ({attackCard.Value}). Defender used {defenseText}. " +
            $"Damage dealt: {damage}."
        );

        result = new CombatResolutionResult(
            resolvedAttackingPlayer,
            resolvedDefendingPlayer,
            attacker,
            defender,
            attackCard,
            defenseCard,
            attackValue,
            defenseValue,
            damage
        );

        ClearCombatState();

        return true;
    }

    private bool IsCardInPlayerHand(PlayerState playerState, CardData card)
    {
        if (card == null || playerState == null || playerState.Deck == null)
        {
            return false;
        }

        foreach (CardData handCard in playerState.Deck.Hand)
        {
            if (handCard == card)
            {
                return true;
            }
        }

        return false;
    }
}


