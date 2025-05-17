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
        // Tìm hoặc tạo Canvas chung cho lobby UI
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
        }

        // Tạo Panel riêng cho player
        uiPanel = new GameObject("PlayerPanel_" + index);
        uiPanel.transform.SetParent(canvasGO.transform);
        RectTransform panelRT = uiPanel.AddComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(400, 150);

        // Đặt vị trí panel theo index, để không chồng UI (tối đa 4 player)
        // Các panel cách nhau 160 pixels theo chiều dọc, từ y = 200 xuống dưới
        float yPos = 200 - index * 160;
        panelRT.anchoredPosition = new Vector2(0, yPos);
        panelRT.localScale = Vector3.one;

        // Background Panel
        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Player Name Text
        GameObject nameGO = new GameObject("PlayerName");
        nameGO.transform.SetParent(uiPanel.transform);
        playerNameText = nameGO.AddComponent<Text>();
        playerNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playerNameText.fontSize = 36;
        playerNameText.color = Color.white;
        playerNameText.alignment = TextAnchor.UpperCenter;
        RectTransform nameRT = playerNameText.rectTransform;
        nameRT.sizeDelta = new Vector2(380, 40);
        nameRT.anchoredPosition = new Vector2(0, 50);
        nameRT.localScale = Vector3.one;

        // Ready State Text
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
        btnImage.color = Color.green;  // mặc định màu xanh
        readyButton.targetGraphic = btnImage;
        RectTransform btnRT = buttonGO.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(180, 50);
        btnRT.anchoredPosition = new Vector2(0, -50);
        btnRT.localScale = Vector3.one;

        // Text cho nút
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

        // Gán sự kiện click cho nút
        readyButton.onClick.AddListener(OnReadyClicked);

        UpdateUI();
    }

    void UpdateUI()
    {
        playerNameText.text = "Player [" + (index + 1) + "]";
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
}
