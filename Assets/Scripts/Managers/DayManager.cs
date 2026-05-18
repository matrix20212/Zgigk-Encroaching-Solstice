using UnityEngine;
using UnityEngine.Events;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Czas")]
    public float dayDuration = 240f;   // 4 minuty = 1 dzieñ
    public float nightDuration = 150f; // 2.5 minuty = 1 noc

    public int currentDay = 1;
    public bool isDay = true;

    public UnityEvent OnDayStart;
    public UnityEvent OnNightStart;
    public UnityEvent OnNewDay;

    private float timer;

    void Awake() => Instance = this;

    void Update()
    {
        timer += Time.deltaTime;
        float phase = isDay ? dayDuration : nightDuration;

        if (timer >= phase)
        {
            timer = 0f;
            if (isDay)
            {
                isDay = false;
                OnNightStart?.Invoke();
                EndDay();
            }
            else
            {
                isDay = true;
                currentDay++;
                OnDayStart?.Invoke();
                OnNewDay?.Invoke();
            }
        }
    }

    void EndDay()
    {
        // Wszystkie budynki produkuj¹ surowce
        foreach (var b in FindObjectsOfType<BuildingInstance>())
            b.ProduceResources();

        // Zu¿ycie jedzenia
        ResourceManager.Instance.ConsumeFood();
    }

    public float GetDayProgress()
    {
        float phase = isDay ? dayDuration : nightDuration;
        return timer / phase;
    }
}