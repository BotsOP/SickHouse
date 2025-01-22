using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "DenisScene";
    [SerializeField] private LeaderboardObject leaderboardObject;
    [SerializeField] private TMP_Text leaderboardEntry;
    [SerializeField] private RectTransform leaderboardContent;
    [SerializeField] private RectTransform leaderboardUI;
    private bool showingLeaderboard;
    private List<LeaderboardRanking> leaderboard;

    private void OnEnable()
    {
        if (leaderboardObject.Load())
        {
            Debug.Log($"loaded leaderboard");
            leaderboard = leaderboardObject.leaderboard.OrderByDescending(x => x.playerTime).ToList();
            InstantiateAllLeaderboardEntries();
            return;
        }
        leaderboard = new List<LeaderboardRanking>();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void flipShowRanking()
    {
        showingLeaderboard = !showingLeaderboard;
        if (showingLeaderboard)
        {
            leaderboardUI.gameObject.SetActive(true);
            return;
        }
        leaderboardUI.gameObject.SetActive(false);
    }
    
    private void InstantiateAllLeaderboardEntries()
    {
        for (int i = 0; i < leaderboardContent.childCount; i++)
        {
            Destroy(leaderboardContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < leaderboard.Count; i++)
        {
            Instantiate(leaderboardEntry, leaderboardContent).text =
                leaderboard[i].playerName + " - " + leaderboard[i].playerTime.ToString("#.##");
        }
    }
}
