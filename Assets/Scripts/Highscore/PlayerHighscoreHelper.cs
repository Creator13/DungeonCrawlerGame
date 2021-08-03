using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Dungen.Highscore
{
    [Serializable]
    public struct HighscoreListing
    {
        public string nickname;
        public long time;
        public int score;
    }

    public class PlayerHighscoreHelper : MonoBehaviour
    {
        [Serializable]
        private struct HighscoreListWrapper
        {
            public HighscoreListing[] highscores;
        }

        [Serializable]
        public struct User
        {
            public int id;
            public string email;
            public string nickname;

            public override string ToString()
            {
                return $"User: id={id}; email={email}; nickname={nickname}";
            }
        }

        public bool LoggedIn => !Equals(CurrentUser, default(User));
        public User CurrentUser { get; private set; }
        public HighscoreListing[] highscoreList;

        public event Action<string> LoginFailed;
        public event Action<User> LoginSucceeded;

        public event Action HighscoresDownloaded;

        public IEnumerator PlayerLoginRequest(string email, string password)
        {
            using var www = UnityWebRequest.Post(HighscoreConstants.GetUrl("/login/player"),
                new Dictionary<string, string> {{"email", email}, {"password", password}});

            yield return www.SendWebRequest();

            if (www.responseCode == 200)
            {
                Debug.Log(www.downloadHandler.text);

                var user = JsonUtility.FromJson<User>(www.downloadHandler.text);

                Debug.Log(user);

                CurrentUser = user;
                LoginSucceeded?.Invoke(user);

                // After retrieving the user data from logging in, we no longer need the player to be authenticated as they can't submit their
                // own high scores for safety; this is the game server's responsibility.
                StartCoroutine(DoPlayerLogout());
            }
            else
            {
                var msg = www.downloadHandler.text;

                if (www.responseCode == 404)
                {
                    msg = "Could not connect to the high score server!";
                }
                else if (www.responseCode != 401)
                {
                    Debug.LogError($"Unexpected server error: {www.responseCode} - {www.downloadHandler.text}");
                    msg = "Unexpected server error!";
                }

                LoginFailed?.Invoke(msg);
            }
        }

        public IEnumerator DoPlayerLogout()
        {
            using var www = UnityWebRequest.Get(HighscoreConstants.GetUrl("/login/logout"));

            yield return www.SendWebRequest();

            if (www.responseCode != 200)
            {
                Debug.LogError("Unexpected server response while logging player out: " + www.responseCode);
            }
        }

        public IEnumerator HighscoreListRequest()
        {
            using var www = UnityWebRequest.Get(HighscoreConstants.GetUrl("/api/highscore-list?count=200"));

            yield return www.SendWebRequest();

            var json = www.downloadHandler.text;
            Debug.Log($"{{\"highscores\":{json}}}");
            var wrapper = JsonUtility.FromJson<HighscoreListWrapper>($"{{\"highscores\":{json}}}");
            highscoreList = wrapper.highscores;
            HighscoresDownloaded?.Invoke();
        }

        private void OnDestroy()
        {
            if (!LoggedIn) return;
            
            using var www = UnityWebRequest.Get(HighscoreConstants.GetUrl("/login/logout"));

            www.SendWebRequest();
            while (!www.isDone) { }

            if (www.responseCode != 200)
            {
                Debug.LogError($"Failed to log out player: HTTP {www.responseCode}");
            }
        }
    }
}
