using System;
using UnityEngine;

public class StartingBuildingPlacer : MonoBehaviour
{
    [Header("Ratusz")]
    [SerializeField] private BuildingData townHallData;
    [SerializeField] private Vector2Int gridPosition = new Vector2Int(8, 8);
    [SerializeField] private float heightOffset = 0.05f;

    [Header("Kamera")]
    [SerializeField] private bool moveCameraToBuilding = true;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 12f, -10f);

    private void Start()
    {
        PlaceStartingBuilding();
    }

    private void PlaceStartingBuilding()
    {
        if (townHallData == null || townHallData.prefab == null)
        {
            Debug.LogError("Brak BuildingData albo prefaba ratusza w StartingBuildingPlacer.");
            return;
        }

        if (GridManager.Instance == null)
        {
            Debug.LogError("Brak GridManager.Instance na scenie.");
            return;
        }

        Vector2Int size = townHallData.GridSize;

        if (!GridManager.Instance.IsAreaFree(gridPosition.x, gridPosition.y, size.x, size.y))
        {
            Debug.LogError("Nie mo¿na postawiæ ratusza startowego, bo wybrane pola s¹ zajête.");
            return;
        }

        Vector3 position = GetBuildingCenterWorldPosition(gridPosition, size);
        position.y = GetGroundHeight(position.x, position.z) + heightOffset;

        GameObject building = Instantiate(townHallData.prefab, position, Quaternion.identity);

        BuildingInstance instance = building.GetComponent<BuildingInstance>();

        if (instance == null)
            instance = building.AddComponent<BuildingInstance>();

        instance.Init(townHallData, gridPosition, GridManager.Instance);

        GridManager.Instance.OccupyArea(
            gridPosition.x,
            gridPosition.y,
            size.x,
            size.y,
            building
        );

        if (moveCameraToBuilding)
            MoveCameraTo(position);
    }

    private Vector3 GetBuildingCenterWorldPosition(Vector2Int startGridPosition, Vector2Int size)
    {
        Vector3 firstCellCenter = GridManager.Instance.GridToWorld(startGridPosition.x, startGridPosition.y);
        float cellSize = GridManager.Instance.cellSize;

        float offsetX = (size.x - 1) * cellSize * 0.5f;
        float offsetZ = (size.y - 1) * cellSize * 0.5f;

        return firstCellCenter + new Vector3(offsetX, 0f, offsetZ);
    }

    private float GetGroundHeight(float x, float z)
    {
        Ray ray = new Ray(new Vector3(x, 100f, z), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 200f);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
                continue;

            return hit.point.y;
        }

        Terrain terrain = Terrain.activeTerrain;

        if (terrain != null)
            return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;

        return 0f;
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

    private void MoveCameraTo(Vector3 buildingPosition)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        CameraController cameraController = targetCamera.GetComponent<CameraController>();

        if (cameraController == null)
            cameraController = targetCamera.GetComponentInParent<CameraController>();

        if (cameraController != null)
        {
            cameraController.FocusOnPoint(buildingPosition, cameraOffset, true);
            return;
        }

        targetCamera.transform.position = buildingPosition + cameraOffset;
        targetCamera.transform.LookAt(buildingPosition);
    }
}