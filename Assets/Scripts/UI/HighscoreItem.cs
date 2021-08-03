using System;
using Dungen.Highscore;
using TMPro;
using UnityEngine;

namespace Dungen.UI
{
    public class HighscoreItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text position;
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text date;

        public void SetValues(HighscoreListing hs, int position)
        {
            this.position.text = position.ToString();
            playerName.text = $"{hs.nickname}: {hs.score}";
            date.text = ParseUnixTimeToString(hs.time);
        }

        private static string ParseUnixTimeToString(long millis)
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(millis).Date;
            return $"{date.Year}-{date.Month}-{date.Day}";
        }
    }
}
