using UnityEngine;

public class Tile : MonoBehaviour
{
    // Tile position på spelbrädet, exempelvis (0,0)
    public Vector2Int GridPosition { get; private set; }

    //public CharacterUnit OccupyingUnit { get; private set; } Ska vara detta sen
    public GameObject OccupyingUnit { get; private set; }

    // True om någon står på rutan
    public bool IsOccupied => OccupyingUnit != null;

    private void Awake()
    {
        Debug.Log("Tile Awake körs på: " + gameObject.name);
        // Hämtar tile position automatiskt från Unity positionen
        GridPosition = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Döp om objectet i hierarchy
        gameObject.name = $"Tile_{GridPosition.x}_{GridPosition.y}";
    }

    //public void SetOccupyingUnit(CharacterUnit unit) Ska vara detta sen
    public void SetOccupyingUnit(GameObject unit)
    {
        OccupyingUnit = unit;
    }

    // Töm rutan när unit lämnar den
    public void ClearOccupyingUnit()
    {
        OccupyingUnit = null;
    }
}