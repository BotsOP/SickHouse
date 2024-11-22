using System;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EventType = Managers.EventType;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField] private GameObject scoreUI;
    [SerializeField] private GameObject newHigh;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private Text score;
    [SerializeField] private Text highScore;

    private void OnDisable()
    {
        EventSystem.Unsubscribe(EventType.GAME_OVER, GameFinished);
    }

    void OnEnable()
    {
        EventSystem.Subscribe(EventType.GAME_OVER, GameFinished);
        Time.timeScale = 1.0f;
        newHigh.SetActive(false);
        scoreUI.SetActive(false);
    }

    private void GameFinished()
    {
        float finalScore = Time.timeSinceLevelLoad;
        scoreUI.SetActive(true);
        gameUI.SetActive(false);
        score.text = finalScore.ToString();
        highScore.text = PlayerPrefs.GetFloat("HighScore").ToString();

        if (PlayerPrefs.GetFloat("HighScore") < finalScore)
        {
            newHigh.SetActive(true);
            PlayerPrefs.GetFloat("HighScore", finalScore);
            PlayerPrefs.Save();
        }
        Time.timeScale = 0f;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
