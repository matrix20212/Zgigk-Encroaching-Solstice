using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Encroaching Solstice/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Sprite icon;

    public int sizeX = 1;
    public int sizeZ = 1;
    public Vector2Int size = Vector2Int.one;

    public bool continuousPlacement = false;
    public bool blockManualDelete = false;
    public bool triggerGameOverOnDestroyed = false;

    public int woodCost;
    public int foodCost;
    public int metalCost;

    public int maxHp = 100;

    public BuildingRole role = BuildingRole.Production;

    public ResourceType producedResource = ResourceType.Wood;
    public float productionInterval = 5f;
    public int productionAmount = 10;

    public int maxWorkers;
    public int productionPerWorkerPerDay;

    public bool clearTreesInRange = false;
    public float treeClearInterval = 8f;
    public float treeClearRange = 8f;

    public float attackRange = 8f;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileSpawnHeight = 1.5f;
    public float projectileHitDistance = 0.25f;

    public Vector2Int GridSize => new Vector2Int(Mathf.Max(1, sizeX), Mathf.Max(1, sizeZ));

    public AudioClip shootSound;

    private void OnValidate()
    {
        sizeX = Mathf.Max(1, sizeX);
        sizeZ = Mathf.Max(1, sizeZ);
        size = new Vector2Int(sizeX, sizeZ);

        maxHp = Mathf.Max(1, maxHp);

        productionInterval = Mathf.Max(0.1f, productionInterval);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);

        treeClearInterval = Mathf.Max(0.1f, treeClearInterval);
        treeClearRange = Mathf.Max(0f, treeClearRange);

        projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
        projectileSpawnHeight = Mathf.Max(0f, projectileSpawnHeight);
        projectileHitDistance = Mathf.Max(0.05f, projectileHitDistance);
    }
}