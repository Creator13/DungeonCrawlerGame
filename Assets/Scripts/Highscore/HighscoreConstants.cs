namespace Dungen.Highscore
{
    public static class HighscoreConstants
    {
        public const string HIGHSCORE_SERVER_URL = "http://localhost";

        // Obviously this should not be in the code but I'm leaving it here so you can see what's happening
        public const string VERY_SECRET_SERVER_PASSWORD = "[3[uhku@&809<.34*$dskdd1opil[f]]#(";

        public static string GetUrl(string path)
        {
            return HIGHSCORE_SERVER_URL + path;
        }
    }
}
