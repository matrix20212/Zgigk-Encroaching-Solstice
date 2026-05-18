using UnityEngine;

public class BuildingInstance : MonoBehaviour
{
    public BuildingData data;
    public int gridX, gridZ;
    public int currentWorkers;

    public void Init(BuildingData d, int x, int z)
    {
        data = d;
        gridX = x;
        gridZ = z;
        currentWorkers = 0;
    }

    public void ProduceResources()
    {
        if (data.producedResource == ResourceType.None) return;
        int amount = currentWorkers * data.productionPerWorkerPerDay;
        ResourceManager.Instance.Add(data.producedResource, amount);
    }
}