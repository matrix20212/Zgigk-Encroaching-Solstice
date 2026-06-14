using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Zasoby startowe")]
    public int wood = 300;
    public int food = 180;
    public int metal = 100;

    [Header("Populacja")]
    public int population = 10;
    public int maxPopulation = 30;
    public int reservedPopulation = 0;

    [Header("Tworzenie ludzi")]
    public int minimumFoodToCreatePopulation = 15;
    public int foodForNormalPopulationGrowth = 50;
    public int foodForFastPopulationGrowth = 150;
    public float lowFoodPopulationIntervalMultiplier = 2f;
    public float highFoodPopulationIntervalMultiplier = 0.5f;

    [Header("Jedzenie")]
    public float foodConsumptionInterval = 10f;
    public int foodPerPerson = 1;
    public int starvationPopulationLoss = 1;

    public UnityEvent OnResourcesChanged;

    private readonly List<BuildingInstance> workerBuildings = new List<BuildingInstance>();
    private Coroutine foodRoutine;

    public int FreePopulation => Mathf.Max(0, population - reservedPopulation);

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foodRoutine = StartCoroutine(FoodConsumptionLoop());
        RebalanceWorkers();
        OnResourcesChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (foodRoutine != null)
            StopCoroutine(foodRoutine);
    }

    public bool CanAfford(int woodCost, int metalCost)
    {
        return wood >= woodCost && metal >= metalCost;
    }

    public bool CanAfford(BuildingData data)
    {
        if (data == null)
            return false;

        return wood >= data.woodCost &&
               food >= data.foodCost &&
               metal >= data.metalCost;
    }

    public void Spend(int woodCost, int metalCost)
    {
        wood -= woodCost;
        metal -= metalCost;

        wood = Mathf.Max(0, wood);
        metal = Mathf.Max(0, metal);

        OnResourcesChanged?.Invoke();
    }

    public void Spend(BuildingData data)
    {
        if (data == null)
            return;

        wood -= data.woodCost;
        food -= data.foodCost;
        metal -= data.metalCost;

        wood = Mathf.Max(0, wood);
        food = Mathf.Max(0, food);
        metal = Mathf.Max(0, metal);

        OnResourcesChanged?.Invoke();
    }

    public void Add(ResourceType type, int amount)
    {
        AddResource(type, amount);
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
            return;

        switch (type)
        {
            case ResourceType.Wood:
                wood += amount;
                break;
            case ResourceType.Food:
                food += amount;
                break;
            case ResourceType.Metal:
                metal += amount;
                break;
            case ResourceType.Population:
                AddPopulation(amount);
                return;
        }

        OnResourcesChanged?.Invoke();
    }

    public bool CanCreatePopulation()
    {
        if (population >= maxPopulation)
            return false;

        if (food < minimumFoodToCreatePopulation)
            return false;

        return true;
    }

    public void AddPopulation(int amount)
    {
        if (amount <= 0)
            return;

        if (!CanCreatePopulation())
            return;

        population = Mathf.Min(maxPopulation, population + amount);
        RebalanceWorkers();
        OnResourcesChanged?.Invoke();
    }

    public float GetPopulationProductionIntervalMultiplier()
    {
        if (food < minimumFoodToCreatePopulation)
            return -1f;

        int normalFood = Mathf.Max(minimumFoodToCreatePopulation + 1, foodForNormalPopulationGrowth);
        int fastFood = Mathf.Max(normalFood + 1, foodForFastPopulationGrowth);

        float lowMultiplier = Mathf.Max(1f, lowFoodPopulationIntervalMultiplier);
        float fastMultiplier = Mathf.Clamp(highFoodPopulationIntervalMultiplier, 0.1f, 1f);

        if (food < normalFood)
        {
            float t = Mathf.InverseLerp(minimumFoodToCreatePopulation, normalFood, food);
            return Mathf.Lerp(lowMultiplier, 1f, t);
        }

        float fastT = Mathf.InverseLerp(normalFood, fastFood, food);
        return Mathf.Lerp(1f, fastMultiplier, fastT);
    }

    public float GetPopulationProductionInterval(float baseInterval)
    {
        baseInterval = Mathf.Max(0.1f, baseInterval);

        float multiplier = GetPopulationProductionIntervalMultiplier();

        if (multiplier < 0f)
            return -1f;

        return Mathf.Max(0.1f, baseInterval * multiplier);
    }

    public void ChangePopulationCapacity(int amount)
    {
        maxPopulation = Mathf.Max(0, maxPopulation + amount);

        if (population > maxPopulation)
            population = maxPopulation;

        RebalanceWorkers();
        OnResourcesChanged?.Invoke();
    }

    public void RegisterWorkerBuilding(BuildingInstance building)
    {
        if (building == null)
            return;

        if (building.RequiredWorkers <= 0)
            return;

        if (!workerBuildings.Contains(building))
            workerBuildings.Add(building);

        RebalanceWorkers();
    }

    public void UnregisterWorkerBuilding(BuildingInstance building)
    {
        if (building == null)
            return;

        workerBuildings.Remove(building);
        RebalanceWorkers();
    }

    public void RebalanceWorkers()
    {
        reservedPopulation = 0;
        int remainingPeople = population;

        for (int i = workerBuildings.Count - 1; i >= 0; i--)
        {
            if (workerBuildings[i] == null || !workerBuildings[i].IsAlive)
                workerBuildings.RemoveAt(i);
        }

        foreach (BuildingInstance building in workerBuildings)
        {
            int required = building.RequiredWorkers;
            int assigned = Mathf.Min(required, remainingPeople);

            building.SetAssignedWorkers(assigned);

            reservedPopulation += assigned;
            remainingPeople -= assigned;

            if (remainingPeople < 0)
                remainingPeople = 0;
        }

        OnResourcesChanged?.Invoke();
    }

    private IEnumerator FoodConsumptionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, foodConsumptionInterval));
            ConsumeFood();
        }
    }

    public void ConsumeFood()
    {
        if (population <= 0)
        {
            OnResourcesChanged?.Invoke();
            return;
        }

        int neededFood = population * Mathf.Max(1, foodPerPerson);

        if (food >= neededFood)
        {
            food -= neededFood;
        }
        else
        {
            food = 0;

            int loss = Mathf.Clamp(starvationPopulationLoss, 1, population);
            population -= loss;

            RebalanceWorkers();

            Debug.Log("Brak jedzenia! Populacja maleje.");
        }

        OnResourcesChanged?.Invoke();
    }
    public void KillPopulation(int amount)
    {
        if (amount <= 0)
            return;

        population = Mathf.Max(0, population - amount);
        RebalanceWorkers();
        OnResourcesChanged?.Invoke();
    }
}