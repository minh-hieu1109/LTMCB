using TMPro;
using UnityEngine;

public class GameChatUI : MonoBehaviour
{
    public static GameChatUI Instance;

    [SerializeField] private GameObject chatPanel;   
    [SerializeField] private TMP_Text chatText;      
    [SerializeField] private TMP_InputField chatInput;

    private bool chatVisible = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        chatPanel.SetActive(chatVisible);
        chatText.text = "";
    }

    private void Update()
    {
        if (PlayerChat.LocalPlayerInstance == null)
            return;

        if (!chatInput.isFocused && Input.GetKeyDown(KeyCode.T))
        {
            ToggleChat();
        }

        if (chatVisible && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            SendMessageFromInput();
        }
    }

    private void ToggleChat()
    {
        chatVisible = !chatVisible;
        chatPanel.SetActive(chatVisible);

        if (chatVisible)
        {
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
        else
        {
            chatInput.DeactivateInputField();
        }
    }

    public void SendMessageFromButton()
    {
        SendMessageFromInput();
    }

    private void SendMessageFromInput()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text))
            return;

        PlayerChat.LocalPlayerInstance?.CmdSendMessage(chatInput.text);

        chatInput.text = "";
        chatInput.ActivateInputField();
    }

    public void ReceiveMessage(string message)
    {
        chatText.text += "\n" + message;
    }
}
