using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
