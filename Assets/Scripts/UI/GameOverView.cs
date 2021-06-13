using Dungen.Gameplay;
using Dungen.Gameplay.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dungen.UI
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private DungenGame game;

        [SerializeField] private TMP_Text score;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button menuButton;

        private void Awake()
        {
            quitButton.onClick.AddListener(OnClickQuit);
            menuButton.onClick.AddListener(OnClickMenu);
        }

        private void OnDestroy()
        {
            quitButton.onClick.RemoveListener(OnClickQuit);
            menuButton.onClick.RemoveListener(OnClickMenu);
        }

        private void Start()
        {
            score.text = $"Final score - {game.Score}";
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
