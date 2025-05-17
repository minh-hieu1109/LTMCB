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

    private bool chatActive = true;

    public override void OnStartAuthority()
    {
        chatUI.SetActive(chatActive);
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            chatActive = !chatActive;
            chatUI.SetActive(chatActive);

            if (chatActive)
                chatInput.ActivateInputField();
            else
                chatInput.DeactivateInputField();
        }

        if (chatActive && chatInput.isFocused && Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageFromInput();
        }
    }

    [Client]
    public void SendMessageFromButton() // Gắn vào button OnClick
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