using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Rozmiar siatki")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 2f;

    private GridCell[,] grid;

    void Awake()
    {
        Instance = this;
        InitGrid();
    }

    void InitGrid()
    {
        grid = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                grid[x, z] = new GridCell(x, z);
            }
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(int x, int z)
    {
        return new Vector3(
            x * cellSize + cellSize / 2f,
            0f,
            z * cellSize + cellSize / 2f
        );
    }

    public GridCell GetCell(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return null;
        return grid[x, z];
    }

    public bool IsCellFree(int x, int z)
    {
        GridCell cell = GetCell(x, z);
        return cell != null && !cell.isOccupied;
    }

    public bool IsAreaFree(int x, int z, int sizeX, int sizeZ)
    {
        for (int dx = 0; dx < sizeX; dx++)
            for (int dz = 0; dz < sizeZ; dz++)
                if (!IsCellFree(x + dx, z + dz)) return false;
        return true;
    }

    public void OccupyArea(int x, int z, int sizeX, int sizeZ, GameObject building)
    {
        for (int dx = 0; dx < sizeX; dx++)
        {
            for (int dz = 0; dz < sizeZ; dz++)
            {
                GridCell cell = GetCell(x + dx, z + dz);
                if (cell != null)
                {
                    cell.isOccupied = true;
                    cell.occupant = building;
                }
            }
        }
    }

    public void FreeArea(int x, int z, int sizeX, int sizeZ)
    {
        for (int dx = 0; dx < sizeX; dx++)
        {
            for (int dz = 0; dz < sizeZ; dz++)
            {
                GridCell cell = GetCell(x + dx, z + dz);
                if (cell != null)
                {
                    cell.isOccupied = false;
                    cell.occupant = null;
                }
            }
        }
    }
    public void ReleaseArea(Vector2Int start, Vector2Int size)
    {
        FreeArea(start.x, start.y, size.x, size.y);
    }

    // DEBUG
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.2f);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 center = new Vector3(
                    x * cellSize + cellSize / 2f, 0,
                    z * cellSize + cellSize / 2f
                );
                Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }
}