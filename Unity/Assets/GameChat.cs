using TMPro;
using UnityEngine;
using WebSocketSharp;
using Photon.Pun;
public class GameChat : MonoBehaviour
{
    
    public TextMeshProUGUI chatText;
    public TMP_InputField inputField;

    private bool isInputFieldToggled;
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!isInputFieldToggled)
            {
                isInputFieldToggled = true;
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInputFieldToggled)
            {
                isInputFieldToggled = false;

                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        && UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inputField.gameObject
        && !string.IsNullOrEmpty(inputField.text))
        {
            string message = $"{PhotonNetwork.LocalPlayer.NickName}: {inputField.text}";
            GetComponent<PhotonView>().RPC("SendChat", RpcTarget.All, message);
            inputField.text = "";
            isInputFieldToggled = false;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }
    [PunRPC]
    public void SendChat(string text)
    {
        chatText.text = chatText.text + "\n" + text;
    }

}
