using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wróg")]
    [SerializeField] private EnemyUnit enemyPrefab;

    [Header("Mapa")]
    [SerializeField] private bool useGridAsMapBounds = true;
    [SerializeField] private Vector3 mapCenter = Vector3.zero;
    [SerializeField] private Vector2 mapHalfSize = new Vector2(20f, 20f);
    [SerializeField] private float spawnMargin = 2f;
    [SerializeField] private float yPosition = 0.5f;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private int maxAliveEnemies = 20;
    [SerializeField] private int enemiesPerSpawn = 1;

    [Header("Skalowanie trudnoœci")]
    [SerializeField] private bool scaleDifficultyOverTime = true;
    [SerializeField] private float difficultyIncreaseInterval = 30f;
    [SerializeField] private float spawnIntervalMultiplier = 0.9f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private int maxAliveIncrease = 3;
    [SerializeField] private int enemiesPerSpawnIncreaseEveryLevels = 3;
    [SerializeField] private int maxEnemiesPerSpawn = 5;

    private Coroutine spawnRoutine;
    private Coroutine difficultyRoutine;
    private int difficultyLevel = 0;

    private void Start()
    {
        UpdateMapBoundsFromGrid();
    }

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());

        if (scaleDifficultyOverTime)
            difficultyRoutine = StartCoroutine(DifficultyLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        if (difficultyRoutine != null)
            StopCoroutine(difficultyRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.2f, spawnInterval));

            EnemyUnit[] enemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

            if (enemies.Length >= maxAliveEnemies)
                continue;

            int freeSlots = maxAliveEnemies - enemies.Length;
            int count = Mathf.Min(enemiesPerSpawn, freeSlots);

            for (int i = 0; i < count; i++)
                SpawnEnemy();
        }
    }

    private IEnumerator DifficultyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(1f, difficultyIncreaseInterval));
            IncreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        difficultyLevel++;

        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * spawnIntervalMultiplier);
        maxAliveEnemies += maxAliveIncrease;

        if (enemiesPerSpawnIncreaseEveryLevels > 0 &&
            difficultyLevel % enemiesPerSpawnIncreaseEveryLevels == 0)
        {
            enemiesPerSpawn = Mathf.Min(maxEnemiesPerSpawn, enemiesPerSpawn + 1);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        UpdateMapBoundsFromGrid();

        Vector3 position = GetRandomEdgePosition();
        Instantiate(enemyPrefab, position, Quaternion.identity);
    }

    private void UpdateMapBoundsFromGrid()
    {
        if (!useGridAsMapBounds)
            return;

        if (GridManager.Instance == null)
            return;

        float width = GridManager.Instance.width * GridManager.Instance.cellSize;
        float height = GridManager.Instance.height * GridManager.Instance.cellSize;

        mapCenter = new Vector3(width * 0.5f, 0f, height * 0.5f);
        mapHalfSize = new Vector2(width * 0.5f, height * 0.5f);
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);

        float minX = mapCenter.x - mapHalfSize.x - spawnMargin;
        float maxX = mapCenter.x + mapHalfSize.x + spawnMargin;
        float minZ = mapCenter.z - mapHalfSize.y - spawnMargin;
        float maxZ = mapCenter.z + mapHalfSize.y + spawnMargin;

        float x;
        float z;

        if (side == 0)
        {
            x = Random.Range(minX, maxX);
            z = maxZ;
        }
        else if (side == 1)
        {
            x = Random.Range(minX, maxX);
            z = minZ;
        }
        else if (side == 2)
        {
            x = maxX;
            z = Random.Range(minZ, maxZ);
        }
        else
        {
            x = minX;
            z = Random.Range(minZ, maxZ);
        }

        return new Vector3(x, yPosition, z);
    }
}