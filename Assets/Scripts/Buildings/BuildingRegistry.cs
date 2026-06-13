using System.Collections.Generic;
using UnityEngine;

public static class BuildingRegistry
{
    private static readonly HashSet<BuildingInstance> buildings = new HashSet<BuildingInstance>();

    public static IReadOnlyCollection<BuildingInstance> Buildings => buildings;

    public static void Register(BuildingInstance building)
    {
        if (building != null)
            buildings.Add(building);
    }

    public static void Unregister(BuildingInstance building)
    {
        if (building != null)
            buildings.Remove(building);
    }

    public static BuildingInstance GetNearest(Vector3 position)
    {
        BuildingInstance nearest = null;
        float bestDistance = float.MaxValue;

        foreach (var building in buildings)
        {
            if (building == null || !building.IsAlive)
                continue;

            float distance = (building.transform.position - position).sqrMagnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = building;
            }
        }

        return nearest;
    }
}
