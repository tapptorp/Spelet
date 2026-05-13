using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public CharacterUnit OccupyingUnit { get; private set; }

    public bool IsOccupied => OccupyingUnit != null;

    public void SetGridPosition(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        gameObject.name = $"Tile_{GridPosition.x}_{GridPosition.y}";
    }

    public void SetOccupyingUnit(CharacterUnit unit)
    {
        OccupyingUnit = unit;
    }

    public void ClearOccupyingUnit()
    {
        OccupyingUnit = null;
    }
}