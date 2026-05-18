using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Ruch")]
    public float moveSpeed = 20f;
    public float fastMoveMultiplier = 2f;

    [Header("Zoom")]
    public float zoomSpeed = 200f;
    public float minZoom = 10f;
    public float maxZoom = 80f;

    [Header("Obrót")]
    public float rotationSpeed = 3f;

    [Header("K¹t kamery")]
    public float minPitch = 20f;
    public float maxPitch = 80f;

    private Camera cam;

    private float yaw = 45f;
    private float pitch = 45f;

    void Start()
    {
        cam = Camera.main;

        transform.position = new Vector3(0, 30, -30);

        UpdateRotation();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir += forward;
        if (Input.GetKey(KeyCode.S)) moveDir -= forward;
        if (Input.GetKey(KeyCode.D)) moveDir += right;
        if (Input.GetKey(KeyCode.A)) moveDir -= right;

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= fastMoveMultiplier;

        transform.position += moveDir.normalized * speed * Time.deltaTime;
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            UpdateRotation();
        }
    }

    void UpdateRotation()
    {
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (cam.orthographic)
        {
            cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize,
                minZoom,
                maxZoom
            );
        }
        else
        {
            transform.position += transform.forward * scroll * zoomSpeed * Time.deltaTime;
        }
    }
}