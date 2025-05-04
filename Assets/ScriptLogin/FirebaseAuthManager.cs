using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FirebaseAuthManager : MonoBehaviour
{
    // Các GameObject cho các form UI
    public GameObject loginForm; // Form đăng nhập
    public GameObject loginWithAccount; // Phần nội dung đăng nhập trong LoginForm
    public GameObject registerForm; // Form đăng ký
    public GameObject forgotPasswordForm; // Form quên mật khẩu

    // Các InputField cho nhập liệu
    public InputField loginUsernameInput; // Ô nhập email trong LoginForm
    public InputField loginPasswordInput; // Ô nhập mật khẩu trong LoginForm
    public InputField registerUsernameInput; // Ô nhập email trong RegisterForm
    public InputField registerPasswordInput; // Ô nhập mật khẩu trong RegisterForm
    public InputField registerConfirmPasswordInput; // Ô nhập xác nhận mật khẩu trong RegisterForm
    public InputField registerCharacterNameInput; // Ô nhập tên nhân vật trong RegisterForm
    public InputField forgotPasswordEmailInput; // Ô nhập email trong ForgotPasswordForm

    // Các Text cho thông báo
    public Text loginNotifyText; // Text hiển thị thông báo trong LoginForm
    public Text registerNotifyText; // Text hiển thị thông báo trong RegisterForm
    public Text forgotNotifyText; // Text hiển thị thông báo trong ForgotPasswordForm
    public Text forgotPasswordText; // Text kích hoạt ForgotPasswordForm
    public Text backToLoginText; // Text quay lại LoginForm từ ForgotPasswordForm

    // Các Button cho tương tác
    public Button loginButton; // Nút đăng nhập trong LoginForm
    public Button registerButton; // Nút đăng ký trong RegisterForm
    public Button loadRegisterFormButton; // Nút chuyển sang RegisterForm
    public Button loadLoginFormButton; // Nút chuyển sang LoginForm
    public Button sendResetButton; // Nút gửi yêu cầu đặt lại mật khẩu trong ForgotPasswordForm

    private FirebaseAuth auth; // Đối tượng Firebase Auth
    private FirebaseApp app; // Ứng dụng Firebase

    private string lastCharacterName = ""; // Lưu tạm tên nhân vật để giữ khi đăng ký lỗi

    void Start()
    {
        // Khởi tạo cấu hình Firebase
        FirebaseApp.Create(new AppOptions
        {
            ApiKey = "AIzaSyC7R6Du-VpYMU1G97Pjq2Y6CfIy5B1hzL0",
            AppId = "1:15447035979:web:b7315fd718c98da406fdce",
            ProjectId = "logingameuser-702a9"
        });

        // Kiểm tra và khắc phục dependencies của Firebase
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

        // Gán sự kiện cho các nút
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        registerButton.onClick.AddListener(OnRegisterButtonClicked);
        loadRegisterFormButton.onClick.AddListener(ShowRegisterForm);
        loadLoginFormButton.onClick.AddListener(ShowLoginForm);
        sendResetButton.onClick.AddListener(OnSendResetClicked);

        // Gán sự kiện click cho Text quên mật khẩu
        var forgotTrigger = forgotPasswordText.gameObject.AddComponent<EventTrigger>();
        var forgotEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        forgotEntry.callback.AddListener((eventData) => ShowForgotPasswordForm());
        forgotTrigger.triggers.Add(forgotEntry);

        // Gán sự kiện click cho Text quay lại đăng nhập
        var backTrigger = backToLoginText.gameObject.AddComponent<EventTrigger>();
        var backEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        backEntry.callback.AddListener((eventData) => ShowLoginForm());
        backTrigger.triggers.Add(backEntry);

        // Hiển thị mặc định LoginWithAccount
        loginWithAccount.SetActive(true);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(false);

        // Gán sự kiện thay đổi giá trị cho InputCharacterName và khôi phục tên nhân vật
        registerCharacterNameInput.onValueChanged.AddListener(OnCharacterNameChanged);
        registerCharacterNameInput.text = lastCharacterName;
    }

    // Cập nhật tên nhân vật khi người dùng thay đổi
    private void OnCharacterNameChanged(string newName)
    {
        lastCharacterName = newName;
    }

    // Kiểm tra định dạng email hợp lệ
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

    // Xử lý sự kiện khi nhấn nút đăng nhập
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

        // Thực hiện đăng nhập với Firebase
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

            // Lưu thông tin phiên vào PlayerPrefs
            PlayerPrefs.SetString("UserEmail", user.Email);
            PlayerPrefs.SetString("LoginTime", System.DateTime.Now.ToString());
            PlayerPrefs.SetString("CharacterName", lastCharacterName);
            PlayerPrefs.Save();

            loginNotifyText.text = $"Đăng nhập thành công: {lastCharacterName}";
            Debug.Log($"Đăng nhập thành công: {user.Email}, CharacterName: {lastCharacterName}");
            SendLoginNotification(user);
        });
    }

    // Xử lý sự kiện khi nhấn nút đăng ký
    public void OnRegisterButtonClicked()
    {
        string email = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;
        string characterName = registerCharacterNameInput.text;
        Debug.Log($"Register - Email: {email}, Password: {password}, Confirm: {confirmPassword}, CharacterName: {characterName}");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(characterName))
        {
            registerNotifyText.text = "Vui lòng nhập email, mật khẩu và tên nhân vật.";
            return;
        }
        if (!IsValidEmail(email))
        {
            registerNotifyText.text = "Email không hợp lệ.";
            registerUsernameInput.text = "";
            registerPasswordInput.text = "";
            registerConfirmPasswordInput.text = "";
            return;
        }
        if (password != confirmPassword)
        {
            registerNotifyText.text = "Mật khẩu xác nhận không khớp.";
            registerPasswordInput.text = "";
            registerConfirmPasswordInput.text = "";
            return;
        }

        // Tạo tài khoản với Firebase
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
                        registerUsernameInput.text = "";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message += "Email đã được sử dụng.";
                        registerUsernameInput.text = "";
                        break;
                    case AuthError.WeakPassword:
                        message += "Mật khẩu quá yếu.";
                        registerPasswordInput.text = "";
                        registerConfirmPasswordInput.text = "";
                        break;
                    default:
                        message += ex.Message;
                        registerUsernameInput.text = "";
                        registerPasswordInput.text = "";
                        registerConfirmPasswordInput.text = "";
                        break;
                }
                registerNotifyText.text = message;
                return;
            }

            FirebaseUser user = task.Result.User;
            // Gửi email xác nhận
            user.SendEmailVerificationAsync().ContinueWithOnMainThread(verifyTask =>
            {
                if (verifyTask.IsFaulted)
                {
                    registerNotifyText.text = $"Lỗi gửi email xác nhận: {verifyTask.Exception.Message}";
                    registerUsernameInput.text = "";
                    registerPasswordInput.text = "";
                    registerConfirmPasswordInput.text = "";
                    return;
                }
                registerNotifyText.text = "Email xác nhận đã được gửi. Vui lòng kiểm tra hộp thư.";
                Debug.Log($"Email xác nhận đã được gửi đến: {user.Email}");
                registerUsernameInput.text = "";
                registerPasswordInput.text = "";
                registerConfirmPasswordInput.text = "";
            });
        });
    }

    // Xử lý sự kiện khi nhấn nút gửi yêu cầu đặt lại mật khẩu
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

        // Gửi email đặt lại mật khẩu
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

            forgotNotifyText.text = "Email đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư.";
            Debug.Log("Email đặt lại mật khẩu đã được gửi.");
        });
    }

    // Gửi thông báo đăng nhập (giả lập qua log)
    private void SendLoginNotification(FirebaseUser user)
    {
        Debug.Log($"Thông báo: Tài khoản {user.Email} đã đăng nhập vào lúc {System.DateTime.Now}");
    }

    // Hiển thị RegisterForm
    void ShowRegisterForm()
    {
        Debug.Log("Chuyển sang RegisterForm");
        loginWithAccount.SetActive(false);
        registerForm.SetActive(true);
        forgotPasswordForm.SetActive(false);
        loginNotifyText.text = "";
        registerCharacterNameInput.text = lastCharacterName; // Khôi phục tên nhân vật
    }

    // Hiển thị LoginWithAccount
    void ShowLoginForm()
    {
        Debug.Log("Chuyển sang LoginWithAccount");
        loginWithAccount.SetActive(true);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(false);
        registerNotifyText.text = "";
        forgotNotifyText.text = "";
    }

    // Hiển thị ForgotPasswordForm
    void ShowForgotPasswordForm()
    {
        Debug.Log("Chuyển sang ForgotPasswordForm");
        loginWithAccount.SetActive(false);
        registerForm.SetActive(false);
        forgotPasswordForm.SetActive(true);
        loginNotifyText.text = "";
    }
}