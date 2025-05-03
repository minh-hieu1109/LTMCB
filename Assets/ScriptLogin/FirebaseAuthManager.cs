using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FirebaseAuthManager : MonoBehaviour
{
    public GameObject loginForm;
    public GameObject loginWithAccount;
    public GameObject registerForm;
    public GameObject forgotPasswordForm; // Form quên mật khẩu
    public InputField loginUsernameInput;
    public InputField loginPasswordInput;
    public InputField registerUsernameInput;
    public InputField registerPasswordInput;
    public InputField registerConfirmPasswordInput;
    public InputField forgotPasswordEmailInput;
    public Text loginNotifyText;
    public Text registerNotifyText;
    public Text forgotNotifyText; // NotifyText của ForgotPasswordForm
    public Text forgotPasswordText; // Text cho quên mật khẩu
    public Text backToLoginText; // Text để quay lại LoginForm
    public Button loginButton;
    public Button registerButton;
    public Button loadRegisterFormButton;
    public Button loadLoginFormButton;
    public Button sendResetButton; // Button gửi yêu cầu đặt lại mật khẩu

    private FirebaseAuth auth;
    private FirebaseApp app;

    void Start()
    {
        FirebaseApp.Create(new AppOptions
        {
            ApiKey = "AIzaSyC7R6Du-VpYMU1G97Pjq2Y6CfIy5B1hzL0",
            AppId = "1:15447035979:web:b7315fd718c98da406fdce",
            ProjectId = "logingameuser-702a9"
        });

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase khởi tạo thành công!");
            }
            else
            {
                Debug.LogError($"Không thể khởi tạo Firebase: {task.Result}");
            }
        });

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        loadRegisterFormButton.onClick.AddListener(ShowRegisterForm);
        loadLoginFormButton.onClick.AddListener(ShowLoginForm);
        sendResetButton.onClick.AddListener(OnSendResetClicked);

        var forgotTrigger = forgotPasswordText.gameObject.AddComponent<EventTrigger>();
        var forgotEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        forgotEntry.callback.AddListener((eventData) => ShowForgotPasswordForm());
        forgotTrigger.triggers.Add(forgotEntry);

        var backTrigger = backToLoginText.gameObject.AddComponent<EventTrigger>();
        var backEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        backEntry.callback.AddListener((eventData) => ShowLoginForm());
        backTrigger.triggers.Add(backEntry);

        loginWithAccount.SetActive(true);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(false);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public void OnLoginButtonClicked()
    {
        string email = loginUsernameInput.text;
        string password = loginPasswordInput.text;
        Debug.Log($"Login - Email: {email}, Password: {password}");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginNotifyText.text = "Vui lòng nhập email và mật khẩu.";
            return;
        }
        if (!IsValidEmail(email))
        {
            loginNotifyText.text = "Email không hợp lệ.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                FirebaseException ex = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)ex.ErrorCode;
                string message = "Lỗi đăng nhập: ";
                switch (errorCode)
                {
                    case AuthError.InvalidEmail:
                        message += "Email không hợp lệ.";
                        break;
                    case AuthError.WrongPassword:
                        message += "Mật khẩu sai.";
                        break;
                    case AuthError.UserNotFound:
                        message += "Tài khoản không tồn tại.";
                        break;
                    default:
                        message += ex.Message;
                        break;
                }
                loginNotifyText.text = message;
                return;
            }

            FirebaseUser user = task.Result.User;
            if (!user.IsEmailVerified)
            {
                loginNotifyText.text = "Vui lòng xác nhận email trước khi đăng nhập.";
                return;
            }

            loginNotifyText.text = $"Đăng nhập thành công: {user.Email}";
            Debug.Log($"Đăng nhập thành công: {user.Email}");
            SendLoginNotification(user); // Gửi thông báo đăng nhập
        });
    }

    public void OnRegisterButtonClicked()
    {
        string email = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;
        Debug.Log($"Register - Email: {email}, Password: {password}, Confirm: {confirmPassword}");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            registerNotifyText.text = "Vui lòng nhập email và mật khẩu.";
            return;
        }
        if (!IsValidEmail(email))
        {
            registerNotifyText.text = "Email không hợp lệ.";
            return;
        }
        if (password != confirmPassword)
        {
            registerNotifyText.text = "Mật khẩu xác nhận không khớp.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                FirebaseException ex = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)ex.ErrorCode;
                string message = "Lỗi đăng ký: ";
                switch (errorCode)
                {
                    case AuthError.InvalidEmail:
                        message += "Email không hợp lệ.";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message += "Email đã được sử dụng.";
                        break;
                    default:
                        message += ex.Message;
                        break;
                }
                registerNotifyText.text = message;
                return;
            }

            FirebaseUser user = task.Result.User;
            user.SendEmailVerificationAsync().ContinueWithOnMainThread(verifyTask =>
            {
                if (verifyTask.IsFaulted)
                {
                    registerNotifyText.text = $"Lỗi gửi email xác nhận: {verifyTask.Exception.Message}";
                    return;
                }
                registerNotifyText.text = "Email xác nhận đã được gửi. Vui lòng kiểm tra hộp thư.";
                Debug.Log($"Email xác nhận đã được gửi đến: {user.Email}");
            });
        });
    }

    public void OnSendResetClicked()
    {
        string email = forgotPasswordEmailInput.text;
        Debug.Log($"Forgot Password - Email: {email}");

        if (string.IsNullOrEmpty(email))
        {
            forgotNotifyText.text = "Vui lòng nhập email.";
            return;
        }
        if (!IsValidEmail(email))
        {
            forgotNotifyText.text = "Email không hợp lệ.";
            return;
        }

        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                FirebaseException ex = task.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)ex.ErrorCode;
                string message = "Lỗi gửi email: ";
                switch (errorCode)
                {
                    case AuthError.InvalidEmail:
                        message += "Email không hợp lệ.";
                        break;
                    case AuthError.UserNotFound:
                        message += "Tài khoản không tồn tại.";
                        break;
                    default:
                        message += ex.Message;
                        break;
                }
                forgotNotifyText.text = message;
                return;
            }

            forgotNotifyText.text = "Email đặt lại mật khẩu đã được gửi.\n Vui lòng kiểm tra hộp thư.";
            Debug.Log("Email đặt lại mật khẩu đã được gửi.");
        });
    }

    private void SendLoginNotification(FirebaseUser user)
    {
        // Firebase không hỗ trợ gửi email thông báo đăng nhập tự động
        // Giả lập thông báo qua log
        Debug.Log($"Thông báo: Tài khoản {user.Email} đã đăng nhập vào lúc {System.DateTime.Now}");
        // Có thể tích hợp Firebase Cloud Functions để gửi email tùy chỉnh
    }

    void ShowRegisterForm()
    {
        Debug.Log("Chuyển sang RegisterForm");
        loginWithAccount.SetActive(false);
        registerForm.SetActive(true);
        forgotPasswordForm.SetActive(false);
        loginNotifyText.text = "";
    }

    void ShowLoginForm()
    {
        Debug.Log("Chuyển sang LoginWithAccount");
        loginWithAccount.SetActive(true);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(false);
        registerNotifyText.text = "";
        forgotNotifyText.text = "";
    }

    void ShowForgotPasswordForm()
    {
        Debug.Log("Chuyển sang ForgotPasswordForm");
        loginWithAccount.SetActive(false);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(true);
        loginNotifyText.text = "";
    }
}