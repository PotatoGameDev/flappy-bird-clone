using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using System;
using System.Text;
using TMPro;

public class LeaderboardsManager : MonoBehaviour
{
    private const string PlayerNamePrefKey = "LeaderboardPlayerName";
    private const string PlayerHighScorePrefKey = "LeaderboardPlayerHighScore";
    private const string LeaderboardId = "EndgameInfiniteMode";
    private const int MaxNameLength = 50;

    [SerializeField]
    private RectTransform entriesContainer;
    [SerializeField]
    private GameObject leaderboardEntryPrefab;
    [SerializeField]
    private GameObject leaderboardsView;
    [SerializeField]
    private GameObject changePlayerNameView;
    [SerializeField]
    private TMP_InputField playerNameInput;
    [SerializeField]
    private int topEntriesCount = 10;

    private bool isInitialized = false;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return;
        }

        isInitialized = true;
        RefreshLeaderboardsView();
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            RefreshLeaderboardsView();
        }
    }

    public async void AddHighScore(int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            PlayerPrefs.SetInt(PlayerHighScorePrefKey, score);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        LoadAndShowLeaderboard();
    }

    public async void LoadAndShowLeaderboard()
    {
        ClearContents();
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId);

            if (scoresResponse.Results.Count == 0)
            {
                CreateSeparatorRow(Loc.Get("leaderboards_no_results"));
                return;
            }

            string localPlayerId = AuthenticationService.Instance.PlayerId;
            bool playerInTopList = false;

            int shown = 0;
            foreach (var entry in scoresResponse.Results)
            {
                if (shown >= topEntriesCount)
                {
                    break;
                }
                shown++;
                CreateEntryRow(entry.Rank + 1, entry.Score, entry.PlayerName);
                if (entry.PlayerId == localPlayerId)
                {
                    playerInTopList = true;
                }
            }

            if (!playerInTopList)
            {
                try
                {
                    var playerEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
                    CreateSeparatorRow("...");
                    CreateEntryRow(playerEntry.Rank + 1, playerEntry.Score, playerEntry.PlayerName);
                }
                catch (Exception e)
                {
                    Debug.Log("Could not fetch player's own rank: " + e.Message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    private void CreateEntryRow(int displayRank, double score, string playerName)
    {
        GameObject row = Instantiate(leaderboardEntryPrefab, entriesContainer);
        var label = row.GetComponentInChildren<TextMeshProUGUI>();
        string cleanedName = StripUnityPostfix(playerName);
        label.text = $"{displayRank}. {score}: {cleanedName}";
    }

    private void CreateSeparatorRow(string text)
    {
        GameObject row = Instantiate(leaderboardEntryPrefab, entriesContainer);
        var label = row.GetComponentInChildren<TextMeshProUGUI>();
        label.text = text;
    }

    private void ClearContents()
    {
        for (int i = entriesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(entriesContainer.GetChild(i).gameObject);
        }
    }

    private string SanitizeName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return "Player";
        }

        string trimmed = playerName.Trim();
        var sb = new StringBuilder();
        foreach (char c in trimmed)
        {
            if (char.IsControl(c) || c == '#')
            {
                continue;
            }
            sb.Append(c);
        }

        string cleaned = sb.ToString().Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            cleaned = "Player";
        }
        if (cleaned.Length > MaxNameLength)
        {
            cleaned = cleaned.Substring(0, MaxNameLength);
        }
        return cleaned;
    }

    private string StripUnityPostfix(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            return playerName;
        }
        int hashIndex = playerName.IndexOf('#');
        return hashIndex >= 0 ? playerName.Substring(0, hashIndex) : playerName;
    }

    public async void SetPlayerName(string name)
    {
        string sanitized = SanitizeName(name);
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(sanitized);
            PlayerPrefs.SetString(PlayerNamePrefKey, sanitized);
            PlayerPrefs.Save();
            ShowLeaderboardsView();
            RefreshLeaderboardsView();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void SavePlayerName()
    {
        SetPlayerName(playerNameInput.text);
    }

    public void ShowEditNameView()
    {
        leaderboardsView.SetActive(false);
        changePlayerNameView.SetActive(true);
    }

    public void ShowLeaderboardsView()
    {
        leaderboardsView.SetActive(true);
        changePlayerNameView.SetActive(false);
    }

    public void RefreshLeaderboardsView()
    {
        string playerName = PlayerPrefs.GetString(PlayerNamePrefKey, null);
        if (playerName == null)
        {
            ShowEditNameView();
            return;
        }

        int highScore = PlayerPrefs.GetInt(PlayerHighScorePrefKey, 0);
        int lastScore = GameManager.Instance != null ? GameManager.Instance.lastScore : 0;

        if (lastScore > highScore)
        {
            AddHighScore(lastScore);
        }
        else
        {
            LoadAndShowLeaderboard();
        }

        ShowLeaderboardsView();
    }
}

