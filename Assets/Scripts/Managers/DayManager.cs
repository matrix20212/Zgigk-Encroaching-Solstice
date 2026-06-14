using UnityEngine;
using UnityEngine.Events;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Czas")]
    public float dayDuration = 240f;
    public float nightDuration = 150f;

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

        float dayProgress = timer / phase;
        float timeOfDay = isDay
            ? Mathf.Lerp(0.25f, 0.75f, dayProgress)
            : Mathf.Lerp(0.75f, 1.25f, dayProgress) % 1f;

        if (DayNightLighting.Instance != null)
            DayNightLighting.Instance.UpdateLighting(timeOfDay);

        if (timer >= phase)
        {
            timer = 0f;

            if (isDay)
            {
                isDay = false;
                OnNightStart?.Invoke();
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

    public float GetDayProgress()
    {
        float phase = isDay ? dayDuration : nightDuration;
        return timer / phase;
    }
}