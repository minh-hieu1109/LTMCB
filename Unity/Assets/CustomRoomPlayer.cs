using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomRoomPlayer : NetworkRoomPlayer
{
    private GameObject uiPanel;
    private Text playerNameText;
    private Button readyButton;
    private Text readyStateText;
    private Text roomCodeText;

    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerNickname;

    [SyncVar(hook = nameof(OnRoomCodeChanged))]
    public string roomCode;

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(DelayedCreateUI());
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Set room code trên server khi host tạo
        //if (isServer && isLocalPlayer)
        //{
            roomCode = PlayerPrefs.GetString("room_code", "UNKNOWN");
        //}
    }

    private System.Collections.IEnumerator DelayedCreateUI()
    {
        yield return new WaitForEndOfFrame();
        CreateUI();
    }

    void CreateUI()
    {
        // Tìm hoặc tạo Canvas
        GameObject canvasGO = GameObject.Find("LobbyCanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("LobbyCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Room Code Text
            GameObject codeGO = new GameObject("RoomCodeText");
            codeGO.transform.SetParent(canvasGO.transform);
            roomCodeText = codeGO.AddComponent<Text>();
            roomCodeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            roomCodeText.fontSize = 36;
            roomCodeText.color = Color.cyan;
            roomCodeText.alignment = TextAnchor.UpperCenter;
            RectTransform codeRT = roomCodeText.rectTransform;
            codeRT.sizeDelta = new Vector2(500, 50);
            codeRT.anchoredPosition = new Vector2(0, 300);
            codeRT.localScale = Vector3.one;
        }
        else
        {
            var existingCode = canvasGO.transform.Find("RoomCodeText");
            if (existingCode != null)
                roomCodeText = existingCode.GetComponent<Text>();
        }

        // Panel player
        uiPanel = new GameObject("PlayerPanel_" + index);
        uiPanel.transform.SetParent(canvasGO.transform);
        RectTransform panelRT = uiPanel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(400, 150);
        float yPos = 200 - index * 160;
        panelRT.anchoredPosition = new Vector2(0, yPos);
        panelRT.localScale = Vector3.one;

        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Player Name
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(uiPanel.transform);
        playerNameText = nameGO.AddComponent<Text>();
        playerNameText.font = Font.CreateDynamicFontFromOSFont("Arial", 36);
        playerNameText.fontSize = 36;
        playerNameText.color = Color.white;
        playerNameText.alignment = TextAnchor.UpperCenter;
        RectTransform nameRT = playerNameText.rectTransform;
        nameRT.sizeDelta = new Vector2(380, 40);
        nameRT.anchoredPosition = new Vector2(0, 50);
        nameRT.localScale = Vector3.one;

        // Ready State
        GameObject stateGO = new GameObject("ReadyState");
        stateGO.transform.SetParent(uiPanel.transform);
        readyStateText = stateGO.AddComponent<Text>();
        readyStateText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        readyStateText.fontSize = 28;
        readyStateText.color = Color.yellow;
        readyStateText.alignment = TextAnchor.MiddleCenter;
        RectTransform stateRT = readyStateText.rectTransform;
        stateRT.sizeDelta = new Vector2(380, 30);
        stateRT.anchoredPosition = new Vector2(0, 10);
        stateRT.localScale = Vector3.one;

        // Ready Button
        GameObject buttonGO = new GameObject("ReadyButton");
        buttonGO.transform.SetParent(uiPanel.transform);
        readyButton = buttonGO.AddComponent<Button>();
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = Color.green;
        readyButton.targetGraphic = btnImage;
        RectTransform btnRT = buttonGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(180, 50);
        btnRT.anchoredPosition = new Vector2(0, -50);
        btnRT.localScale = Vector3.one;

        // Button Text
        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(buttonGO.transform);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 28;
        btnText.color = Color.black;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "READY";
        RectTransform btnTextRT = btnText.rectTransform;
        btnTextRT.sizeDelta = new Vector2(180, 50);
        btnTextRT.anchoredPosition = Vector2.zero;
        btnTextRT.localScale = Vector3.one;

        readyButton.onClick.AddListener(OnReadyClicked);

        UpdateUI();
    }

    void UpdateUI()
    {
        playerNameText.text = string.IsNullOrEmpty(playerNickname) ? $"Player [{(index + 1)}]" : playerNickname;
        readyStateText.text = readyToBegin ? "READY" : "NOT READY";

        Text btnText = readyButton.GetComponentInChildren<Text>();
        btnText.text = readyToBegin ? "CANCEL" : "READY";
        readyButton.image.color = readyToBegin ? Color.red : Color.green;

        if (roomCodeText != null)
            roomCodeText.text = "Room Code: " + (string.IsNullOrEmpty(roomCode) ? "..." : roomCode);
    }

    public void OnReadyClicked()
    {
        CmdChangeReadyState(!readyToBegin);
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        UpdateUI();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (uiPanel != null) uiPanel.SetActive(false);
        }
        else
        {
            if (uiPanel != null && !uiPanel.activeSelf) uiPanel.SetActive(true);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (uiPanel != null) Destroy(uiPanel);
    }

    void OnNicknameChanged(string oldName, string newName)
    {
        UpdateUI();
    }

    void OnRoomCodeChanged(string oldCode, string newCode)
    {
        UpdateUI();
    }

    [Command]
    public void CmdSetNickname(string nickname)
    {
        playerNickname = nickname;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        string nickname = PlayerPrefs.GetString("nickname", "Unknown");
        CmdSetNickname(nickname);
    }
}
