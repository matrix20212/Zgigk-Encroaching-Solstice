using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSelectionManager : MonoBehaviour
{
    public static BuildingSelectionManager Instance;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private BuildingMenuUI menuUI;
    [SerializeField] private float maxRayDistance = 500f;
    [SerializeField] private TMP_FontAsset customFont;
    private BuildingInstance selectedBuilding;

    private void Start()
    {
        Instance = this;
        if (targetCamera == null)
            targetCamera = Camera.main;
        if (menuUI == null)
            menuUI = FindFirstObjectByType<BuildingMenuUI>();
        if (menuUI == null)
        {
            GameObject menuObject = new GameObject("BuildingMenuUI");
            menuUI = menuObject.AddComponent<BuildingMenuUI>();
        }

        Invoke(nameof(ApplyFont), 0.1f);
    }

    private void ApplyFont()
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log($"Znaleziono {texts.Length} tekstów");

        foreach (TMP_Text t in texts)
        {
            Debug.Log($"{t.name} -> {t.font.name}");
            t.font = customFont;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ClearSelection();

        if (Input.GetMouseButtonDown(1))
            ClearSelection();

        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (BuildingManager.Instance != null && BuildingManager.Instance.IsBuildingMode)
            return;

        TrySelectBuildingByGrid();
    }

    private void TrySelectBuildingByGrid()
    {
        if (GridManager.Instance == null)
            return;

        Vector3 worldPosition = GetMouseWorldPositionOnGrid();
        Vector2Int gridPosition = GridManager.Instance.WorldToGrid(worldPosition);

        GridCell cell = GridManager.Instance.GetCell(gridPosition.x, gridPosition.y);

        if (cell == null || !cell.isOccupied || cell.occupant == null)
        {
            ClearSelection();
            return;
        }

        BuildingInstance building = cell.occupant.GetComponent<BuildingInstance>();

        if (building == null)
            building = cell.occupant.GetComponentInChildren<BuildingInstance>();

        if (building == null)
        {
            ClearSelection();
            return;
        }

        SelectBuilding(building);
    }

    private Vector3 GetMouseWorldPositionOnGrid()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return Vector3.zero;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
                continue;

            return hit.point;
        }

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }

    private bool ShouldIgnoreHit(Collider collider)
    {
        if (collider == null)
            return true;

        if (collider.GetComponentInParent<BuildingInstance>() != null)
            return true;

        if (collider.GetComponentInParent<EnemyUnit>() != null)
            return true;

        return false;
    }

    public void SelectBuilding(BuildingInstance building)
    {
        selectedBuilding = building;
        if (menuUI != null)
        {
            menuUI.Show(building);
            ApplyFont();
        }
    }

    public void ClearSelection()
    {
        selectedBuilding = null;

        if (menuUI != null)
            menuUI.Hide();
    }

    public void DeleteSelectedBuilding()
    {
        if (selectedBuilding == null)
            return;

        if (!selectedBuilding.CanBeManuallyDeleted)
            return;

        BuildingInstance buildingToDelete = selectedBuilding;
        ClearSelection();
        buildingToDelete.RemoveByPlayer();
    }
}