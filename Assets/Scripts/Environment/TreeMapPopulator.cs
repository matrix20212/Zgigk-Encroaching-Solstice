using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeMapPopulator : MonoBehaviour
{
    [Header("Prefaby drzew")]
    [SerializeField] private List<GameObject> treePrefabs = new List<GameObject>();

    [Header("Zagêszczenie")]
    [Range(0f, 1f)]
    [SerializeField] private float density = 0.35f;

    [SerializeField] private bool useRandomSeed = false;
    [SerializeField] private int seed = 12345;

    [Header("Ochrona okolicy ratusza")]
    [SerializeField] private Vector2Int townHallGridPosition = new Vector2Int(8, 8);
    [SerializeField] private BuildingData townHallData;
    [SerializeField] private int protectedRadiusInCells = 10;

    [Header("Wygl¹d")]
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(0.9f, 1.15f);

    private IEnumerator Start()
    {
        yield return null;
        PopulateTrees();
    }

    private void PopulateTrees()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("Brak GridManager.Instance. Nie mo¿na rozrzuciæ drzew.");
            return;
        }

        if (treePrefabs == null || treePrefabs.Count == 0)
        {
            Debug.LogWarning("Brak prefabów drzew w TreeMapPopulator.");
            return;
        }

        if (useRandomSeed)
            UnityEngine.Random.InitState(seed);

        for (int x = 0; x < GridManager.Instance.width; x++)
        {
            for (int z = 0; z < GridManager.Instance.height; z++)
            {
                if (IsProtectedTownHallArea(x, z))
                    continue;

                if (!GridManager.Instance.IsCellFree(x, z))
                    continue;

                if (UnityEngine.Random.value > density)
                    continue;

                SpawnTree(x, z);
            }
        }
    }

    private void SpawnTree(int x, int z)
    {
        GameObject prefab = treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Count)];

        if (prefab == null)
            return;

        Vector3 position = GridManager.Instance.GridToWorld(x, z);
        position.y = GetGroundHeight(position.x, position.z) + heightOffset;

        Quaternion rotation = Quaternion.identity;

        if (randomRotation)
            rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

        GameObject tree = Instantiate(prefab, position, rotation);

        float scale = UnityEngine.Random.Range(randomScaleRange.x, randomScaleRange.y);
        tree.transform.localScale *= scale;

        TreeResource treeResource = tree.GetComponent<TreeResource>();

        if (treeResource == null)
            treeResource = tree.AddComponent<TreeResource>();

        treeResource.Init(new Vector2Int(x, z), GridManager.Instance);

        GridManager.Instance.OccupyArea(x, z, 1, 1, tree);
    }

    private bool IsProtectedTownHallArea(int x, int z)
    {
        Vector2 center = GetTownHallCenter();
        Vector2 cell = new Vector2(x, z);

        return Vector2.Distance(cell, center) <= protectedRadiusInCells;
    }

    private Vector2 GetTownHallCenter()
    {
        Vector2Int size = Vector2Int.one;

        if (townHallData != null)
            size = townHallData.GridSize;

        return new Vector2(
            townHallGridPosition.x + (size.x - 1) * 0.5f,
            townHallGridPosition.y + (size.y - 1) * 0.5f
        );
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

        if (collider.GetComponentInParent<TreeResource>() != null)
            return true;

        return false;
    }
}