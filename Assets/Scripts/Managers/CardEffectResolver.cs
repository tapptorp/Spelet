using UnityEngine;

public enum CardEffectResolutionStatus
{
    Completed,
    NeedsMoreInput,
    Failed
}

public class CardEffectResolutionResult
{
    public CardEffectResolutionStatus Status { get; private set; }
    public string Message { get; private set; }

    public bool WasSuccessful => Status == CardEffectResolutionStatus.Completed;

    private CardEffectResolutionResult(CardEffectResolutionStatus status, string message)
    {
        Status = status;
        Message = message;
    }

    public static CardEffectResolutionResult Completed(string message = null)
    {
        return new CardEffectResolutionResult(CardEffectResolutionStatus.Completed, message);
    }

    public static CardEffectResolutionResult NeedsMoreInput(string message)
    {
        return new CardEffectResolutionResult(CardEffectResolutionStatus.NeedsMoreInput, message);
    }

    public static CardEffectResolutionResult Failed(string message)
    {
        return new CardEffectResolutionResult(CardEffectResolutionStatus.Failed, message);
    }
}

public class CardEffectContext
{
    public PlayerState PlayingPlayer { get; private set; }
    public PlayerState OpposingPlayer { get; private set; }

    public PlayerState AttackingPlayer { get; private set; }
    public PlayerState DefendingPlayer { get; private set; }

    public CharacterUnit Attacker { get; private set; }
    public CharacterUnit Defender { get; private set; }

    public CardData AttackCard { get; private set; }
    public CardData DefenseCard { get; private set; }

    public int AttackValue { get; set; }
    public int DefenseValue { get; set; }
    public CombatWinner Winner { get; set; }

    public bool IsCombat => AttackingPlayer != null && DefendingPlayer != null;

    private CardEffectContext() { }

    public static CardEffectContext CreateNonCombat(PlayerState playingPlayer, PlayerState opposingPlayer)
    {
        return new CardEffectContext
        {
            PlayingPlayer = playingPlayer,
            OpposingPlayer = opposingPlayer,
            Winner = CombatWinner.Defender
        };
    }

    public static CardEffectContext CreateCombat(
        PlayerState attackingPlayer,
        PlayerState defendingPlayer,
        CharacterUnit attacker,
        CharacterUnit defender,
        CardData attackCard,
        CardData defenseCard)
    {
        return new CardEffectContext
        {
            PlayingPlayer = attackingPlayer,
            OpposingPlayer = defendingPlayer,
            AttackingPlayer = attackingPlayer,
            DefendingPlayer = defendingPlayer,
            Attacker = attacker,
            Defender = defender,
            AttackCard = attackCard,
            DefenseCard = defenseCard,
            AttackValue = attackCard != null ? attackCard.Value : 0,
            DefenseValue = defenseCard != null ? defenseCard.Value : 0,
            Winner = CombatWinner.Defender
        };
    }
}

public class CardEffectResolver : MonoBehaviour
{
    public bool CanResolveEffectsNow(
        CardData card,
        CardEffectTiming timing,
        CardEffectContext context,
        bool cardBelongsToDefender,
        out string validationError)
    {
        validationError = null;

        if (card == null)
        {
            validationError = "Cannot resolve effects. Card is missing.";
            return false;
        }

        if (context == null)
        {
            validationError = $"Cannot resolve effects for {card.CardName}. Effect context is missing.";
            return false;
        }

        if (card.Effects == null)
        {
            return true;
        }

        foreach (CardEffectData effect in card.Effects)
        {
            if (effect == null || effect.Timing != timing)
            {
                continue;
            }

            if (!CanResolveEffectNow(effect, card, context, cardBelongsToDefender, out validationError))
            {
                return false;
            }
        }

        return true;
    }

