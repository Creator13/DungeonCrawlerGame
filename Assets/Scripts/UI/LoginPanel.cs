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

        private PlayerHighscoreHelper playerHighscoreHelper;

        private void Start()
        {
            welcome.gameObject.SetActive(false);
            loginDetails.gameObject.SetActive(true);
            
            login.onClick.AddListener(Login);
            playerHighscoreHelper = gameController.PlayerHighscoreHelper;
            
            playerHighscoreHelper.LoginFailed += FlashLoginMessage;
            playerHighscoreHelper.LoginSucceeded += ShowUser;
        }

        private void OnDisable()
        {
            login.onClick.RemoveListener(Login);
            
            if (playerHighscoreHelper)
            {
                playerHighscoreHelper.LoginFailed -= FlashLoginMessage;
                playerHighscoreHelper.LoginSucceeded -= ShowUser;
            }
        }

        private void Login()
        {
            error.gameObject.SetActive(false);
            StartCoroutine(playerHighscoreHelper.PlayerLoginRequest(email.text, password.text));
        }

        private void FlashLoginMessage(string msg)
        {
            error.gameObject.SetActive(true);
            error.text = msg;
        }

        private void ShowUser(PlayerHighscoreHelper.User user)
        {
            playerHighscoreHelper.LoginFailed -= FlashLoginMessage;
            playerHighscoreHelper.LoginSucceeded -= ShowUser;
         
            loginDetails.gameObject.SetActive(false);
            welcome.gameObject.SetActive(true);
            
            welcome.text = $"Welcome {user.nickname}!";
        }
    }
}
