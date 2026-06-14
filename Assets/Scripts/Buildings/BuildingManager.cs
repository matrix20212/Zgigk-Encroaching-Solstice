using System;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Podgląd budynku")]
    public Material previewMaterialOK;
    public Material previewMaterialBlock;

    [Header("Korekta wysokości")]
    public float heightOffset = 0.05f;

    private BuildingData selectedBuilding;
    private GameObject previewObj;
    private bool isBuilding = false;
    private int currentRotation = 0;

    public bool IsBuildingMode => isBuilding;

    void Awake() => Instance = this;

    void Update()
    {
        if (!isBuilding) return;

        if (Input.GetKeyDown(KeyCode.R))
            currentRotation = (currentRotation + 90) % 360;

        UpdatePreview();

        if (Input.GetMouseButtonDown(0)) TryPlace();
        if (Input.GetMouseButtonDown(1)) CancelBuilding();
    }

    public void StartBuilding(BuildingData data)
    {
        if (!ResourceManager.Instance.CanAfford(data.woodCost, data.metalCost))
        {
            Debug.Log("Za mało surowców!");
            return;
        }

        selectedBuilding = data;
        isBuilding = true;
        currentRotation = 0;

        if (previewObj != null)
            Destroy(previewObj);

        previewObj = Instantiate(data.prefab);
        SetPreviewMaterial(previewObj, previewMaterialOK);
        SetPreviewCollidersEnabled(false);
    }

    void UpdatePreview()
    {
        if (previewObj == null || selectedBuilding == null)
            return;

        Vector3 worldPos = GetMouseWorldPosition();
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
        Vector2Int placementSize = GetPlacementSize();

        Vector3 snappedPos = GetBuildingCenterWorldPosition(gridPos, placementSize);
        snappedPos.y = GetGroundHeight(snappedPos.x, snappedPos.z) + heightOffset;

        previewObj.transform.position = snappedPos;
        previewObj.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        bool canPlace = GridManager.Instance.IsAreaFree(
            gridPos.x, gridPos.y,
            placementSize.x, placementSize.y
        );

        SetPreviewMaterial(previewObj, canPlace ? previewMaterialOK : previewMaterialBlock);
    }

    void TryPlace()
    {
        if (selectedBuilding == null)
            return;

        if (!ResourceManager.Instance.CanAfford(selectedBuilding.woodCost, selectedBuilding.metalCost))
        {
            CancelBuilding();
            return;
        }

        Vector3 worldPos = GetMouseWorldPosition();
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
        Vector2Int placementSize = GetPlacementSize();

        if (!GridManager.Instance.IsAreaFree(
            gridPos.x, gridPos.y,
            placementSize.x, placementSize.y))
            return;

        ResourceManager.Instance.Spend(selectedBuilding.woodCost, selectedBuilding.metalCost);

        Vector3 finalPos = GetBuildingCenterWorldPosition(gridPos, placementSize);
        finalPos.y = GetGroundHeight(finalPos.x, finalPos.z) + heightOffset;

        Quaternion finalRotation = Quaternion.Euler(0f, currentRotation, 0f);
        GameObject building = Instantiate(selectedBuilding.prefab, finalPos, finalRotation);

        BuildingInstance bi = building.GetComponent<BuildingInstance>();
        if (bi == null)
            bi = building.AddComponent<BuildingInstance>();

        bi.Init(selectedBuilding, gridPos, GridManager.Instance);

        GridManager.Instance.OccupyArea(
            gridPos.x, gridPos.y,
            placementSize.x, placementSize.y,
            building
        );

        if (!selectedBuilding.continuousPlacement)
        {
            CancelBuilding();
            return;
        }

        if (!ResourceManager.Instance.CanAfford(selectedBuilding.woodCost, selectedBuilding.metalCost))
        {
            CancelBuilding();
            return;
        }

        UpdatePreview();
    }

    Vector2Int GetPlacementSize()
    {
        return new Vector2Int(selectedBuilding.sizeX, selectedBuilding.sizeZ);
    }

    Vector3 GetBuildingCenterWorldPosition(Vector2Int gridPos, Vector2Int placementSize)
    {
        Vector3 firstCellCenter = GridManager.Instance.GridToWorld(gridPos.x, gridPos.y);
        float cellSize = GridManager.Instance.cellSize;

        float offsetX = (placementSize.x - 1) * cellSize * 0.5f;
        float offsetZ = (placementSize.y - 1) * cellSize * 0.5f;

        return firstCellCenter + new Vector3(offsetX, 0f, offsetZ);
    }

    float GetGroundHeight(float x, float z)
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

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 500f);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreHit(hit.collider))
                continue;

            return hit.point;
        }

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero;
    }

    bool ShouldIgnoreHit(Collider collider)
    {
        if (collider == null)
            return true;

        if (previewObj != null && collider.transform.IsChildOf(previewObj.transform))
            return true;

        if (collider.GetComponentInParent<BuildingInstance>() != null)
            return true;

        if (collider.GetComponentInParent<EnemyUnit>() != null)
            return true;

        return false;
    }

    void SetPreviewCollidersEnabled(bool enabled)
    {
        if (previewObj == null)
            return;

        Collider[] colliders = previewObj.GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
            collider.enabled = enabled;
    }

    public void CancelBuilding()
    {
        isBuilding = false;
        selectedBuilding = null;
        currentRotation = 0;

        if (previewObj != null)
            Destroy(previewObj);
    }

    void SetPreviewMaterial(GameObject obj, Material mat)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }
}