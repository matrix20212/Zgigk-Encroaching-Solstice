using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Ruch kamery")]
    public float edgeScrollSpeed = 25f;
    public float dragPanSpeed = 0.6f;
    public float movementSmoothing = 12f;

    [Header("Edge scrolling")]
    public bool useEdgeScrolling = true;
    public float edgeSize = 20f;

    [Header("Zoom")]
    public float zoomSpeed = 8f;
    public float minZoom = 15f;
    public float maxZoom = 70f;
    public float zoomSmoothing = 12f;

    [Header("Obrót opcjonalny")]
    public bool allowRotation = false;
    public float rotationSpeed = 90f;

    [Header("K¹t kamery")]
    public float pitch = 55f;
    public float yaw = 45f;

    [Header("Granice mapy")]
    public bool useMapBounds = true;
    public Vector2 minBounds = new Vector2(-100f, -100f);
    public Vector2 maxBounds = new Vector2(100f, 100f);

    private Camera cam;

    private Vector3 targetPosition;
    private float targetZoom;

    private Vector3 lastMousePosition;

    void Start()
    {
        cam = Camera.main;

        targetPosition = new Vector3(0f, 50f, -50f);
        transform.position = targetPosition;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (cam != null && cam.orthographic)
        {
            targetZoom = cam.orthographicSize;
        }
        else
        {
            targetZoom = transform.position.y;
        }
    }

    void Update()
    {
        HandleEdgeScrolling();
        HandleMouseDrag();
        HandleZoom();
        HandleRotation();

        ApplyCameraMovement();
    }

    void HandleEdgeScrolling()
    {
        if (!useEdgeScrolling)
            return;

        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x < 0 || mousePos.x > Screen.width ||
            mousePos.y < 0 || mousePos.y > Screen.height)
            return;

        Vector3 moveDir = Vector3.zero;

        Vector3 forward = GetCameraForwardOnGround();
        Vector3 right = GetCameraRightOnGround();

        if (mousePos.y >= Screen.height - edgeSize)
            moveDir += forward;

        if (mousePos.y <= edgeSize)
            moveDir -= forward;

        if (mousePos.x >= Screen.width - edgeSize)
            moveDir += right;

        if (mousePos.x <= edgeSize)
            moveDir -= right;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();

            float zoomFactor = GetZoomFactor();

            targetPosition += moveDir * edgeScrollSpeed * zoomFactor * Time.deltaTime;
            ClampTargetPosition();
        }
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            Vector3 right = GetCameraRightOnGround();
            Vector3 forward = GetCameraForwardOnGround();

            float zoomFactor = GetZoomFactor();

            Vector3 move =
                (-right * delta.x - forward * delta.y)
                * dragPanSpeed
                * zoomFactor
                * Time.deltaTime;

            targetPosition += move;

            lastMousePosition = Input.mousePosition;

            ClampTargetPosition();
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) < 0.001f)
            return;

        if (cam != null && cam.orthographic)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        else
        {
            targetPosition.y -= scroll * zoomSpeed * 5f;
            targetPosition.y = Mathf.Clamp(targetPosition.y, minZoom, maxZoom);

            ClampTargetPosition();
        }
    }

    void HandleRotation()
    {
        if (!allowRotation)
            return;

        float rotationInput = 0f;

        if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f;

        if (Input.GetKey(KeyCode.E))
            rotationInput = 1f;

        if (Mathf.Abs(rotationInput) < 0.001f)
            return;

        float angle = rotationInput * rotationSpeed * Time.deltaTime;

        RotateAroundLookPoint(angle);
    }

    void RotateAroundLookPoint(float angle)
    {
        Vector3 pivot = GetLookPointOnGround();

        yaw += angle;
        UpdateRotation();

        Vector3 directionFromPivot = targetPosition - pivot;

        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        directionFromPivot = rotation * directionFromPivot;

        targetPosition = pivot + directionFromPivot;

        ClampTargetPosition();
    }

    Vector3 GetLookPointOnGround()
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = new Ray(targetPosition, transform.forward);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return targetPosition + GetCameraForwardOnGround() * 20f;
    }

    void ApplyCameraMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            movementSmoothing * Time.deltaTime
        );

        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                zoomSmoothing * Time.deltaTime
            );
        }
    }

    void UpdateRotation()
    {
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    Vector3 GetCameraForwardOnGround()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    Vector3 GetCameraRightOnGround()
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }

    float GetZoomFactor()
    {
        float currentZoom;

        if (cam != null && cam.orthographic)
            currentZoom = cam.orthographicSize;
        else
            currentZoom = transform.position.y;

        return Mathf.Clamp(currentZoom / minZoom, 1f, 3f);
    }

    void ClampTargetPosition()
    {
        if (!useMapBounds)
            return;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
    }
}