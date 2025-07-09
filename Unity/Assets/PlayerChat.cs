using Mirror;
using UnityEngine;

public class PlayerChat : NetworkBehaviour
{
    public static PlayerChat LocalPlayerInstance;

    public override void OnStartLocalPlayer()
    {
        LocalPlayerInstance = this;
    }

    [Command]
    public void CmdSendMessage(string message)
    {
        RpcReceiveMessage($"Player [{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcReceiveMessage(string message)
    {
        GameChatUI.Instance?.ReceiveMessage(message);
    }
}
