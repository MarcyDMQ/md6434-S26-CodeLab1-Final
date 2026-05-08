using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // for score list
using System.Linq; // for ranking scores

// 新增：单条排行榜数据格式
[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
}

// 修改：这个类现在配合 JSON 读写对象列表数据
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
    public TMP_InputField nameInputField; // 拖入你的输入框
    public GameObject nameInputPanel;     // 拖入包含输入框和按钮的面板
    public GameObject GameOverPanel;
    private string currentPlayerName = "Player"; // 默认名字
    private bool gameStarted = false;     // 控制游戏逻辑是否真正开始
    
    int score = 0;
    public float maxTime = 60;
    private bool gameOver = false;
    private bool isTransitioning = false; // 状态锁，防止重复触发

    const string DIR_DATA = "/Data/";
    const string FILE_HIGHSCORE = DIR_DATA + "highScore.txt";
    
    // 新增：排行榜 JSON 文件路径
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
        // 游戏开始时显示名字输入面板并暂停时间
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
        Time.timeScale = 1; // 恢复时间，游戏正式开始
    }

    public void incrementScore()
    {
        score++;
        // 逻辑修正：先更新高分，再刷新显示，解决延迟感
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
        // 如果名字还没输入或者游戏结束，什么都不做
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

        // 加载下一关
        GoToNextLevel();

        // 切换完后稍微等待一下，防止逻辑冲突
        yield return new WaitForSeconds(0.1f);
    
        isTransitioning = false; 
    }

    void GoToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        // 这里的路径必须和你磁盘上的真实路径完全一致
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
            // 先更新索引
            ASCIILevelLoader.instance.CurrentLevel = currentLevel;
            // 强制调用一次 LoadLevel，确保即便 level 号没变也重载
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
        // 如果文件存在就读，不存在就返回0
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

        // 读取旧数据
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            data = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            // 第一次运行时创建包含 30, 20, 10 的标杆数据
            data.entries.Add(new ScoreEntry { playerName = "Good", score = 30 });
            data.entries.Add(new ScoreEntry { playerName = "Average", score = 20 });
            data.entries.Add(new ScoreEntry { playerName = "Bad", score = 10 });
        }

        // 把当前玩家的名字和分数加进去
        data.entries.Add(new ScoreEntry { playerName = pName, score = pScore });
        // 重新排序（按 score 字段从大到小）
        data.entries = data.entries.OrderByDescending(s => s.score).ToList();
        // 只保留前10个数据 
        data.entries = data.entries.Take(10).ToList(); 
        // 保存
        string newJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, newJson);
        // 刷新显示
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

            // 遍历整个记录列表
            for (int i = 0; i < data.entries.Count; i++)
            {
                ScoreEntry entry = data.entries[i];
                string line = "";

                // 根据数值分配显示格式
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