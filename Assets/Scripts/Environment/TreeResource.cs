using System.Collections.Generic;
using UnityEngine;

public class TreeResource : MonoBehaviour
{
    private static readonly HashSet<TreeResource> trees = new HashSet<TreeResource>();

    private Vector2Int gridPosition;
    private GridManager gridManager;
    private bool removed = false;

    public Vector2Int GridPosition => gridPosition;
    public bool IsRemoved => removed;

    private void OnEnable()
    {
        trees.Add(this);
    }

    private void OnDisable()
    {
        trees.Remove(this);
    }

    public void Init(Vector2Int position, GridManager grid)
    {
        gridPosition = position;
        gridManager = grid;
    }

    public void RemoveFromMap()
    {
        if (removed)
            return;

        removed = true;

        if (gridManager != null)
            gridManager.FreeArea(gridPosition.x, gridPosition.y, 1, 1);

        Destroy(gameObject);
    }

    public static TreeResource GetNearest(Vector3 position, float range)
    {
        TreeResource nearest = null;
        float bestDistance = range * range;

        foreach (TreeResource tree in trees)
        {
            if (tree == null || tree.removed)
                continue;

            float distance = (tree.transform.position - position).sqrMagnitude;

            if (distance <= bestDistance)
            {
                bestDistance = distance;
                nearest = tree;
            }
        }

        return nearest;
    }
}