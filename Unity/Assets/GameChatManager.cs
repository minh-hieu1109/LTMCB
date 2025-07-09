using Mirror;
using System;
using TMPro;
using UnityEngine;

public class GameChatManager : NetworkBehaviour
{
    [SerializeField] private GameObject chatUI;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField chatInput;

    private static event Action<string> OnMessage;

    private bool chatVisible = false;

    public override void OnStartAuthority()
    {
        chatUI.SetActive(chatVisible);
        chatText.text = "";
        OnMessage += HandleNewMessage;
    }

    private void OnDestroy()
    {
        OnMessage -= HandleNewMessage;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        // Nếu đang gõ chat, không toggle bằng T
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
        chatUI.SetActive(chatVisible);

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

    [Client]
    public void SendMessageFromButton()
    {
        SendMessageFromInput();
    }

    [Client]
    private void SendMessageFromInput()
    {
        if (!isLocalPlayer) return;

        if (!string.IsNullOrWhiteSpace(chatInput.text))
        {
            CmdSendMessage(chatInput.text);
            chatInput.text = "";
            chatInput.ActivateInputField();
        }
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        RpcHandleMessage($"Player [{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }

    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }
}
