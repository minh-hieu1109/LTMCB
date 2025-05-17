using UnityEngine;
using Mirror;

[RequireComponent(typeof(Camera))]
public class CameraFollow : NetworkBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float smoothSpeed = 0.125f;
    public float scrollSpeed = 2f;
    public float minZoom = 0.5f;
    public float maxZoom = 5f;
    public float rotationSpeed = 5f;

    private float currentZoom = 5f;
    private float yaw = 0f;
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

        currentZoom = offset.magnitude;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer || target == null) return;

        if (Input.GetKeyDown(KeyCode.Y))
            followBehind = !followBehind;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * scrollSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (followBehind)
            yaw = target.rotation.eulerAngles.y;
        else if (Input.GetMouseButton(1))
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;

        Quaternion rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, offset.y, -currentZoom);
        Vector3 desiredPosition = target.position + desiredOffset;

        RaycastHit hit;
        Vector3 direction = desiredOffset.normalized;
        float distance = desiredOffset.magnitude;

        if (Physics.Raycast(target.position, direction, out hit, distance))
            desiredPosition = hit.point - direction * 0.2f;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
