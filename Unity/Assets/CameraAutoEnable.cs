using UnityEngine;

public class CameraAutoEnable : MonoBehaviour
{
    void Update()
    {
        if (Camera.allCamerasCount == 0)
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                Debug.Log("No cameras detected, enabling backup camera.");
            }
        }
        else
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
