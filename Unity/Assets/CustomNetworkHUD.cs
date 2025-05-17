using Mirror;
using UnityEngine;

public class CustomNetworkHUD : NetworkManagerHUD
{
    public int fontSize = 24;

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = fontSize;

        GUILayout.BeginArea(new Rect(10, 10, 300, 300));

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (GUILayout.Button("Host (Server + Client)", style)) NetworkManager.singleton.StartHost();
            if (GUILayout.Button("Client", style)) NetworkManager.singleton.StartClient();
            if (GUILayout.Button("Server Only", style)) NetworkManager.singleton.StartServer();
        }
        else
        {
            if (NetworkServer.active) GUILayout.Label("Server: active", style);
            if (NetworkClient.isConnected) GUILayout.Label("Client: connected", style);

            if (GUILayout.Button("Stop", style)) NetworkManager.singleton.StopHost();
        }

        GUILayout.EndArea();
    }
}