    public CardEffectResolutionResult ResolveEffects(
        CardData card,
        CardEffectTiming timing,
        CardEffectContext context,
        bool cardBelongsToDefender)
    {
        if (!CanResolveEffectsNow(card, timing, context, cardBelongsToDefender, out string validationError))
        {
            Debug.Log(validationError);
            return CardEffectResolutionResult.Failed(validationError);
        }

        if (card.Effects == null)
        {
            return CardEffectResolutionResult.Completed();
        }

        foreach (CardEffectData effect in card.Effects)
        {
            if (effect == null || effect.Timing != timing)
            {
                continue;
            }

            ApplyEffect(effect, card, context, cardBelongsToDefender);
        }

        return CardEffectResolutionResult.Completed();
    }

    private bool CanResolveEffectNow(
        CardEffectData effect,
        CardData sourceCard,
        CardEffectContext context,
        bool cardBelongsToDefender,
        out string validationError)
    {
        validationError = null;

        if (effect.Value <= 0)
        {
            validationError = $"{sourceCard.CardName} has an effect with value <= 0. Fix this on the CardData asset.";
            return false;
        }

        switch (effect.EffectType)
        {
            case CardEffectType.DrawCards:
                if (GetTargetPlayer(effect.Target, context, cardBelongsToDefender) == null)
                {
                    validationError = $"{sourceCard.CardName} cannot draw cards because its target could not be resolved.";
                    return false;
                }
                return true;

            case CardEffectType.Heal:
            case CardEffectType.DealDamage:
                if (GetTargetUnit(effect.Target, context, cardBelongsToDefender) == null)
                {
                    validationError = $"{sourceCard.CardName} cannot resolve {effect.EffectType} because its target unit could not be resolved.";
                    return false;
                }
                return true;

            case CardEffectType.BonusAttack:
            case CardEffectType.BonusDefense:
                if (!context.IsCombat)
                {
                    validationError = $"{sourceCard.CardName} has {effect.EffectType}, but that effect only works during combat.";
                    return false;
                }
                return true;

            default:
                validationError = $"Unhandled effect type {effect.EffectType} on {sourceCard.CardName}.";
                return false;
        }
    }

    private void ApplyEffect(
        CardEffectData effect,
        CardData sourceCard,
        CardEffectContext context,
        bool cardBelongsToDefender)
    {
        switch (effect.EffectType)
        {
            case CardEffectType.DrawCards:
                DrawCards(GetTargetPlayer(effect.Target, context, cardBelongsToDefender), effect.Value, sourceCard);
                break;

            case CardEffectType.Heal:
                CharacterUnit healTarget = GetTargetUnit(effect.Target, context, cardBelongsToDefender);
                healTarget.Heal(effect.Value);
                Debug.Log($"{sourceCard.CardName}: {healTarget.CharacterName} healed {effect.Value}.");
                break;

            case CardEffectType.DealDamage:
                CharacterUnit damageTarget = GetTargetUnit(effect.Target, context, cardBelongsToDefender);
                damageTarget.TakeDamage(effect.Value);
                Debug.Log($"{sourceCard.CardName}: {damageTarget.CharacterName} took {effect.Value} effect damage.");
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
                // Fatigue damage should be handled by the caller/game rules later.
                break;
            }

            drawnCount++;
        }

        Debug.Log($"{sourceCard.CardName}: {targetPlayer.PlayerName} drew {drawnCount} card(s).");
    }

    private CharacterUnit GetTargetUnit(CardEffectTarget target, CardEffectContext context, bool cardBelongsToDefender)
    {
        PlayerState targetPlayer = GetTargetPlayer(target, context, cardBelongsToDefender);
        return targetPlayer != null ? targetPlayer.Unit : null;
    }

    private PlayerState GetTargetPlayer(CardEffectTarget target, CardEffectContext context, bool cardBelongsToDefender)
    {
        if (context == null)
        {
            return null;
        }

        if (context.IsCombat)
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
            }
        }
        else
        {
            switch (target)
            {
                case CardEffectTarget.Self:
                    return context.PlayingPlayer;

                case CardEffectTarget.Opponent:
                    return context.OpposingPlayer;

                case CardEffectTarget.Attacker:
                case CardEffectTarget.Defender:
                    return null;
            }
        }

        return null;
    }
}


