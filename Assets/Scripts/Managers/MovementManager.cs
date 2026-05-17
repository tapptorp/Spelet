using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public bool TryMoveUnit(CharacterUnit unit, Tile targetTile, out string validationError)
    {
        validationError = GetMoveValidationError(unit, targetTile);

        if (validationError != null)
        {
            return false;
        }

        unit.PlaceOnTile(targetTile);
        return true;
    }

    public bool CanMoveUnitToTile(CharacterUnit unit, Tile targetTile)
    {
        return GetMoveValidationError(unit, targetTile) == null;
    }

    public string GetMoveValidationError(CharacterUnit unit, Tile targetTile)
    {
        if (unit == null)
        {
            return "Cannot move. Unit is null.";
        }

        if (unit.CurrentTile == null)
        {
            return $"{unit.CharacterName} has no current tile.";
        }

        if (targetTile == null)
        {
            return "Cannot move. Target tile is null.";
        }

        if (targetTile.IsOccupied && targetTile.OccupyingUnit != unit)
        {
            return $"Tile at {targetTile.GridPosition} is occupied.";
        }

        if (!IsTileWithinMovementRange(unit, targetTile))
        {
            return $"{targetTile.GridPosition} is outside movement range.";
        }

        return null;
    }

    private bool IsTileWithinMovementRange(CharacterUnit unit, Tile targetTile)
    {
        Vector2Int unitPosition = unit.CurrentTile.GridPosition;
        Vector2Int targetPosition = targetTile.GridPosition;

        int distance = Mathf.Abs(unitPosition.x - targetPosition.x) +
                       Mathf.Abs(unitPosition.y - targetPosition.y);

        return distance <= unit.Movement;
    }
}


