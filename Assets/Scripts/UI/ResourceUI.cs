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
    public TextMeshProUGUI timeText;      // "Godzina"
    public UnityEngine.UI.Image phaseIcon;
    public Sprite sunSprite;
    public Sprite moonSprite;

    [Header("Zagrożenie")]
    public TextMeshProUGUI threatText;    // "62%"
    public UnityEngine.UI.Image threatBar; // pasek postępu

    void Start()
    {
        ResourceManager.Instance.OnResourcesChanged.AddListener(UpdateResources);
        UpdateResources();
    }

    void Update()
    {
        UpdateTime();
        UpdateThreat();
    }

    void UpdateResources()
    {
        var r = ResourceManager.Instance;
        woodText.text = $"{r.wood}";
        foodText.text = $"{r.food}";
        metalText.text = $"{r.metal}";
        populationText.text = $"{r.population}/{r.maxPopulation}";
    }

    void UpdateTime()
    {
        if (DayManager.Instance == null) return;

        int day = DayManager.Instance.currentDay;
        bool isDay = DayManager.Instance.isDay;
        float progress = DayManager.Instance.GetDayProgress();

        // Symulacja godziny 6:00-20:00, noc 20:00-6:00
        float startHour = isDay ? 6f : 20f;
        float endHour = isDay ? 20f : 30f;
        float currentHour = Mathf.Lerp(startHour, endHour, progress) % 24f;

        int hour = Mathf.FloorToInt(currentHour);
        int minute = Mathf.FloorToInt((currentHour - hour) * 60f);

        if (dayText != null) dayText.text = $"DZIEŃ  {day}";
        if (timeText != null) timeText.text = $"{hour:00}:{minute:00}";
        if (phaseIcon != null)
        {
            phaseIcon.sprite = isDay ? sunSprite : moonSprite;
        }
    }

    void UpdateThreat()
    {
        if (ThreatManager.Instance == null) return;

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