using UnityEngine;

public class GridClickTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float dist))
            {
                Vector3 worldPos = ray.GetPoint(dist);
                Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
                bool free = GridManager.Instance.IsCellFree(gridPos.x, gridPos.y);
                Debug.Log($"Klikniêto: œwiat={worldPos}, grid={gridPos}, wolne={free}");
            }
        }
    }
}