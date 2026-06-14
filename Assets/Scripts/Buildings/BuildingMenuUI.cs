using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panelObject;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI statsText;
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

        if (data.role == BuildingRole.Production)
        {
            text +=
                $"\nProdukuje: {GetResourceName(data.producedResource)}" +
                $"\nIloœæ: {data.productionAmount}" +
                $"\nInterwa³: {data.productionInterval:0.##}s";
        }

        if (data.role == BuildingRole.Defensive)
        {
            text +=
                $"\nObra¿enia: {data.attackDamage}" +
                $"\nZasiêg: {data.attackRange:0.##}" +
                $"\nCooldown: {data.attackCooldown:0.##}s";
        }

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
        panelImage.color = new Color(0f, 0f, 0f, 0.78f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20f, 0f);
        panelRect.sizeDelta = new Vector2(300f, 330f);

        titleText = CreateText("Title", panelObject.transform, new Vector2(0f, 120f), new Vector2(260f, 40f), 24);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        statsText = CreateText("Stats", panelObject.transform, new Vector2(0f, 5f), new Vector2(260f, 190f), 17);
        statsText.alignment = TextAlignmentOptions.TopLeft;

        deleteButton = CreateButton("DeleteButton", panelObject.transform, "Usuñ budynek", new Vector2(0f, -90f), new Vector2(220f, 42f));
        deleteButton.onClick.AddListener(DeleteCurrentBuilding);

        closeButton = CreateButton("CloseButton", panelObject.transform, "Zamknij", new Vector2(0f, -135f), new Vector2(220f, 36f));
        closeButton.onClick.AddListener(Hide);
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.text = "";

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI buttonText = CreateText("Text", buttonObject.transform, Vector2.zero, size, 18);
        buttonText.text = label;
        buttonText.alignment = TextAlignmentOptions.Center;

        return button;
    }
}