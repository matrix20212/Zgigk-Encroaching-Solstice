using UnityEngine;

[System.Serializable]
public class GridCell
{
    public int x, z;
    public bool isOccupied;
    public GameObject occupant;

    public GridCell(int x, int z)
    {
        this.x = x;
        this.z = z;
        isOccupied = false;
        occupant = null;
    }
}