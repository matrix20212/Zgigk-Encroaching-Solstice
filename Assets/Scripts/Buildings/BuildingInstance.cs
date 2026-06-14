using System.Collections;
using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [SerializeField] private BuildingData data;

    private int currentHp;
    private Vector2Int gridOrigin;
    private GridManager gridManager;

    private Coroutine productionRoutine;
    private Coroutine defenseRoutine;
    private Coroutine treeClearingRoutine;

    private bool destroyed = false;

    public BuildingData Data => data;
    public bool IsAlive => !destroyed && currentHp > 0;
    public int CurrentHp => currentHp;
    public int MaxHp => data != null ? Mathf.Max(1, data.maxHp) : 100;
    public float HealthPercent => Mathf.Clamp01((float)currentHp / MaxHp);
    public bool CanBeManuallyDeleted => data != null && !data.blockManualDelete;

    private void Awake()
    {
        if (data != null)
            currentHp = Mathf.Max(1, data.maxHp);
    }

    private void Start()
    {
        if (data != null && productionRoutine == null && defenseRoutine == null && treeClearingRoutine == null)
            StartRoleRoutines();

        EnsureHealthBar();
    }

    private void OnEnable()
    {
        BuildingRegistry.Register(this);
    }

    private void OnDisable()
    {
        BuildingRegistry.Unregister(this);
    }

    public void Init(BuildingData buildingData)
    {
        Initialize(buildingData, Vector2Int.zero, null);
    }

    public void Init(BuildingData buildingData, Vector2Int origin, GridManager grid)
    {
        Initialize(buildingData, origin, grid);
    }

    public void Initialize(BuildingData buildingData, Vector2Int origin, GridManager grid)
    {
        data = buildingData;
        gridOrigin = origin;
        gridManager = grid;
        currentHp = data != null ? Mathf.Max(1, data.maxHp) : 100;
        destroyed = false;

        StopAllRoleRoutines();
        StartRoleRoutines();
        EnsureHealthBar();
    }

    private void EnsureHealthBar()
    {
        BuildingHealthBar healthBar = GetComponent<BuildingHealthBar>();

        if (healthBar == null)
            healthBar = gameObject.AddComponent<BuildingHealthBar>();

        healthBar.Init(this);
    }

    private void StartRoleRoutines()
    {
        if (data == null)
            return;

        if (data.role == BuildingRole.Production)
            productionRoutine = StartCoroutine(ProductionLoop());

        if (data.role == BuildingRole.Defensive)
            defenseRoutine = StartCoroutine(DefenseLoop());

        if (data.clearTreesInRange)
            treeClearingRoutine = StartCoroutine(TreeClearingLoop());
    }

    private void StopAllRoleRoutines()
    {
        if (productionRoutine != null)
            StopCoroutine(productionRoutine);

        if (defenseRoutine != null)
            StopCoroutine(defenseRoutine);

        if (treeClearingRoutine != null)
            StopCoroutine(treeClearingRoutine);

        productionRoutine = null;
        defenseRoutine = null;
        treeClearingRoutine = null;
    }

    private IEnumerator ProductionLoop()
    {
        while (IsAlive)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, data.productionInterval));
            ProduceResources();
        }
    }

    public void ProduceResources()
    {
        if (data == null || !IsAlive)
            return;

        if (data.role != BuildingRole.Production)
            return;

        int amount = data.productionAmount;

        if (amount <= 0 && data.productionPerWorkerPerDay > 0)
            amount = data.productionPerWorkerPerDay;

        if (amount <= 0)
            return;

        if (ResourceManager.Instance != null)
            ResourceManager.Instance.AddResource(data.producedResource, amount);
    }

    private IEnumerator TreeClearingLoop()
    {
        while (IsAlive)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, data.treeClearInterval));
            ClearNearestTree();
        }
    }

    private void ClearNearestTree()
    {
        if (data == null || !data.clearTreesInRange)
            return;

        TreeResource tree = TreeResource.GetNearest(transform.position, data.treeClearRange);

        if (tree != null)
            tree.RemoveFromMap();
    }

    private IEnumerator DefenseLoop()
    {
        while (IsAlive)
        {
            EnemyUnit target = FindTarget();

            if (target != null)
                Shoot(target);

            yield return new WaitForSeconds(Mathf.Max(0.1f, data.attackCooldown));
        }
    }

    private EnemyUnit FindTarget()
    {
        EnemyUnit[] enemies = Object.FindObjectsByType<EnemyUnit>();
        EnemyUnit nearest = null;
        float bestDistance = data.attackRange * data.attackRange;

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive)
                continue;

            float distance = (enemy.transform.position - transform.position).sqrMagnitude;

            if (distance <= bestDistance)
            {
                bestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void Shoot(EnemyUnit target)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * data.projectileSpawnHeight;
        GameObject projectileObject;

        if (data.projectilePrefab != null)
        {
            projectileObject = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.transform.position = spawnPosition;
            projectileObject.transform.localScale = Vector3.one * 0.25f;

            Collider collider = projectileObject.GetComponent<Collider>();

            if (collider != null)
                Destroy(collider);
        }

        TowerProjectile projectile = projectileObject.GetComponent<TowerProjectile>();

        if (projectile == null)
            projectile = projectileObject.AddComponent<TowerProjectile>();

        projectile.Init(target, data.attackDamage, data.projectileSpeed, data.projectileHitDistance);
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive)
            return;

        currentHp -= Mathf.Max(0, damage);

        if (currentHp <= 0)
            DestroyBuilding();
    }

    public void RemoveByPlayer()
    {
        if (!CanBeManuallyDeleted)
            return;

        DestroyBuilding();
    }

    private void DestroyBuilding()
    {
        if (destroyed)
            return;

        destroyed = true;
        currentHp = 0;

        StopAllRoleRoutines();

        if (gridManager != null && data != null)
            gridManager.ReleaseArea(gridOrigin, data.GridSize);

        if (data != null && data.triggerGameOverOnDestroyed && GameManager.Instance != null)
            GameManager.Instance.GameOver();

        Destroy(gameObject);
    }
}