using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // for score list
using System.Linq; // for ranking scores

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
}

// using Json to read and load data
[System.Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public class GameManager : MonoBehaviour
{   
    //public GameObject sphere;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    //public TMP_Text gameOverText;
    //public TMP_Text levelText;
    
    public TMP_Text leaderboardText; 

    // inputName
    public TMP_InputField nameInputField;
    public GameObject nameInputPanel;     
    public GameObject GameOverPanel;
    private string currentPlayerName = "Player"; // default player name
    // a bool that controls whether game has started, because players need time to enter their name
    private bool gameStarted = false;     
    
    int score = 0;
    public float maxTime = 60;
    private bool gameOver = false;
    private bool isTransitioning = false; //a bool that avoid multiple trigger of shrinking process

    const string DIR_DATA = "/Data/";
    const string FILE_HIGHSCORE = DIR_DATA + "highScore.txt";
    
    // Json path for leaderboard
    const string FILE_LEADERBOARD = DIR_DATA + "leaderboard.json";
    
    // static instance
    public static GameManager instance;
    
    public int currentLevel = 0;
    public float survivalTime = 0f;
    public float targetTime = 3f; 

    public float HighScore
    {
        get { return LoadHighScore(); }
        set { SaveHighScore(value); }
    }

    void Awake()
    {
        instance = this; 
    }

    void Start()
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        // pause time before player confirm name
        if (nameInputPanel != null) nameInputPanel.SetActive(true);
        Time.timeScale = 0; 

        UpdateScoreDisplay();
    }
    
    public void ConfirmName()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            currentPlayerName = nameInputField.text;
        }

        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        gameStarted = true;
        Time.timeScale = 1; // make the game start
    }

    public void incrementScore()
    {
        score++;
        // refresh score before display highscore to prevent lag display
        if (score > HighScore) { HighScore = score; }
        UpdateScoreDisplay();
    }

    public void decrementScore() 
    {
        score = score - 1;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            //scoreText.text = "Score: " + score + "  HighScore: " + Mathf.FloorToInt(HighScore);
            scoreText.text = "Score: " + score ;
    }

    void Update()
    {
        if (!gameStarted || gameOver) return; 
        // countdown
        maxTime -= Time.deltaTime;
        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(maxTime);

        // load next ascii file after fixed time
        survivalTime += Time.deltaTime; 
        if (survivalTime >= targetTime && !isTransitioning)
        {
            StartCoroutine(WaitAndLoadNextLevel());
            survivalTime = 0f; 
        }
        // Game over 
        if (maxTime <= 0)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {//game over and related UI display
        gameOver = true;
        SaveToLeaderboard(currentPlayerName, score);
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    System.Collections.IEnumerator WaitAndLoadNextLevel()
    {
        isTransitioning = true; 
        Debug.Log("Switching Level");

        yield return new WaitForSeconds(0.4f);

        // load next ascii level
        GoToNextLevel();

        // wait for a few time in case the shrinking is not finished
        yield return new WaitForSeconds(0.1f);
    
        isTransitioning = false; 
    }

    void GoToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        // ascii level txt path
        string nextPath = Application.dataPath + "/Resources/Levels/Level" + nextLevel + ".txt";

        if (File.Exists(nextPath))
        {
            currentLevel = nextLevel;
            Debug.Log("next level");
        }
        else
        {
            currentLevel = 0;
            Debug.Log("final level ");
        }

        if (ASCIILevelLoader.instance != null)
        {
            // update the level num
            ASCIILevelLoader.instance.CurrentLevel = currentLevel;
            // load ascii level 
            ASCIILevelLoader.instance.LoadLevel(); 
        }
    }

    public void startOver() {
        // reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SaveHighScore(float value)
    {
        string fullPath = Application.dataPath + FILE_HIGHSCORE;
        if (!Directory.Exists(Application.dataPath + DIR_DATA)) 
            Directory.CreateDirectory(Application.dataPath + DIR_DATA);
        File.WriteAllText(fullPath, value.ToString("F0"));
    }

    private float LoadHighScore()
    {
        string fullPath = Application.dataPath + FILE_HIGHSCORE;
        // raed the data if it exists
        if (File.Exists(fullPath))
        {
            float result;
            if (float.TryParse(File.ReadAllText(fullPath), out result))
            {
                return result;
            }
        }
        return 0f;
    }

    // leaderboard
    private void SaveToLeaderboard(string pName, int pScore)
    {
        string fullPath = Application.dataPath + FILE_LEADERBOARD;
        LeaderboardData data = new LeaderboardData();

        // raed the old scores
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            data = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            // put default scores in the leaderboard
            data.entries.Add(new ScoreEntry { playerName = "Good", score = 30 });
            data.entries.Add(new ScoreEntry { playerName = "Average", score = 20 });
            data.entries.Add(new ScoreEntry { playerName = "Bad", score = 10 });
        }

        // put player scores in leaderboard
        data.entries.Add(new ScoreEntry { playerName = pName, score = pScore });
        // put scores in order
        data.entries = data.entries.OrderByDescending(s => s.score).ToList();
        // save only the first 10 scores
        data.entries = data.entries.Take(10).ToList(); 
        // save scores to Json
        string newJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, newJson);
        // refresh and display
        DisplayLeaderboard();
    }

    private void DisplayLeaderboard()
    {
        if (leaderboardText == null) return;

        string fullPath = Application.dataPath + FILE_LEADERBOARD;
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);

            string displayStr = "\n";

            // go through the whole leaderboard data
            for (int i = 0; i < data.entries.Count; i++)
            {
                ScoreEntry entry = data.entries[i];
                string line = "";

                // display default scores in certain way
                if (entry.playerName == "Good" && entry.score == 30)
                {
                    line += "Good: 30";
                }
                else if (entry.playerName == "Average" && entry.score == 20)
                {
                    line += "Average: 20";
                }
                else if (entry.playerName == "Bad" && entry.score == 10)
                {
                    line += "Bad: 10";
                }
                else
                {
                    // player name and score
                    line += entry.playerName + ": " + entry.score;
                }

                //mark player score
                //if (entry.playerName == currentPlayerName && entry.score == score)
                //{line += " < YOU";}

                displayStr += line + "\n";
            }
            leaderboardText.text = displayStr;
        }
    }
}