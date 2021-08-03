using Dungen.Gameplay;
using Dungen.Gameplay.States;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private DungenGame game;

        [SerializeField] private TMP_Text score;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private RectTransform highscoreList;
        [SerializeField] private HighscoreItem highscoreItemPrefab;
        [SerializeField] private RectTransform loadingHighscores;
        [SerializeField] private Button refreshHighscores;

        private void Awake()
        {
            quitButton.onClick.AddListener(OnClickQuit);
            menuButton.onClick.AddListener(OnClickMenu);
            refreshHighscores.onClick.AddListener(OnClickRefresh);
        }

        private void OnDisable()
        {
            quitButton.onClick.RemoveListener(OnClickQuit);
            menuButton.onClick.RemoveListener(OnClickMenu);
            refreshHighscores.onClick.RemoveListener(OnClickRefresh);
            
            game.PlayerHighscoreHelper.HighscoresDownloaded -= UpdateHighScoreDisplay;
        }

        private void Start()
        {
            game.PlayerHighscoreHelper.HighscoresDownloaded += UpdateHighScoreDisplay;
            
            score.text = $"Final score - {game.Score}";
            
            DisplayLoadingHighscores();
            StartCoroutine(game.PlayerHighscoreHelper.HighscoreListRequest());
        }

        private void DisplayLoadingHighscores()
        {
            foreach (Transform t in highscoreList)
            {
                Destroy(t.gameObject);
            }

            Instantiate(loadingHighscores, highscoreList);
        }
        
        private void UpdateHighScoreDisplay()
        {
            foreach (Transform t in highscoreList)
            {
                Destroy(t.gameObject);
            }

            var i = 0;
            foreach (var listing in game.PlayerHighscoreHelper.highscoreList)
            {
                var uiItem = Instantiate(highscoreItemPrefab, highscoreList);
                uiItem.SetValues(listing, ++i);
            }
        }

        private void OnClickRefresh()
        {
            DisplayLoadingHighscores();
            StartCoroutine(game.PlayerHighscoreHelper.HighscoreListRequest());
        }
        
        private void OnClickQuit()
        {
            Application.Quit();
        }

        private void OnClickMenu()
        {
            game.RequestStateChange<JoiningState>();
        }
    }
}
