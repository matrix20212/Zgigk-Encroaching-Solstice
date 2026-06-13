using System.Collections;
using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    [SerializeField] private BuildingData data;

    private int currentHp;
    private Vector2Int gridOrigin;
    private GridManager gridManager;
    private Coroutine roleRoutine;

    public BuildingData Data => data;
    public bool IsAlive => currentHp > 0;

    private void Awake()
    {
        if (data != null)
            currentHp = Mathf.Max(1, data.maxHp);
    }

    private void Start()
    {
        if (data != null && roleRoutine == null)
            StartRoleRoutine();
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
        StartRoleRoutine();
    }

    private void StartRoleRoutine()
    {
        if (roleRoutine != null)
            StopCoroutine(roleRoutine);

        if (data == null)
            return;

        if (data.role == BuildingRole.Production)
            roleRoutine = StartCoroutine(ProductionLoop());

        if (data.role == BuildingRole.Defensive)
            roleRoutine = StartCoroutine(DefenseLoop());
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

    private IEnumerator DefenseLoop()
    {
        while (IsAlive)
        {
            EnemyUnit target = FindTarget();

            if (target != null)
                target.TakeDamage(data.attackDamage);

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

    public void TakeDamage(int damage)
    {
        if (!IsAlive)
            return;

        currentHp -= Mathf.Max(0, damage);

        if (currentHp <= 0)
            Die();
    }

    private void Die()
    {
        currentHp = 0;

        if (roleRoutine != null)
            StopCoroutine(roleRoutine);

        if (gridManager != null && data != null)
            gridManager.ReleaseArea(gridOrigin, data.GridSize);

        Destroy(gameObject);
    }
}