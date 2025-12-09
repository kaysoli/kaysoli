using UnityEngine;

namespace RadioDispatch.Core
{
    /// <summary>
    /// Persists lightweight player progress such as best score and options.
    /// </summary>
    public static class ProfileManager
    {
        private const string BestScoreKey = "RadioDispatch_BestScore";
        private const string LastRankKey = "RadioDispatch_LastRank";
        private const string MicSensitivityKey = "RadioDispatch_MicSensitivity";
        private const string LanguageKey = "RadioDispatch_Language";
        private const string SubtitlesKey = "RadioDispatch_Subtitles";

        public static void SaveBestScore(int score)
        {
            if (score <= GetBestScore())
            {
                return;
            }

            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }

        public static int GetBestScore() => PlayerPrefs.GetInt(BestScoreKey, 0);

        public static void SaveLastRank(string rank)
        {
            if (string.IsNullOrWhiteSpace(rank))
            {
                return;
            }

            PlayerPrefs.SetString(LastRankKey, rank);
            PlayerPrefs.Save();
        }

        public static string GetLastRank() => PlayerPrefs.GetString(LastRankKey, "D");

        public static void SaveSettings(SettingsData data)
        {
            if (data == null)
            {
                return;
            }

            PlayerPrefs.SetFloat(MicSensitivityKey, Mathf.Clamp01(data.MicSensitivity));
            PlayerPrefs.SetString(LanguageKey, data.Language ?? "en");
            PlayerPrefs.SetInt(SubtitlesKey, data.ShowSubtitles ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static SettingsData LoadSettings()
        {
            return new SettingsData
            {
                MicSensitivity = PlayerPrefs.GetFloat(MicSensitivityKey, 0.6f),
                Language = PlayerPrefs.GetString(LanguageKey, "en"),
                ShowSubtitles = PlayerPrefs.GetInt(SubtitlesKey, 1) == 1
            };
        }
    }
}
