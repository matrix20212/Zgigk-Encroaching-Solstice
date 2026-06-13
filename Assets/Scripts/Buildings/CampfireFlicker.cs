using UnityEngine;

public class CampfireFlicker : MonoBehaviour
{
    public Light fireLight;
    public float minIntensity = 2.5f;
    public float maxIntensity = 4f;
    public float flickerSpeed = 8f;

    private float baseIntensity;

    void Start() => baseIntensity = fireLight.intensity;

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        float warmth = Mathf.PerlinNoise(0f, Time.time * flickerSpeed * 0.5f);
        fireLight.color = Color.Lerp(
            new Color(1f, 0.5f, 0.1f),
            new Color(1f, 0.8f, 0.3f),
            warmth
        );
    }
}