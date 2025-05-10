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

    // Gọi từ nút đăng ký
    public void OnRegisterButton()
    {
        StartCoroutine(Register());
    }

    // Gọi từ nút đăng nhập
    public void OnLoginButton()
    {
        StartCoroutine(Login());
    }
    public void OnOpenLoginButton()
    {
        registerForm.SetActive(false);
        loginForm.SetActive(true); ;
    }
    public void OnOpenRegisterButton()
    {
        loginForm.SetActive(false);
        registerForm.SetActive(true);
    }
    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", regUsername.text);
        form.AddField("first_name", regFirstName.text);
        form.AddField("last_name", regLastName.text);
        form.AddField("email", regEmail.text);
        form.AddField("password", regPassword.text);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/player/register/", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            regMessage.text = "Đăng ký thành công!";
            registerForm.SetActive(false);
            loginForm.SetActive(true);
        }
        else
        {
            regMessage.text = "Lỗi: " + www.downloadHandler.text;
        }
    }
    [System.Serializable]
    public class TokenResponse
    {
        public string refresh;
        public string access;
    }

    IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", loginUsername.text);
        form.AddField("password", loginPassword.text);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/token/", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Ghi nhận thành công
            loginMessage.text = "Đăng nhập thành công!";

            // Lấy token từ JSON phản hồi
            string json = www.downloadHandler.text;
            TokenResponse token = JsonUtility.FromJson<TokenResponse>(json);

            // Lưu token vào PlayerPrefs
            PlayerPrefs.SetString("access_token", token.access);
            PlayerPrefs.SetString("refresh_token", token.refresh);

            // Chuyển scene sang GameScene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        else
        {
            // Ghi nhận lỗi
            loginMessage.text = "Sai tài khoản hoặc mật khẩu!";
            Debug.LogError(www.downloadHandler.text); // Log lỗi để kiểm tra nếu cần
        }
    }
}
