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
    [SyncVar(hook = nameof(OnNicknameChanged))]
    public string playerNickname;
    private static Font sharedFont;
    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(DelayedCreateUI());
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

            // Room Code trên đầu
            GameObject roomCodeGO = new GameObject("RoomCodeText");
            roomCodeGO.transform.SetParent(canvasGO.transform);
            Text roomCodeText = roomCodeGO.AddComponent<Text>();
            roomCodeText.font = Font.CreateDynamicFontFromOSFont("Arial", 42);
            roomCodeText.fontSize = 42;
            roomCodeText.color = Color.cyan;
            roomCodeText.alignment = TextAnchor.UpperCenter;
            roomCodeText.text = "Room Code: " + PlayerPrefs.GetString("room_code", "UNKNOWN");

            RectTransform codeRT = roomCodeText.rectTransform;
            codeRT.anchorMin = new Vector2(0.5f, 1);
            codeRT.anchorMax = new Vector2(0.5f, 1);
            codeRT.pivot = new Vector2(0.5f, 1);
            codeRT.sizeDelta = new Vector2(600, 60);
            codeRT.anchoredPosition = new Vector2(0, -20);
        }

        // Panel
        uiPanel = new GameObject("PlayerPanel_" + index);
        uiPanel.transform.SetParent(canvasGO.transform);
        RectTransform panelRT = uiPanel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 1);
        panelRT.anchorMax = new Vector2(0.5f, 1);
        panelRT.pivot = new Vector2(0.5f, 1);
        panelRT.sizeDelta = new Vector2(500, 200);
        float spacing = 220;
        float startY = -100;
        float yPos = startY - index * spacing;
        panelRT.anchoredPosition = new Vector2(0, yPos);

        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        panelImage.sprite = Resources.GetBuiltinResource<Sprite>("UISprite");
        panelImage.type = Image.Type.Sliced;

        // Player Name
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(uiPanel.transform);
        playerNameText = nameGO.AddComponent<Text>();
        playerNameText.font = Font.CreateDynamicFontFromOSFont("Arial", 36);
        playerNameText.fontSize = 36;
        playerNameText.color = Color.white;
        playerNameText.alignment = TextAnchor.UpperLeft;

        RectTransform nameRT = playerNameText.rectTransform;
        nameRT.anchorMin = new Vector2(0, 1);
        nameRT.anchorMax = new Vector2(0, 1);
        nameRT.pivot = new Vector2(0, 1);
        nameRT.sizeDelta = new Vector2(460, 50);
        nameRT.anchoredPosition = new Vector2(20, -10);

        // Ready State
        GameObject stateGO = new GameObject("ReadyState");
        stateGO.transform.SetParent(uiPanel.transform);
        readyStateText = stateGO.AddComponent<Text>();
        readyStateText.font = Font.CreateDynamicFontFromOSFont("Arial", 28);
        readyStateText.fontSize = 28;
        readyStateText.color = Color.yellow;
        readyStateText.alignment = TextAnchor.UpperLeft;

        RectTransform stateRT = readyStateText.rectTransform;
        stateRT.anchorMin = new Vector2(0, 1);
        stateRT.anchorMax = new Vector2(0, 1);
        stateRT.pivot = new Vector2(0, 1);
        stateRT.sizeDelta = new Vector2(460, 40);
        stateRT.anchoredPosition = new Vector2(20, -60);

        // Ready Button
        GameObject buttonGO = new GameObject("ReadyButton");
        buttonGO.transform.SetParent(uiPanel.transform);
        readyButton = buttonGO.AddComponent<Button>();
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.7f, 0.2f);
        readyButton.targetGraphic = btnImage;
        RectTransform btnRT = buttonGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0);
        btnRT.anchorMax = new Vector2(0.5f, 0);
        btnRT.pivot = new Vector2(0.5f, 0);
        btnRT.sizeDelta = new Vector2(200, 60);
        btnRT.anchoredPosition = new Vector2(0, 10);

        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(buttonGO.transform);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.font = Font.CreateDynamicFontFromOSFont("Arial", 28);
        btnText.fontSize = 28;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "READY";
        RectTransform btnTextRT = btnText.rectTransform;
        btnTextRT.anchorMin = new Vector2(0, 0);
        btnTextRT.anchorMax = new Vector2(1, 1);
        btnTextRT.pivot = new Vector2(0.5f, 0.5f);
        btnTextRT.sizeDelta = Vector2.zero;
        btnTextRT.anchoredPosition = Vector2.zero;

        // Kick Button
        GameObject kickGO = new GameObject("KickButton");
        kickGO.transform.SetParent(uiPanel.transform);
        Button kickButton = kickGO.AddComponent<Button>();
        Image kickImg = kickGO.AddComponent<Image>();
        kickImg.color = new Color(0.8f, 0.2f, 0.2f);
        kickButton.targetGraphic = kickImg;
        RectTransform kickRT = kickGO.GetComponent<RectTransform>();
        kickRT.anchorMin = new Vector2(1, 1);
        kickRT.anchorMax = new Vector2(1, 1);
        kickRT.pivot = new Vector2(1, 1);
        kickRT.sizeDelta = new Vector2(40, 40);
        kickRT.anchoredPosition = new Vector2(-10, -10);

        GameObject kickTextGO = new GameObject("KickText");
        kickTextGO.transform.SetParent(kickGO.transform);
        Text kickText = kickTextGO.AddComponent<Text>();
        kickText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        kickText.fontSize = 24;
        kickText.color = Color.white;
        kickText.alignment = TextAnchor.MiddleCenter;
        kickText.text = "X";
        RectTransform kickTextRT = kickText.rectTransform;
        kickTextRT.anchorMin = new Vector2(0, 0);
        kickTextRT.anchorMax = new Vector2(1, 1);
        kickTextRT.pivot = new Vector2(0.5f, 0.5f);
        kickTextRT.sizeDelta = Vector2.zero;
        kickTextRT.anchoredPosition = Vector2.zero;

        // Sự kiện
        readyButton.onClick.AddListener(OnReadyClicked);
        //kickButton.onClick.AddListener(OnKickClicked);

        if (!isServer)
            kickGO.SetActive(false);

        UpdateUI();
    }

    void UpdateUI()
    {
        playerNameText.text = string.IsNullOrEmpty(playerNickname) ? $"Player [{(index + 1)}]" : playerNickname;
        readyStateText.text = readyToBegin ? "READY" : "NOT READY";

        // Cập nhật nút theo trạng thái ready
        Text btnText = readyButton.GetComponentInChildren<Text>();
        if (readyToBegin)
        {
            btnText.text = "CANCEL";
            readyButton.image.color = Color.red;
        }
        else
        {
            btnText.text = "READY";
            readyButton.image.color = Color.green;
        }
    }

    public void OnReadyClicked()
    {
        // Toggle trạng thái Ready/Cancel
        CmdChangeReadyState(!readyToBegin);
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);
        UpdateUI();
    }

    private void Update()
    {
        // Tắt UI khi vào Scene game thực sự
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }
        }
        else
        {
            if (uiPanel != null && !uiPanel.activeSelf)
            {
                uiPanel.SetActive(true);
            }
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        // Xóa UI khi client dừng
        if (uiPanel != null)
        {
            Destroy(uiPanel);
        }
    }
    void OnNicknameChanged(string oldName, string newName)
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

        CmdSetNickname(MatchManager.CurrentNickname);
    }

}