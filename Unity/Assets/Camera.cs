using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviourPun
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
    private bool followBehind = false;

    private Camera playerCamera;
    private AudioListener audioListener;

    void Start()
    {
        playerCamera = GetComponent<Camera>();
        audioListener = GetComponent<AudioListener>();

        if (!photonView.IsMine)
        {
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
        }

        currentZoom = offset.magnitude;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine || target == null) return;

        // Ấn Y để bật chế độ follow phía sau nhân vật
        if (Input.GetKeyDown(KeyCode.Y))
        {
            followBehind = !followBehind; // Toggle chế độ theo sau
        }

        // Zoom với lăn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * scrollSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Nếu đang trong chế độ theo sau
        if (followBehind)
        {
            yaw = target.rotation.eulerAngles.y;
        }
        else if (Input.GetMouseButton(1)) // Nếu giữ chuột phải, người chơi được điều khiển camera
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
        }

        // Tính offset theo yaw
        Quaternion rotation = Quaternion.Euler(0, yaw, 0);
        Vector3 desiredOffset = rotation * new Vector3(0, offset.y, -currentZoom);
        Vector3 desiredPosition = target.position + desiredOffset;

        // Xử lý va chạm
        RaycastHit hit;
        Vector3 direction = desiredOffset.normalized;
        float distance = desiredOffset.magnitude;

        if (Physics.Raycast(target.position, direction, out hit, distance))
        {
            desiredPosition = hit.point - direction * 0.2f;
        }

        // Di chuyển và xoay camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
