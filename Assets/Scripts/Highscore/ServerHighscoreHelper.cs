using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Dungen.Highscore
{
    public class ServerHighscoreHelper : MonoBehaviour
    {
        [SerializeField] private bool serverLoggedIn;
        [SerializeField] private int gameSessionId;

        public bool ServerLoggedIn => serverLoggedIn;

        public bool ServerLoginRequest()
        {
            using var sessionStartRequest = UnityWebRequest.Get(HighscoreConstants.GetUrl("/login/start-game-session"));

            sessionStartRequest.SendWebRequest();

            while (!sessionStartRequest.isDone) { } // Method is blocking on purpose

            if (sessionStartRequest.responseCode != 200)
            {
                Debug.LogError($"Couldn't start new session: HTTP {sessionStartRequest.responseCode}");
                serverLoggedIn = false;
                return serverLoggedIn;
            }

            gameSessionId = int.Parse(sessionStartRequest.downloadHandler.text);

            using var serverAuthRequest = UnityWebRequest.Post(HighscoreConstants.GetUrl("/login/server"), new Dictionary<string, string> {
                    {"game-session", $"{gameSessionId}"},
                    {"password", HighscoreConstants.VERY_SECRET_SERVER_PASSWORD + gameSessionId}
                }
            );

            serverAuthRequest.SendWebRequest();
            while (!serverAuthRequest.isDone) { }

            if (serverAuthRequest.responseCode != 200)
            {
                Debug.LogError($"Couldn't log in to server: HTTP {serverAuthRequest.responseCode}");
                serverLoggedIn = false;
                return serverLoggedIn;
            }

            serverLoggedIn = true;
            return serverLoggedIn;
        }

        public void SendHighscoreSubmitRequest(int playerId, int score)
        {
            StartCoroutine(SubmitHighscore(playerId, score));

            // TODO implement a callback for when requests are completed, to prevent the highscores from being submitted too late before the client downloads the list from the server
        }

        private IEnumerator SubmitHighscore(int playerId, int score)
        {
            if (!serverLoggedIn)
            {
                throw new InvalidOperationException("Cannot submit highscore while server is not logged in");
            }

            using var www = UnityWebRequest.Post(HighscoreConstants.GetUrl("/api/add-highscore"),
                new Dictionary<string, string> {{"user_id", $"{playerId}"}, {"score", $"{score}"}});

            yield return www.SendWebRequest();

            if (www.responseCode != 201)
            {
                Debug.Log($"Couldn't submit highscore: HTTP {www.responseCode}");
            }
        }

        private void Logout()
        {
            if (!serverLoggedIn)
            {
                return;
            }

            using var www = UnityWebRequest.Get(HighscoreConstants.GetUrl("/login/logout"));

            www.SendWebRequest();

            while (!www.isDone) { }

            if (www.responseCode != 200)
            {
                Debug.Log($"Server logout failed: HTTP {www.responseCode}");
            }
        }

        private void OnDestroy()
        {
            Logout();
        }
    }
}
