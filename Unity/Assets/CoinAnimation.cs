using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 90f; // độ xoay mỗi giây

    [Header("Bob Up & Down")]
    public float bobHeight = 0.25f;   // biên độ nhấp nhô
    public float bobSpeed = 2f;       // tốc độ nhấp nhô

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
