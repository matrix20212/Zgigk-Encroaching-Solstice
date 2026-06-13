using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyUnit enemyPrefab;
    [SerializeField] private Vector2 mapHalfSize = new Vector2(20f, 20f);
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private int maxAliveEnemies = 20;
    [SerializeField] private float yPosition = 0.5f;

    private Coroutine spawnRoutine;

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.2f, spawnInterval));

            EnemyUnit[] enemies = Object.FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

            if (enemies.Length < maxAliveEnemies)
                SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        Vector3 position = GetRandomEdgePosition();
        Instantiate(enemyPrefab, position, Quaternion.identity);
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);
        float x = 0f;
        float z = 0f;

        if (side == 0)
        {
            x = Random.Range(-mapHalfSize.x, mapHalfSize.x);
            z = mapHalfSize.y;
        }
        else if (side == 1)
        {
            x = Random.Range(-mapHalfSize.x, mapHalfSize.x);
            z = -mapHalfSize.y;
        }
        else if (side == 2)
        {
            x = mapHalfSize.x;
            z = Random.Range(-mapHalfSize.y, mapHalfSize.y);
        }
        else
        {
            x = -mapHalfSize.x;
            z = Random.Range(-mapHalfSize.y, mapHalfSize.y);
        }

        return new Vector3(x, yPosition, z);
    }
}
