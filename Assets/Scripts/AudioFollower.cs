using UnityEngine;

public class AudioListenerFollower : MonoBehaviour
{
    public Transform cameraTransform;
    public float baseMaxDistance = 15f;
    public float minZoom = 15f;
    public float maxZoom = 70f;

    private AudioSource[] allSources;

    void Start()
    {
        allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
    }

    void Update()
    {
        Vector3 pos = cameraTransform.position;
        transform.position = new Vector3(pos.x, 0f, pos.z);

        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, pos.y);
        float newMaxDistance = Mathf.Lerp(baseMaxDistance, baseMaxDistance * 4f, zoomFactor);

        foreach (AudioSource src in allSources)
        {
            src.maxDistance = newMaxDistance;
        }
    }
}