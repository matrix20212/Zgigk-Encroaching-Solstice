using UnityEngine;
using UnityEngine.UI;

public class BuildingHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private Vector2 size = new Vector2(100f, 10f);
    [SerializeField] private float worldScale = 0.01f;

    private BuildingInstance building;
    private Canvas canvas;
    private RectTransform fillRect;
    private Renderer[] renderers;

    public void Init(BuildingInstance target)
    {
        building = target;
        renderers = GetComponentsInChildren<Renderer>();

        if (canvas == null)
            CreateBar();
    }

    private void LateUpdate()
    {
        if (building == null || !building.IsAlive)
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);

            return;
        }

        float percent = building.HealthPercent;
        bool shouldShow = percent < 0.999f && percent > 0f;

        if (canvas != null)
            canvas.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        UpdatePosition();
        UpdateFill(percent);
        FaceCamera();
    }

    private void CreateBar()
    {
        GameObject canvasObject = new GameObject("BuildingHealthBar");
        canvasObject.transform.SetParent(transform);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = size;
        canvasRect.localScale = Vector3.one * worldScale;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(canvasObject.transform, false);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);

        Image fill = fillObject.AddComponent<Image>();
        fill.color = new Color(0.1f, 0.9f, 0.1f, 1f);

        fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        canvasObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (canvas == null)
            return;

        Vector3 position = transform.position + offset;

        Bounds bounds;
        if (TryGetBounds(out bounds))
            position = bounds.center + Vector3.up * (bounds.extents.y + 0.5f);

        canvas.transform.position = position;
    }

    private bool TryGetBounds(out Bounds bounds)
    {
        bounds = new Bounds(transform.position, Vector3.zero);

        if (renderers == null || renderers.Length == 0)
            return false;

        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return found;
    }

    private void UpdateFill(float percent)
    {
        if (fillRect == null)
            return;

        fillRect.anchorMax = new Vector2(Mathf.Clamp01(percent), 1f);
    }

    private void FaceCamera()
    {
        if (canvas == null || Camera.main == null)
            return;

        Transform cameraTransform = Camera.main.transform;
        canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - cameraTransform.position);
    }
}