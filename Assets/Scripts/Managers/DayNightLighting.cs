using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DayNightLighting : MonoBehaviour
{
    public static DayNightLighting Instance;

    [Header("S³oñce / Ksiê¿yc")]
    public Light sunLight;
    public Gradient sunColor;
    public AnimationCurve sunIntensity;

    [Header("Ambient (œwiat³o otoczenia)")]
    public Gradient ambientColor;

    [Header("Mg³a")]
    public Gradient fogColor;
    public AnimationCurve fogDensity;

    [Header("Post Processing")]
    public PostProcessVolume postVolume;
    private ColorGrading colorGrading;
    private Vignette vignette;

    public AnimationCurve colorTemperature;

    void Awake()
    {
        Instance = this;

        if (postVolume != null)
        {
            postVolume.profile.TryGetSettings(out colorGrading);
            postVolume.profile.TryGetSettings(out vignette);
        }

        SetupDefaultGradients();
    }

    public void UpdateLighting(float timeOfDay)
    {

        sunLight.color = sunColor.Evaluate(timeOfDay);
        sunLight.intensity = sunIntensity.Evaluate(timeOfDay);

        sunLight.transform.rotation = Quaternion.Euler(
            (timeOfDay * 360f) - 90f,
            -130f,
            0f
        );

        RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);

        RenderSettings.fogColor = fogColor.Evaluate(timeOfDay);
        RenderSettings.fogDensity = fogDensity.Evaluate(timeOfDay);

        if (colorGrading != null)
        {
            colorGrading.temperature.value = colorTemperature.Evaluate(timeOfDay);
            colorGrading.saturation.value = Mathf.Lerp(-30f, -10f,
                Mathf.Sin(timeOfDay * Mathf.PI)); // noc bardziej szara
        }

        if (vignette != null)
        {
            bool isNight = timeOfDay < 0.25f || timeOfDay > 0.75f;
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value,
                isNight ? 0.55f : 0.35f,
                Time.deltaTime * 2f
            );
        }
    }

    void SetupDefaultGradients()
    {
        sunColor = new Gradient();
        sunColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0.0f),
                new GradientColorKey(new Color(0.9f, 0.4f, 0.1f), 0.23f),
                new GradientColorKey(new Color(1.0f, 0.95f, 0.8f), 0.35f),
                new GradientColorKey(new Color(1.0f, 1.0f, 0.95f), 0.5f),
                new GradientColorKey(new Color(1.0f, 0.7f, 0.3f), 0.75f),
                new GradientColorKey(new Color(0.8f, 0.2f, 0.05f), 0.8f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1.0f), 
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );

        sunIntensity = new AnimationCurve(
            new Keyframe(0.0f, 0.0f), 
            new Keyframe(0.23f, 0.2f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(0.75f, 0.4f),
            new Keyframe(0.8f, 0.0f),
            new Keyframe(1.0f, 0.0f)
        );

        ambientColor = new Gradient();
        ambientColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.05f, 0.15f), 0.0f),  // noc
                new GradientColorKey(new Color(0.3f,  0.15f, 0.1f),  0.23f), // œwit
                new GradientColorKey(new Color(0.4f,  0.45f, 0.5f),  0.5f),  // dzieñ
                new GradientColorKey(new Color(0.2f,  0.1f,  0.05f), 0.78f), // zmierzch
                new GradientColorKey(new Color(0.05f, 0.05f, 0.15f), 1.0f),  // noc
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        fogColor = new Gradient();
        fogColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.05f, 0.06f, 0.12f), 0.0f),  // noc
                new GradientColorKey(new Color(0.6f,  0.35f, 0.2f),  0.23f), // œwit
                new GradientColorKey(new Color(0.55f, 0.6f,  0.65f), 0.5f),  // dzieñ
                new GradientColorKey(new Color(0.5f,  0.25f, 0.1f),  0.77f), // zmierzch
                new GradientColorKey(new Color(0.05f, 0.06f, 0.12f), 1.0f),  // noc
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        fogDensity = new AnimationCurve(
            new Keyframe(0.0f, 0.008f),
            new Keyframe(0.25f, 0.004f),
            new Keyframe(0.5f, 0.002f),
            new Keyframe(0.75f, 0.005f),
            new Keyframe(1.0f, 0.008f)
        );

        colorTemperature = new AnimationCurve(
            new Keyframe(0.0f, -25f),
            new Keyframe(0.23f, 10f),
            new Keyframe(0.5f, 5f),
            new Keyframe(0.75f, 20f),
            new Keyframe(1.0f, -25f)
        );
    }
}