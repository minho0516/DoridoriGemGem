using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public float roundTime;
    [SerializeField] private UIManager UIManager;

    [SerializeField] private bool endingRound = false;
    private Board board;

    public int currentScore;
    public float disPlayScore;
    public float scoreSpeed;

    public int scoreTarget1, scoreTarget2, scoreTarget3;

    private bool isGameStart = false;
    private void Awake()
    {
        if (Instance is null) Instance = this;
        else Destroy(Instance);

        if(UIManager is null)
        {
            UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        }
        if(board is null)
        {
            board = FindObjectOfType<Board>();
        }
    }

    private void Update()
    {
        if(roundTime > 0 && isGameStart)
        {
            roundTime -= Time.deltaTime;

            if(roundTime < 0 )
            {
                roundTime = 0;
                endingRound = true;
            }
        }

        if(endingRound && board.currentState == Board.BoardState.move)
        {
            GameOverCheck();
            endingRound = false;
        }

        UIManager.timeText.text = roundTime.ToString("0.0") + "'s";
        disPlayScore = Mathf.Lerp(disPlayScore, currentScore, scoreSpeed * Time.deltaTime);
        UIManager.scoreText.text = disPlayScore.ToString("0");
    }

    public void MatchAndAddTime()
    {
        if (roundTime + 10f > 30f) roundTime = 30f;
        else roundTime += 10f;
    }

    private void GameOverCheck()
    {
        UIManager.roundOverPanel.SetActive(endingRound);
        UIManager.finalScore.text = currentScore.ToString("0");

        if(currentScore >= scoreTarget3)
        {
            UIManager.roundResultText.text = "3 Star";

            UIManager.star1.SetActive(true);
            UIManager.star2.SetActive(true);
            UIManager.star3.SetActive(true);
        }
        else if (currentScore >= scoreTarget2)
        {
            UIManager.roundResultText.text = "2 Star";
            UIManager.star1.SetActive(true);
            UIManager.star2.SetActive(true);
        }
        else if (currentScore >= scoreTarget1)
        {
            UIManager.roundResultText.text = "1 Star";
            UIManager.star1.SetActive(true);
        }
        else
        {
            UIManager.roundResultText.text = "Try Again";
        }
    }

    public void ReTryButtonCallbacks()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);

        isGameStart = false;
        UIManager.roundStatPanel.SetActive(true);
    }

    public void GameStartButtonCallbacks()
    {
        isGameStart = true;
        UIManager.roundStatPanel.SetActive(false);
    }
}
