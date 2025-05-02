using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    void Update()
    {
        transform.LookAt(targetCamera.transform);
    }
}
