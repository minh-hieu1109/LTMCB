using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketClient : MonoBehaviour
{
    private ClientWebSocket webSocket;
    private Uri serverUri = new Uri("ws://localhost:8000/ws/chatroom/testroom/");

    // Start là nơi khởi tạo và bắt đầu kết nối
    async void Start()
    {
        webSocket = new ClientWebSocket();

        try
        {
            // Kết nối đến server WebSocket
            await webSocket.ConnectAsync(serverUri, CancellationToken.None);
            Debug.Log("Connected to WebSocket server!");

            // Bắt đầu nhận tin nhắn
            ReceiveMessages();

            // Gửi một tin nhắn mẫu sau khi kết nối
            await SendMessage("Hello from Unity!", 1);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to WebSocket: {e.Message}");
        }
    }

    // Phương thức để nhận tin nhắn từ WebSocket
    async void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"Received message: {message}");
                    // Xử lý tin nhắn nhận được (ví dụ: cập nhật UI)
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error receiving message: {e.Message}");
            }
        }
    }

    async Task SendMessage(string message, int playerId)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            // Tạo đối tượng DTO
            string group = "testroom";
            ChatMessage chatMessage = new ChatMessage
            {
                body = message,
                player_id = playerId,
                group = group,
            };

            // Chuyển đối tượng DTO thành JSON
            string jsonMessage = JsonUtility.ToJson(chatMessage);
            Debug.Log($"Sending message: {jsonMessage}");
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log($"Sent message: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending message: {e.Message}");
            }
        }
    }

    // OnApplicationQuit sẽ đóng kết nối khi ứng dụng dừng
    async void OnApplicationQuit()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            Debug.Log("WebSocket connection closed.");
        }
    }
    [System.Serializable]
    public class ChatMessage
    {
        public string body;
        public int player_id;
        public string group;
    }
}
