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

    void Awake() => Instance = this;

    void Update()
    {
        if (!isBuilding) return;
        UpdatePreview();

        if (Input.GetKeyDown(KeyCode.R))
            currentRotation = (currentRotation + 90) % 360;

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
        previewObj = Instantiate(data.prefab);
        SetPreviewMaterial(previewObj, previewMaterialOK);
    }

    void UpdatePreview()
    {
        Vector3 worldPos = GetMouseWorldPosition();
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
        Vector3 snappedPos = GridManager.Instance.GridToWorld(gridPos.x, gridPos.y);

        snappedPos.y = GetTerrainHeight(snappedPos.x, snappedPos.z) + heightOffset;

        previewObj.transform.position = snappedPos;
        previewObj.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

        bool canPlace = GridManager.Instance.IsAreaFree(
            gridPos.x, gridPos.y,
            selectedBuilding.sizeX, selectedBuilding.sizeZ
        );

        SetPreviewMaterial(previewObj, canPlace ? previewMaterialOK : previewMaterialBlock);
    }

    void TryPlace()
    {
        Vector3 worldPos = GetMouseWorldPosition();
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);

        if (!GridManager.Instance.IsAreaFree(
            gridPos.x, gridPos.y,
            selectedBuilding.sizeX, selectedBuilding.sizeZ)) return;

        ResourceManager.Instance.Spend(selectedBuilding.woodCost, selectedBuilding.metalCost);

        Vector3 finalPos = GridManager.Instance.GridToWorld(gridPos.x, gridPos.y);
        finalPos.y = GetTerrainHeight(finalPos.x, finalPos.z) + heightOffset;

        Quaternion finalRotation = Quaternion.Euler(0f, currentRotation, 0f);
        GameObject building = Instantiate(selectedBuilding.prefab, finalPos, finalRotation);

        BuildingInstance bi = building.GetComponent<BuildingInstance>();
        if (bi == null) bi = building.AddComponent<BuildingInstance>();

        bi.Init(selectedBuilding, gridPos, GridManager.Instance);

        GridManager.Instance.OccupyArea(
            gridPos.x, gridPos.y,
            selectedBuilding.sizeX, selectedBuilding.sizeZ, building);

        CancelBuilding();
    }

    float GetTerrainHeight(float x, float z)
    {
        Ray ray = new Ray(new Vector3(x, 100f, z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            return hit.point.y;

        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
            return terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;

        return 0f;
    }

    public void CancelBuilding()
    {
        isBuilding = false;
        selectedBuilding = null;
        currentRotation = 0;
        if (previewObj != null) Destroy(previewObj);
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            return hit.point;

        Plane plane = new Plane(Vector3.up, new Vector3(0, 2f, 0));
        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero;
    }

    void SetPreviewMaterial(GameObject obj, Material mat)
    {
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }
}