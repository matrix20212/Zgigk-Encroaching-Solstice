using UnityEngine;

public class DayNightLighting : MonoBehaviour
{
    public Light sunLight;

    [Header("Kolory oœwietlenia")]
    public Color dayColor = new Color(1f, 0.95f, 0.8f);
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f);

    [Header("Intensywnoœæ")]
    public float dayIntensity = 1.2f;
    public float nightIntensity = 0.2f;

    void Update()
    {
        bool isDay = DayManager.Instance.isDay;
        float t = DayManager.Instance.GetDayProgress();

        sunLight.color = Color.Lerp(isDay ? dayColor : nightColor,
                                        isDay ? nightColor : dayColor, t);
        sunLight.intensity = Mathf.Lerp(isDay ? dayIntensity : nightIntensity,
                                        isDay ? nightIntensity : dayIntensity, t);

        float angle = isDay ? Mathf.Lerp(30f, 170f, t) : Mathf.Lerp(190f, 350f, t);
        sunLight.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
    }
}