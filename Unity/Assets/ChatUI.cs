using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField messageInputField;  // Ô nhập tin nhắn
    public Button sendButton;             // Nút gửi
    public TMP_Text chatWindow;               // Khu vực hiển thị tin nhắn

    private ClientWebSocket webSocket;
    private string playerId = "player1";         // PlayerId được lấy từ server Django

    async void Start()
    {
        webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri("ws://127.0.0.1:8000/ws/chatroom/testroom/"), CancellationToken.None);
        Debug.Log("WebSocket connected!");

        // Chạy lắng nghe tin nhắn không chặn luồng
        _ = Task.Run(ReceiveMessages);  // Không dùng await

        sendButton.onClick.AddListener(() => _ = SendMessage());
    }

    // Lắng nghe tin nhắn từ server
    async Task ReceiveMessages()
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

                    // Hiển thị tin nhắn lên UI
                    DisplayMessage(message);
                }
            }
            catch (Exception e)
            {
                    Debug.LogError($"Error receiving message: {e.Message}");
            }
        }
    }


    async Task SendMessage()
    {
        if (webSocket.State == WebSocketState.Open && !string.IsNullOrEmpty(messageInputField.text))
        {
            string messageContent = messageInputField.text;

            ChatMessage msg = new ChatMessage
            {
                body = messageContent,
                player_id = 2, // Hoặc thay bằng giá trị động nếu có
                group = "testroom"
            };

            string messageJson = JsonUtility.ToJson(msg);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            Debug.Log("Message sent: " + messageContent);
            messageInputField.text = "";
        }
        else
        {
            Debug.LogWarning("WebSocket not open or message is empty!");
        }
    }

    [System.Serializable]
    public class IncomingMessage
    {
        public string content;
    }

    void DisplayMessage(string message)
    {
        try
        {
            IncomingMessage parsed = JsonUtility.FromJson<IncomingMessage>(message);
            chatWindow.text += "\n" + parsed.content;
        }
        catch
        {
            Debug.LogWarning("Could not parse message: " + message);
            chatWindow.text += "\n" + message;  // fallback
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
