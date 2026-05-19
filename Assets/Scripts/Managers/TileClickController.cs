using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TileClickController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Camera mainCamera;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask clickableLayers = ~0;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Input Settings")]
    [SerializeField] private bool ignoreClicksOverUI = true;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (uiManager == null)
        {
            uiManager = FindAnyObjectByType<UIManager>();
        }



        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleLeftMouseClick();
        }
    }

    private void HandleLeftMouseClick()
    {

        if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (uiManager != null)
        {
            uiManager.ClearSelectedCardInfo();
        }

        if (gameManager == null)
        {
            Debug.LogWarning("TileClickController is missing a GameManager reference.");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("TileClickController is missing a Camera reference.");
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, clickableLayers))
        {
            Debug.Log("Clicked, but no object was hit.");
            return;
        }

        Tile clickedTile = hit.collider.GetComponentInParent<Tile>();

        if (clickedTile == null)
        {
            Debug.Log($"Clicked {hit.collider.gameObject.name}, but it was not a tile.");
            return;
        }

        gameManager.TryMoveActiveUnitToTile(clickedTile);
    }
}