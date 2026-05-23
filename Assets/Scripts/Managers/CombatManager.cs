using UnityEngine;

public enum CombatWinner
{
    Attacker,
    Defender
}

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
    public CombatWinner Winner { get; private set; }

    public CombatResolutionResult(
        PlayerState attackingPlayer,
        PlayerState defendingPlayer,
        CharacterUnit attacker,
        CharacterUnit defender,
        CardData attackCard,
        CardData defenseCard,
        int attackValue,
        int defenseValue,
        int damageDealt,
        CombatWinner winner)
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
        Winner = winner;
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

        if (!CanResolveCombat(defenseCard))
        {
            return false;
        }

        CardData attackCard = selectedAttackCard;
        PlayerState resolvedAttackingPlayer = attackingPlayerState;
        PlayerState resolvedDefendingPlayer = defendingPlayerState;
        CharacterUnit attacker = resolvedAttackingPlayer.Unit;
        CharacterUnit defender = pendingAttackTarget;

        bool removedAttackCard = resolvedAttackingPlayer.Deck.RemoveCardFromHand(attackCard);

        if (!removedAttackCard)
        {
            Debug.LogWarning($"Could not play {attackCard.CardName}. It was not found in attacker's hand.");
            return false;
        }

        if (defenseCard != null)
        {
            bool removedDefenseCard = resolvedDefendingPlayer.Deck.RemoveCardFromHand(defenseCard);

            if (!removedDefenseCard)
            {
                // This should be unreachable because CanResolveCombat already checked the hand.
                Debug.LogWarning($"Could not play {defenseCard.CardName}. It was not found in defender's hand.");
                return false;
            }
        }

        CombatContext context = new CombatContext(
            resolvedAttackingPlayer,
            resolvedDefendingPlayer,
            attacker,
            defender,
            attackCard,
            defenseCard
        );

        Debug.Log(BuildRevealText(context));

        // Unmatched-like order: defender first, then attacker.
        ResolveEffects(defenseCard, CardEffectTiming.Immediately, context, true);
        ResolveEffects(attackCard, CardEffectTiming.Immediately, context, false);

        ResolveEffects(defenseCard, CardEffectTiming.DuringCombat, context, true);
        ResolveEffects(attackCard, CardEffectTiming.DuringCombat, context, false);

        context.Winner = context.AttackValue > context.DefenseValue
            ? CombatWinner.Attacker
            : CombatWinner.Defender;

        int damage = Mathf.Max(context.AttackValue - context.DefenseValue, 0);
        defender.TakeDamage(damage);

        Debug.Log(
            $"Combat values after during-combat effects: " +
            $"Attack {context.AttackValue}, Defense {context.DefenseValue}. " +
            $"Winner: {context.Winner}. Damage dealt: {damage}."
        );

        ResolveEffects(defenseCard, CardEffectTiming.AfterCombat, context, true);
        ResolveEffects(attackCard, CardEffectTiming.AfterCombat, context, false);

        resolvedAttackingPlayer.Deck.AddCardToDiscardPile(attackCard);

        if (defenseCard != null)
        {
            resolvedDefendingPlayer.Deck.AddCardToDiscardPile(defenseCard);
        }

        result = new CombatResolutionResult(
            resolvedAttackingPlayer,
            resolvedDefendingPlayer,
            attacker,
            defender,
            attackCard,
            defenseCard,
            context.AttackValue,
            context.DefenseValue,
            damage,
            context.Winner
        );

        ClearCombatState();

        return true;
    }

    private bool CanResolveCombat(CardData defenseCard)
    {
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

        return true;
    }

    private void ResolveEffects(CardData card, CardEffectTiming timing, CombatContext context, bool cardBelongsToDefender)
    {
        if (card == null || card.Effects == null)
        {
            return;
        }

        foreach (CardEffectData effect in card.Effects)
        {
            if (effect == null || effect.Timing != timing)
            {
                continue;
            }

            ApplyEffect(effect, context, cardBelongsToDefender, card);
        }
    }

    private void ApplyEffect(CardEffectData effect, CombatContext context, bool cardBelongsToDefender, CardData sourceCard)
    {
        if (effect.Value <= 0)
        {
            Debug.LogWarning($"{sourceCard.CardName} has an effect with value <= 0. Skipping effect.");
            return;
        }

        PlayerState targetPlayer = GetTargetPlayer(effect.Target, context, cardBelongsToDefender);
        CharacterUnit targetUnit = targetPlayer != null ? targetPlayer.Unit : null;

        switch (effect.EffectType)
        {
            case CardEffectType.DrawCards:
                DrawCards(targetPlayer, effect.Value, sourceCard);
                break;

            case CardEffectType.Heal:
                if (targetUnit == null)
                {
                    Debug.LogWarning($"Cannot heal. Target unit is missing for {sourceCard.CardName}.");
                    return;
                }

                targetUnit.Heal(effect.Value);
                Debug.Log($"{sourceCard.CardName}: {targetUnit.CharacterName} healed {effect.Value}.");
                break;

            case CardEffectType.DealDamage:
                if (targetUnit == null)
                {
                    Debug.LogWarning($"Cannot deal damage. Target unit is missing for {sourceCard.CardName}.");
                    return;
                }

                targetUnit.TakeDamage(effect.Value);
                Debug.Log($"{sourceCard.CardName}: {targetUnit.CharacterName} took {effect.Value} effect damage.");
                break;

            case CardEffectType.BonusAttack:
                context.AttackValue += effect.Value;
                Debug.Log($"{sourceCard.CardName}: attack value increased by {effect.Value}.");
                break;

            case CardEffectType.BonusDefense:
                context.DefenseValue += effect.Value;
                Debug.Log($"{sourceCard.CardName}: defense value increased by {effect.Value}.");
                break;

            default:
                Debug.LogWarning($"Unhandled effect type {effect.EffectType} on {sourceCard.CardName}.");
                break;
        }
    }

    private void DrawCards(PlayerState targetPlayer, int amount, CardData sourceCard)
    {
        if (targetPlayer == null || targetPlayer.Deck == null)
        {
            Debug.LogWarning($"Cannot draw cards. Target player or deck is missing for {sourceCard.CardName}.");
            return;
        }

        int drawnCount = 0;

        for (int i = 0; i < amount; i++)
        {
            CardData drawnCard = targetPlayer.Deck.DrawCard();

            if (drawnCard == null)
            {
                break;
            }

            drawnCount++;
        }

        Debug.Log($"{sourceCard.CardName}: {targetPlayer.PlayerName} drew {drawnCount} card(s).");
    }

    private PlayerState GetTargetPlayer(CardEffectTarget target, CombatContext context, bool cardBelongsToDefender)
    {
        switch (target)
        {
            case CardEffectTarget.Self:
                return cardBelongsToDefender
                    ? context.DefendingPlayer
                    : context.AttackingPlayer;

            case CardEffectTarget.Opponent:
                return cardBelongsToDefender
                    ? context.AttackingPlayer
                    : context.DefendingPlayer;

            case CardEffectTarget.Attacker:
                return context.AttackingPlayer;

            case CardEffectTarget.Defender:
                return context.DefendingPlayer;

            default:
                Debug.LogWarning($"Unhandled effect target {target}.");
                return null;
        }
    }

    private string BuildRevealText(CombatContext context)
    {
        string defenseText = context.DefenseCard != null
            ? $"{context.DefenseCard.CardName} ({context.DefenseCard.Value})"
            : "no defense";

        return
            $"Combat revealed: {context.Attacker.CharacterName} attacked {context.Defender.CharacterName} with " +
            $"{context.AttackCard.CardName} ({context.AttackCard.Value}). Defender used {defenseText}.";
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

    private class CombatContext
    {
        public PlayerState AttackingPlayer { get; private set; }
        public PlayerState DefendingPlayer { get; private set; }
        public CharacterUnit Attacker { get; private set; }
        public CharacterUnit Defender { get; private set; }
        public CardData AttackCard { get; private set; }
        public CardData DefenseCard { get; private set; }

        public int AttackValue { get; set; }
        public int DefenseValue { get; set; }
        public CombatWinner Winner { get; set; }

        public CombatContext(
            PlayerState attackingPlayer,
            PlayerState defendingPlayer,
            CharacterUnit attacker,
            CharacterUnit defender,
            CardData attackCard,
            CardData defenseCard)
        {
            AttackingPlayer = attackingPlayer;
            DefendingPlayer = defendingPlayer;
            Attacker = attacker;
            Defender = defender;
            AttackCard = attackCard;
            DefenseCard = defenseCard;
            AttackValue = attackCard != null ? attackCard.Value : 0;
            DefenseValue = defenseCard != null ? defenseCard.Value : 0;
            Winner = CombatWinner.Defender;
        }
    }
}


