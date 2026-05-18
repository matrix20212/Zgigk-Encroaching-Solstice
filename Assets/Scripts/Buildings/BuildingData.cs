using UnityEngine;

[CreateAssetMenu(menuName = "Game/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Podstawowe")]
    public string buildingName;
    public GameObject prefab;
    public Sprite icon;

    [Header("Rozmiar na gridzie")]
    public int sizeX = 1;
    public int sizeZ = 1;

    [Header("Koszty budowy")]
    public int woodCost;
    public int metalCost;

    [Header("Produkcja / funkcja")]
    public int maxWorkers;
    public ResourceType producedResource;
    public int productionPerWorkerPerDay;
}

public enum ResourceType { None, Wood, Food, Metal }