using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class chat : MonoBehaviour
{
    public GameObject chatPanel;
    public TMP_InputField inputField;

    void Update()
    {
        // Nếu không đang nhập vào InputField thì mới toggle
        if (Input.GetKeyDown(KeyCode.T) && EventSystem.current.currentSelectedGameObject != inputField.gameObject)
        {
            chatPanel.SetActive(!chatPanel.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            chatPanel.SetActive(true);
        }
    }
}