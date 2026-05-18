using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Zasoby startowe")]
    public int wood = 300;
    public int food = 180;
    public int metal = 100;
    public int population = 10;
    public int maxPopulation = 30;

    // Eventy
    public UnityEvent OnResourcesChanged;

    void Awake() => Instance = this;

    public bool CanAfford(int woodCost, int metalCost)
    {
        return wood >= woodCost && metal >= metalCost;
    }

    public void Spend(int woodCost, int metalCost)
    {
        wood -= woodCost;
        metal -= metalCost;
        OnResourcesChanged?.Invoke();
    }

    public void Add(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood: wood += amount; break;
            case ResourceType.Food: food += amount; break;
            case ResourceType.Metal: metal += amount; break;
        }
        OnResourcesChanged?.Invoke();
    }

    // Wywo³ywane ka¿dej nocy
    public void ConsumeFood()
    {
        food -= population;
        if (food < 0)
        {
            food = 0;
            // TODO: kara za brak jedzenia
            Debug.Log("Brak jedzenia! Populacja g³oduje.");
        }
        OnResourcesChanged?.Invoke();
    }
}