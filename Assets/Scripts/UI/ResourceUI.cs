using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Header("Surowce")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI populationText;

    [Header("Czas")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI phaseText;

    [Header("Zagrożenie")]
    public TextMeshProUGUI threatText;
    public UnityEngine.UI.Image threatBar;

    void Start()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged.AddListener(UpdateResources);
            UpdateResources();
        }
    }

    void Update()
    {
        UpdateTime();
        UpdateThreat();
    }

    void UpdateResources()
    {
        ResourceManager r = ResourceManager.Instance;

        if (r == null)
            return;

        if (woodText != null)
            woodText.text = $"{r.wood}";

        if (foodText != null)
            foodText.text = $"{r.food}";

        if (metalText != null)
            metalText.text = $"{r.metal}";

        if (populationText != null)
            populationText.text = $"{r.population}/{r.maxPopulation}  Zajęci: {r.reservedPopulation}  Wolni: {r.FreePopulation}";
    }

    void UpdateTime()
    {
        if (DayManager.Instance == null)
            return;

        int day = DayManager.Instance.currentDay;
        bool isDay = DayManager.Instance.isDay;
        float progress = DayManager.Instance.GetDayProgress();

        float startHour = isDay ? 6f : 20f;
        float endHour = isDay ? 20f : 30f;
        float currentHour = Mathf.Lerp(startHour, endHour, progress) % 24f;

        int hour = Mathf.FloorToInt(currentHour);
        int minute = Mathf.FloorToInt((currentHour - hour) * 60f);

        if (dayText != null)
            dayText.text = $"DZIEŃ  {day}";

        if (timeText != null)
            timeText.text = $"{hour:00}:{minute:00}";

        if (phaseText != null)
            phaseText.text = isDay ? "DZIEŃ" : "NOC";
    }

    void UpdateThreat()
    {
        if (ThreatManager.Instance == null)
            return;

        float threat = ThreatManager.Instance.threatLevel;

        if (threatText != null)
            threatText.text = $"{Mathf.RoundToInt(threat)}%";

        if (threatBar != null)
        {
            threatBar.fillAmount = threat / 100f;
            threatBar.color = ThreatManager.Instance.GetThreatColor();
        }
    }
}