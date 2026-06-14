using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panelObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI statsText;
    private RectTransform statsTextRect;
    private Button deleteButton;
    private Button closeButton;

    private BuildingInstance currentBuilding;

    private void Awake()
    {
        CreateUiIfNeeded();
        Hide();
    }

    private void Update()
    {
        if (currentBuilding == null || !currentBuilding.IsAlive)
        {
            Hide();
            return;
        }

        UpdateTexts();
    }

    public void Show(BuildingInstance building)
    {
        currentBuilding = building;

        CreateUiIfNeeded();

        panelObject.SetActive(true);
        deleteButton.gameObject.SetActive(building.CanBeManuallyDeleted);

        UpdateTexts();
    }

    public void Hide()
    {
        currentBuilding = null;

        if (panelObject != null)
            panelObject.SetActive(false);
    }

    private void DeleteCurrentBuilding()
    {
        if (currentBuilding == null)
            return;

        if (!currentBuilding.CanBeManuallyDeleted)
            return;

        BuildingInstance buildingToDelete = currentBuilding;
        Hide();
        buildingToDelete.RemoveByPlayer();
    }

    private void UpdateTexts()
    {
        if (currentBuilding == null || currentBuilding.Data == null)
            return;

        BuildingData data = currentBuilding.Data;

        titleText.text = string.IsNullOrWhiteSpace(data.buildingName)
            ? "Budynek"
            : data.buildingName;

        statsText.text = BuildStatsText(currentBuilding, data);
        deleteButton.gameObject.SetActive(currentBuilding.CanBeManuallyDeleted);

        statsText.ForceMeshUpdate();
        float height = Mathf.Max(380f, statsText.preferredHeight + 20f);
        statsTextRect.sizeDelta = new Vector2(0f, height);
    }

    private string BuildStatsText(BuildingInstance building, BuildingData data)
    {
        string roleText = data.role == BuildingRole.Production ? "Produkcyjny" : "Defensywny";

        string text =
            $"HP: {building.CurrentHp}/{building.MaxHp}\n" +
            $"Rola: {roleText}\n" +
            $"Rozmiar: {data.sizeX}x{data.sizeZ}\n" +
            $"Koszt: drewno {data.woodCost}, metal {data.metalCost}";

        if (data.foodCost > 0)
            text += $", jedzenie {data.foodCost}";

        text += BuildWorkersText(building);

        if (data.populationCapacityBonus > 0)
        {
            text +=
                "\n\n<b><color=#FFD36A>Populacja</color></b>" +
                $"\nLimit populacji: +{data.populationCapacityBonus}";
        }

        if (data.producesPopulation)
            text += BuildPopulationProductionText(building, data);

        if (HasResourceProduction(data))
            text += BuildProductionText(building, data);

        if (data.clearTreesInRange)
            text += BuildTreeClearingText(building, data);

        if (data.role == BuildingRole.Defensive)
            text += BuildDefenseText(building, data);

        if (!building.CanBeManuallyDeleted)
            text += "\n\n<color=#BBBBBB>Nie mo¿na usun¹æ rêcznie</color>";

        return text;
    }

    private string BuildWorkersText(BuildingInstance building)
    {
        string text = "\n\n<b><color=#FFD36A>Pracownicy</color></b>";

        if (building.RequiredWorkers <= 0)
            return text + "\nNie wymaga";

        text += $"\nObsada: {building.AssignedWorkers}/{building.RequiredWorkers}";

        if (building.AssignedWorkers <= 0)
            text += "\nStatus: stoi";
        else if (building.AssignedWorkers < building.RequiredWorkers)
            text += $"\nSpowolnienie: x{building.WorkSlowdownMultiplier:0.##}";
        else
            text += "\nSpowolnienie: brak";

        return text;
    }

    private string BuildPopulationProductionText(BuildingInstance building, BuildingData data)
    {
        float interval = building.GetAdjustedInterval(data.populationProductionInterval);

        string text =
            "\n\n<b><color=#FFD36A>Tworzenie ludzi</color></b>" +
            $"\nIloœæ: {data.populationProductionAmount}";

        if (interval < 0f)
            text += "\nInterwa³: stoi";
        else
            text += $"\nInterwa³: {interval:0.##}s";

        return text;
    }

    private bool HasResourceProduction(BuildingData data)
    {
        return data.productionAmount > 0 || data.productionPerWorkerPerDay > 0;
    }

    private string BuildProductionText(BuildingInstance building, BuildingData data)
    {
        float interval = building.GetAdjustedInterval(data.productionInterval);

        string text =
            "\n\n<b><color=#FFD36A>Produkcja surowców</color></b>" +
            $"\nTyp: {GetResourceName(data.producedResource)}" +
            $"\nIloœæ: {data.productionAmount}";

        if (interval < 0f)
            text += "\nInterwa³: stoi";
        else
            text += $"\nInterwa³: {interval:0.##}s";

        return text;
    }

    private string BuildTreeClearingText(BuildingInstance building, BuildingData data)
    {
        float interval = building.GetAdjustedInterval(data.treeClearInterval);

        string text =
            "\n\n<b><color=#FFD36A>Wycinka drzew</color></b>" +
            $"\nZasiêg: {data.treeClearRange:0.##}";

        if (interval < 0f)
            text += "\nInterwa³: stoi";
        else
            text += $"\nInterwa³: {interval:0.##}s";

        return text;
    }

    private string BuildDefenseText(BuildingInstance building, BuildingData data)
    {
        float cooldown = building.GetAdjustedInterval(data.attackCooldown);

        string text =
            "\n\n<b><color=#FFD36A>Obrona</color></b>" +
            $"\nObra¿enia: {data.attackDamage}" +
            $"\nZasiêg: {data.attackRange:0.##}";

        if (cooldown < 0f)
            text += "\nCooldown: stoi";
        else
            text += $"\nCooldown: {cooldown:0.##}s";

        return text;
    }

    private string GetResourceName(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood:
                return "Drewno";
            case ResourceType.Food:
                return "Jedzenie";
            case ResourceType.Metal:
                return "Metal";
            case ResourceType.Population:
                return "Populacja";
            default:
                return type.ToString();
        }
    }

    private void CreateUiIfNeeded()
    {
        if (panelObject != null)
            return;

        Canvas existingCanvas = FindFirstObjectByType<Canvas>();

        if (existingCanvas != null)
        {
            canvas = existingCanvas;
        }
        else
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        panelObject = new GameObject("BuildingMenuPanel");
        panelObject.transform.SetParent(canvas.transform, false);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.86f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20f, 0f);
        panelRect.sizeDelta = new Vector2(380f, 560f);

        titleText = CreateTitleText(panelObject.transform);

        ScrollRect scrollRect = CreateStatsScroll(panelObject.transform);
        statsText = scrollRect.content.GetComponent<TextMeshProUGUI>();
        statsTextRect = scrollRect.content;

        deleteButton = CreateButton("DeleteButton", panelObject.transform, "Usuñ budynek", new Vector2(0f, 62f), new Vector2(280f, 42f));
        deleteButton.onClick.AddListener(DeleteCurrentBuilding);

        closeButton = CreateButton("CloseButton", panelObject.transform, "Zamknij", new Vector2(0f, 16f), new Vector2(280f, 38f));
        closeButton.onClick.AddListener(Hide);
    }

    private TextMeshProUGUI CreateTitleText(Transform parent)
    {
        GameObject textObject = new GameObject("Title");
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 28;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -14f);
        rect.sizeDelta = new Vector2(-40f, 42f);

        return text;
    }

    private ScrollRect CreateStatsScroll(Transform parent)
    {
        GameObject viewportObject = new GameObject("StatsViewport");
        viewportObject.transform.SetParent(parent, false);

        RectTransform viewportRect = viewportObject.AddComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0f, 0f);
        viewportRect.anchorMax = new Vector2(1f, 1f);
        viewportRect.offsetMin = new Vector2(18f, 110f);
        viewportRect.offsetMax = new Vector2(-18f, -64f);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0f);

        viewportObject.AddComponent<RectMask2D>();

        GameObject contentObject = new GameObject("StatsText");
        contentObject.transform.SetParent(viewportObject.transform, false);

        TextMeshProUGUI text = contentObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 15;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.richText = true;
        text.lineSpacing = 2f;
        text.text = "";

        RectTransform contentRect = text.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 380f);

        ScrollRect scrollRect = viewportObject.AddComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false;
        scrollRect.scrollSensitivity = 22f;

        return scrollRect;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 bottomPosition, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = bottomPosition;
        rect.sizeDelta = size;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(buttonObject.transform, false);

        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.fontSize = 18;
        buttonText.color = Color.white;
        buttonText.text = label;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.enableWordWrapping = false;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }
}