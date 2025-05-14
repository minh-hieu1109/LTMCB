using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("Register Form")]
    public TMP_InputField regUsername, regFirstName, regLastName, regEmail, regPassword;
    public TMP_Text regMessage;
    public GameObject registerForm, loginForm, verifyForm;

    [Header("Login Form")]
    public TMP_InputField loginUsername, loginPassword;
    public TMP_Text loginMessage;

    [Header("Verify Form")]
    public TMP_InputField codeInput;
    public TMP_Text verifyMessage;
    private string currentUsername;

    [System.Serializable]
    public class TokenResponse
    {
        public string refresh;
        public string access;
    }

    [System.Serializable]
    public class RegisterResponse
    {
        public string message;
        public string username;
        public string email;
    }

    [System.Serializable]
    public class VerifyResponse
    {
        public string message;
        public string error;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string detail;
    }

    [System.Serializable]
    public class GenericErrorResponse
    {
        public string error;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Xóa PlayerPrefs để ngăn nhảy Scene (bỏ sau khi test)
        PlayerPrefs.DeleteAll();
        // Mở LoginForm mặc định
        if (loginForm != null) loginForm.SetActive(true);
        if (registerForm != null) registerForm.SetActive(false);
        if (verifyForm != null) verifyForm.SetActive(false);
    }

    public void OnRegisterButton()
    {
        StartCoroutine(Register());
    }

    public void OnLoginButton()
    {
        StartCoroutine(Login());
    }

    public void OnOpenLoginButton()
    {
        if (registerForm != null) registerForm.SetActive(false);
        if (loginForm != null) loginForm.SetActive(true);
        if (verifyForm != null) verifyForm.SetActive(false);
    }

    public void OnOpenRegisterButton()
    {
        if (loginForm != null) loginForm.SetActive(false);
        if (registerForm != null) registerForm.SetActive(true);
        if (verifyForm != null) verifyForm.SetActive(false);
    }

    public void OnVerifyButton()
    {
        StartCoroutine(VerifyEmail());
    }

    IEnumerator Register()
    {
        if (regUsername == null || regFirstName == null || regLastName == null || regEmail == null || regPassword == null)
        {
            if (regMessage != null) regMessage.text = "X Lỗi: UI components chưa được gán.";
            yield return null;
        }
        else if (string.IsNullOrEmpty(regUsername.text) || string.IsNullOrEmpty(regFirstName.text) ||
                 string.IsNullOrEmpty(regLastName.text) || string.IsNullOrEmpty(regEmail.text) ||
                 string.IsNullOrEmpty(regPassword.text))
        {
            if (regMessage != null) regMessage.text = "X Vui lòng điền đầy đủ thông tin.";
            yield return null;
        }
        else if (!regEmail.text.Contains("@") || !regEmail.text.Contains("."))
        {
            if (regMessage != null) regMessage.text = "X Email không hợp lệ.";
            yield return null;
        }
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("username", regUsername.text);
            form.AddField("first_name", regFirstName.text);
            form.AddField("last_name", regLastName.text);
            form.AddField("email", regEmail.text);
            form.AddField("password", regPassword.text);

            using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/player/register/", form))
            {
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    RegisterResponse response = null;
                    bool parseSuccess = false;
                    try
                    {
                        response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                        parseSuccess = response != null && !string.IsNullOrEmpty(response.message);
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    if (parseSuccess)
                    {
                        if (regMessage != null) regMessage.text = "OK " + response.message;
                        currentUsername = response.username;
                        if (registerForm != null && verifyForm != null)
                        {
                            registerForm.SetActive(false);
                            verifyForm.SetActive(true);
                            if (verifyMessage != null) verifyMessage.text = "Nhập mã xác thực từ email";
                        }
                        else
                        {
                            Debug.LogError("registerForm hoặc verifyForm chưa được gán");
                            if (regMessage != null) regMessage.text = "X Lỗi: UI chưa được gán.";
                        }
                    }
                    else
                    {
                        if (regMessage != null) regMessage.text = "X Lỗi: Phản hồi không hợp lệ từ server.";
                    }
                    yield return null;
                }
                else
                {
                    string errorMsg = ParseErrorMessage(www.downloadHandler.text);
                    if (regMessage != null) regMessage.text = "X Lỗi: " + errorMsg;
                    yield return null;
                }
            }
        }
    }

    IEnumerator VerifyEmail()
    {
        if (codeInput == null || string.IsNullOrEmpty(codeInput.text))
        {
            if (verifyMessage != null) verifyMessage.text = "X Vui lòng nhập mã xác thực.";
            yield return null;
        }
        else if (string.IsNullOrEmpty(currentUsername))
        {
            if (verifyMessage != null) verifyMessage.text = "X Lỗi: Username không hợp lệ.";
            yield return null;
        }
        else
        {
            string url = $"http://127.0.0.1:8000/verify-email/?username={UnityWebRequest.EscapeURL(currentUsername)}&token={UnityWebRequest.EscapeURL(codeInput.text)}";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    VerifyResponse response = null;
                    bool parseSuccess = false;
                    try
                    {
                        response = JsonUtility.FromJson<VerifyResponse>(www.downloadHandler.text);
                        parseSuccess = response != null && !string.IsNullOrEmpty(response.message);
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    if (parseSuccess)
                    {
                        if (verifyMessage != null) verifyMessage.text = "OK Đăng ký thành công!";
                        yield return new WaitForSeconds(2f);
                        if (verifyForm != null && loginForm != null)
                        {
                            verifyForm.SetActive(false);
                            loginForm.SetActive(true);
                        }
                        else
                        {
                            Debug.LogError("verifyForm hoặc loginForm chưa được gán");
                            if (verifyMessage != null) verifyMessage.text = "X Lỗi: UI chưa được gán.";
                        }
                    }
                    else
                    {
                        if (verifyMessage != null) verifyMessage.text = "X Mã xác thực không đúng, vui lòng nhập lại.";
                    }
                    yield return null;
                }
                else
                {
                    string errorMsg = ParseErrorMessage(www.downloadHandler.text);
                    if (verifyMessage != null) verifyMessage.text = "X Lỗi: " + errorMsg;
                    yield return null;
                }
            }
        }
    }

    IEnumerator Login()
    {
        if (loginUsername == null || loginPassword == null)
        {
            if (loginMessage != null) loginMessage.text = "X Lỗi: UI components chưa được gán.";
            yield return null;
        }
        else if (string.IsNullOrEmpty(loginUsername.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            if (loginMessage != null) loginMessage.text = "X Vui lòng điền đầy đủ thông tin.";
            yield return null;
        }
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("username", loginUsername.text);
            form.AddField("password", loginPassword.text);

            using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/token/", form))
            {
                www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    TokenResponse token = null;
                    bool parseSuccess = false;
                    try
                    {
                        token = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
                        parseSuccess = token != null && !string.IsNullOrEmpty(token.access);
                    }
                    catch
                    {
                        parseSuccess = false;
                    }

                    if (parseSuccess)
                    {
                        PlayerPrefs.SetString("access_token", token.access);
                        PlayerPrefs.SetString("refresh_token", token.refresh);
                        if (loginMessage != null) loginMessage.text = "OK Đăng nhập thành công!";
                        // Tắt tất cả giao diện trước khi chuyển Scene

                        if (gameObject != null) gameObject.SetActive(false);
                        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
                    }
                    else
                    {
                        if (loginMessage != null) loginMessage.text = "X Đăng nhập thất bại: Phản hồi không hợp lệ.";
                        yield return null;
                    }
                }
                else
                {
                    string errorMsg = ParseErrorMessage(www.downloadHandler.text);
                    if (loginMessage != null) loginMessage.text = "X Lỗi: " + errorMsg;
                    yield return null;
                }
            }
        }
    }

    private string ParseErrorMessage(string json)
    {
        try
        {
            ErrorResponse err = JsonUtility.FromJson<ErrorResponse>(json);
            if (!string.IsNullOrEmpty(err.detail))
                return err.detail;
        }
        catch { }
        try
        {
            var genericErr = JsonUtility.FromJson<GenericErrorResponse>(json);
            if (!string.IsNullOrEmpty(genericErr.error))
                return genericErr.error;
        }
        catch { }
        return "Không thể xử lý phản hồi từ server.";
    }
}