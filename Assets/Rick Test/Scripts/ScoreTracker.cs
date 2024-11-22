using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject scoreUI;
    public GameObject newHigh;
    public Text score;
    public Text highScore;

    public float timeAlive;
    bool gameHasBegun;
    float finalScore = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;
        finalScore = 0;
        newHigh.SetActive(false);
        scoreUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gridManager.wallDistance > 0)
        {
            //Start timer
            timeAlive += Time.deltaTime;
            gameHasBegun = true;
        }
        else
        {
            //This little situation is when the  game ends!
            //Stop timer and if game has started, save the score
            if (gameHasBegun)
            {
                //Save the score and wipe the timer
                if (finalScore == 0)
                {
                    finalScore = timeAlive;
                    scoreUI.SetActive(true);
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
                timeAlive = 0;                
            }
            else
            {
                //If the game hasn't started yet
                timeAlive = 0;
            }
        }
    }
}
