using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private float tileSpacing = 2f;

    private Dictionary<Vector2Int, Tile> tilesByPosition = new Dictionary<Vector2Int, Tile>();

    private void Start()
    {
        RegisterAllTiles();
    }

    private void RegisterAllTiles()
    {
        tilesByPosition.Clear();

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsInactive.Exclude);

        foreach (Tile tile in allTiles)
        {
            Vector2Int position = WorldPositionToGridPosition(tile.transform.position);

            tile.SetGridPosition(position);

            if (tilesByPosition.ContainsKey(position))
            {
                Debug.LogWarning($"Duplicate tile position found at {position}. Tile: {tile.name}");
                continue;
            }

            tilesByPosition.Add(position, tile);
        }

        Debug.Log($"BoardManager registered {tilesByPosition.Count} tiles.");
    }

    private Vector2Int WorldPositionToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / tileSpacing),
            Mathf.RoundToInt(worldPosition.z / tileSpacing)
        );
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        tilesByPosition.TryGetValue(position, out Tile tile);
        return tile;
    }

    public bool HasTileAtPosition(Vector2Int position)
    {
        return tilesByPosition.ContainsKey(position);
    }

    public bool IsTileOccupied(Vector2Int position)
    {
        Tile tile = GetTileAtPosition(position);

        if (tile == null)
        {
            return false;
        }

        return tile.IsOccupied;
    }
}