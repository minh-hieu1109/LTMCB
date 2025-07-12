using UnityEngine;

public class BoxUI : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 45f; 

    [Header("Floating")]
    public float floatAmplitude = 0.25f; 
    public float floatFrequency = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float newY = startPos.y + Mathf.Sin(Time.time * Mathf.PI * 2f * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
