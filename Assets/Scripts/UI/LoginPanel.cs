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
        [SerializeField] private Button login;

        private HighscoreUserManager highscoreUserManager;

        private void Start()
        {
            welcome.gameObject.SetActive(false);
            loginDetails.gameObject.SetActive(true);
            
            login.onClick.AddListener(Login);
            highscoreUserManager = gameController.HighscoreUserManager;
            
            highscoreUserManager.LoginFailed += FlashLoginMessage;
            highscoreUserManager.LoginSucceeded += ShowUser;
        }

        private void OnDisable()
        {
            login.onClick.RemoveListener(Login);
            
            if (highscoreUserManager)
            {
                highscoreUserManager.LoginFailed -= FlashLoginMessage;
                highscoreUserManager.LoginSucceeded -= ShowUser;
            }
        }

        private void Login()
        {
            error.gameObject.SetActive(false);
            StartCoroutine(highscoreUserManager.PlayerLoginRequest(email.text, password.text));
        }

        private void FlashLoginMessage(string msg)
        {
            error.gameObject.SetActive(true);
            error.text = msg;
        }

        private void ShowUser(HighscoreUserManager.User user)
        {
            highscoreUserManager.LoginFailed -= FlashLoginMessage;
            highscoreUserManager.LoginSucceeded -= ShowUser;
         
            loginDetails.gameObject.SetActive(false);
            welcome.gameObject.SetActive(true);
            
            welcome.text = $"Welcome {user.nickname}!";
        }
    }
}
