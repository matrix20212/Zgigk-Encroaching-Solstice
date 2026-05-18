using UnityEngine;

public class TestBuildTrigger : MonoBehaviour
{
    public BuildingData tartakData;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            BuildingManager.Instance.StartBuilding(tartakData);
    }
}