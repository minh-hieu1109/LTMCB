using UnityEngine;
using Mirror;

[RequireComponent(typeof(Camera))]
public class CameraFollow : NetworkBehaviour
{
    [Header("Target")]
    public Transform target; // sẽ tự gán nếu null

    [Header("Offset & Zoom")]
    public Vector3 offset = new Vector3(0, 2, -5);
    public float smoothSpeed = 0.125f;
    public float scrollSpeed = 2f;
    public float minZoom = 1f;
    public float maxZoom = 8f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public float returnSpeed = 2f;

    private float currentZoom;
    private float yaw;
    private float targetYaw; // dùng để quay về từ từ

    private bool followBehind = true;

    private Camera playerCamera;
    private AudioListener audioListener;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();

        if (!isLocalPlayer)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            enabled = false;
            return;
        }

        // Nếu target chưa gán thì mặc định chính là player
        if (target == null)
            target = transform.parent != null ? transform.parent : transform;

        currentZoom = offset.magnitude;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer || target == null)
            return;

        // Toggle camera mode
        if (Input.GetKeyDown(KeyCode.Y))
            followBehind = !followBehind;

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * scrollSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Xử lý góc xoay
        if (followBehind)
        {
            targetYaw = target.rotation.eulerAngles.y;
        }

        if (Input.GetMouseButton(1))
        {
            // Xoay tự do
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            targetYaw = yaw;
        }
        else if (!followBehind)
        {
            // Quay về từ từ hướng mặc định
            targetYaw = Mathf.LerpAngle(targetYaw, target.rotation.eulerAngles.y, Time.deltaTime * returnSpeed);
            yaw = targetYaw;
        }
        else
        {
            yaw = targetYaw;
        }

        Quaternion rotation = Quaternion.Euler(0, targetYaw, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, offset.y, -currentZoom);
        Vector3 desiredPosition = target.position + desiredOffset;

        // Raycast tránh chui tường
        RaycastHit hit;
        Vector3 direction = desiredOffset.normalized;
        float distance = desiredOffset.magnitude;

        if (Physics.Raycast(target.position, direction, out hit, distance))
        {
            desiredPosition = hit.point - direction * 0.2f;
        }

        // Smooth move
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Luôn nhìn vào target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
