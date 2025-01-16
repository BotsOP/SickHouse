using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EventType = Managers.EventType;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField] private LeaderboardObject leaderboardObject;
    [SerializeField] private GameObject scoreUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private TMP_Text char1;
    [SerializeField] private TMP_Text char2;
    [SerializeField] private TMP_Text char3;
    [SerializeField] private TMP_Text char4;
    [SerializeField] private TMP_Text char5;
    [SerializeField] private TMP_Text saveButtonText;
    [SerializeField] private Text score;
    [SerializeField] private TMP_Text leaderboardEntry;
    [SerializeField] private RectTransform leaderboardContent;
    [SerializeField] private Text highScore;
    [SerializeField] private Text newRecord;
    private List<LeaderboardRanking> leaderboard;

    private void Start()
    {
        GameFinished();
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(EventType.GAME_OVER, GameFinished);
    }

    void OnEnable()
    {
        UpdateSaveButton(0);
        EventSystem.Subscribe(EventType.GAME_OVER, GameFinished);
        Time.timeScale = 1.0f;
        // newHigh.SetActive(false);
        // scoreUI.SetActive(false);
        if (leaderboardObject.Load())
        {
            Debug.Log($"loaded leaderboard");
            leaderboard = leaderboardObject.leaderboard.OrderByDescending(x => x.playerTime).ToList();
            InstantiateAllLeaderboardEntries();
            return;
        }

        leaderboard = new List<LeaderboardRanking>();
    }

    private void InstantiateAllLeaderboardEntries()
    {
        for (int i = 0; i < leaderboardContent.childCount; i++)
        {
            Destroy(leaderboardContent.GetChild(i).GetComponent<TMP_Text>());
            Destroy(leaderboardContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < leaderboard.Count; i++)
        {
            Instantiate(leaderboardEntry, leaderboardContent).text =
                leaderboard[i].playerName + " - " + leaderboard[i].playerTime.ToString("#.##");
        }
    }

    private void GameFinished()
    {
        Time.timeScale = 0f;
        float finalScore = Time.timeSinceLevelLoad;
        scoreUI.SetActive(true);
        gameUI.SetActive(false);
        score.text = finalScore.ToString();
        highScore.text = leaderboard[0].playerTime.ToString("#.##");

        if (leaderboard[0].playerTime < finalScore)
        {
            newRecord.gameObject.SetActive(true);
        }
    }

    public void SaveScore()
    {
        leaderboard.Add(new LeaderboardRanking(GetPlayerName(), Time.timeSinceLevelLoad));
        leaderboard = leaderboard.OrderByDescending(x => x.playerTime).ToList();
        leaderboardObject.leaderboard = leaderboard;
        leaderboardObject.Save();
        InstantiateAllLeaderboardEntries();
    }

    public void UpdateSaveButton(int _)
    {
        saveButtonText.text = "Save score as: '" + GetPlayerName() + "'";
    }
    

    private string GetPlayerName()
    {
        return char1.text + char2.text + char3.text + char4.text + char5.text;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
