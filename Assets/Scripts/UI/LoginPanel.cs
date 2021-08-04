using Dungen.Gameplay;
using Dungen.Highscore;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] private DungenGame gameController;

        [SerializeField] private TMP_Text welcome;
        [SerializeField] private RectTransform loginDetails;

        [SerializeField] private TMP_Text error;
        [SerializeField] private TMP_InputField email;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private Button loginButton;

        private TMP_Text loginButtonText;

        private PlayerHighscoreHelper playerHighscoreHelper;

        private void Start()
        {
            loginButtonText = loginButton.GetComponentInChildren<TMP_Text>();

            welcome.gameObject.SetActive(false);
            loginDetails.gameObject.SetActive(true);

            loginButton.onClick.AddListener(Login);
            playerHighscoreHelper = gameController.PlayerHighscoreHelper;

            playerHighscoreHelper.LoginFailed += OnLoginFailed;
            playerHighscoreHelper.LoginSucceeded += ShowUser;
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveListener(Login);

            if (playerHighscoreHelper)
            {
                playerHighscoreHelper.LoginFailed -= OnLoginFailed;
                playerHighscoreHelper.LoginSucceeded -= ShowUser;
            }
        }

        private void Update()
        {
            loginButton.interactable = ValidateInput();
        }

        private bool ValidateInput()
        {
            return string.IsNullOrEmpty(email.text) && string.IsNullOrEmpty(password.text);
        }

        private void Login()
        {
            error.gameObject.SetActive(false);

            loginButtonText.text = "Logging in...";
            loginButton.interactable = false;

            StartCoroutine(playerHighscoreHelper.PlayerLoginRequest(email.text, password.text));
        }

        private void OnLoginFailed(string msg)
        {
            FlashLoginMessage(msg);

            loginButtonText.text = "Log in";
            loginButton.interactable = true;
        }

        private void FlashLoginMessage(string msg)
        {
            error.gameObject.SetActive(true);
            error.text = msg;
        }

        private void ShowUser(PlayerHighscoreHelper.User user)
        {
            playerHighscoreHelper.LoginFailed -= OnLoginFailed;
            playerHighscoreHelper.LoginSucceeded -= ShowUser;

            loginDetails.gameObject.SetActive(false);
            welcome.gameObject.SetActive(true);

            welcome.text = $"Welcome {user.nickname}!";
        }
    }
}
