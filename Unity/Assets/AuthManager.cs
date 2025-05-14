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
    public GameObject registerForm, loginForm;

    [Header("Login Form")]
    public TMP_InputField loginUsername, loginPassword;
    public TMP_Text loginMessage;

    [System.Serializable]
    public class TokenResponse
    {
        public string refresh;
        public string access;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public string detail;
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
        registerForm.SetActive(false);
        loginForm.SetActive(true);
    }

    public void OnOpenRegisterButton()
    {
        loginForm.SetActive(false);
        registerForm.SetActive(true);
    }

    IEnumerator Register()
    {
        if (string.IsNullOrEmpty(regUsername.text) || string.IsNullOrEmpty(regFirstName.text) ||
            string.IsNullOrEmpty(regLastName.text) || string.IsNullOrEmpty(regEmail.text) ||
            string.IsNullOrEmpty(regPassword.text))
        {
            regMessage.text = "❌ Vui lòng điền đầy đủ thông tin.";
            yield break;
        }

        if (!regEmail.text.Contains("@") || !regEmail.text.Contains("."))
        {
            regMessage.text = "❌ Email không hợp lệ.";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", regUsername.text);
        form.AddField("first_name", regFirstName.text);
        form.AddField("last_name", regLastName.text);
        form.AddField("email", regEmail.text);
        form.AddField("password", regPassword.text);
        Debug.Log($"Sending: username={regUsername.text}, first_name={regFirstName.text}, last_name={regLastName.text}, email={regEmail.text}, password={regPassword.text}");

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/player/register/", form))
        {
            www.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Đăng ký response: " + www.downloadHandler.text);
                try
                {
                    var response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                    regMessage.text = "✅ " + response.message;
                    //registerForm.SetActive(false);
                    //verifyForm.SetActive(true); // Chuyển sang form xác thực
                }
                catch
                {
                    regMessage.text = "❌ Lỗi: Phản hồi không hợp lệ từ server.";
                    Debug.LogError("Lỗi parse JSON: " + www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError($"Đăng ký thất bại: {www.result}, Error: {www.error}, Response: {www.downloadHandler.text}");
                string errorMsg = ParseErrorMessage(www.downloadHandler.text);
                regMessage.text = "❌ Lỗi: " + errorMsg;
            }
        }
    }

    [System.Serializable]
    private class RegisterResponse
    {
        public string message;
        public string username;
        public string email;
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", loginUsername.text);
        form.AddField("password", loginPassword.text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/token/", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Đăng nhập response: " + www.downloadHandler.text);
                try
                {
                    TokenResponse token = JsonUtility.FromJson<TokenResponse>(www.downloadHandler.text);
                    PlayerPrefs.SetString("access_token", token.access);
                    PlayerPrefs.SetString("refresh_token", token.refresh);
                    loginMessage.text = "✅ Đăng nhập thành công!";
                    SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
                }
                catch
                {
                    loginMessage.text = "❌ Đăng nhập thất bại: Phản hồi không hợp lệ.";
                    Debug.LogError("Lỗi parse JSON: " + www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError($"Đăng nhập thất bại: {www.result}, Error: {www.error}, Response: {www.downloadHandler.text}");
                loginMessage.text = "❌ Lỗi: " + ParseErrorMessage(www.downloadHandler.text);
            }
        }
    }

    // Trích xuất lỗi từ JSON (nếu có)
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
        return "Không thể xử lý phản hồi từ server: " + json.Substring(0, Mathf.Min(json.Length, 200)); // Giới hạn độ dài
    }

    [System.Serializable]
    public class GenericErrorResponse
    {
        public string error;
    }
}

//void Start()
//{
//    // Kiểm tra nếu người dùng đã đăng nhập, tự động chuyển tới GameScene
//    if (PlayerPrefs.HasKey("access_token"))
//    {
//        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
//    }
//}
