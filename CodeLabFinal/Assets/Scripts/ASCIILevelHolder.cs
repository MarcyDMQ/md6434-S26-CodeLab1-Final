using System.IO;
using UnityEngine;

public class ASCIILevelLoader : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;
    public GameObject pointC;
    public GameObject pointD;
    public GameObject pointE;
    public GameObject speed;
    public GameObject minus;
    
    // put"Levels/Level<num>.txt"in inspector
    public string fileLocation; 
    private int currentLevel = 0;
    GameObject loadedLevel;

    public int CurrentLevel
    {
        set { currentLevel = value; LoadLevel(); }
        get { return currentLevel; }
    }
    
    public int xOffset = 5;
    public int yOffset = 4;

    public static ASCIILevelLoader instance;
        
    void Awake() 
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLevel();
    }

    public void LoadLevel()
    {
        if (loadedLevel != null) Destroy(loadedLevel);
        loadedLevel = new GameObject("Level" + currentLevel);
        
        // change the filePath
        string relativePath = fileLocation.Replace("<num>", currentLevel.ToString());
        
        // read the txt file in streamingAssets
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);

        string[] lines = File.ReadAllLines(fullPath);

        // generate objects
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 centerPos = playerObj != null ? playerObj.transform.position : Vector3.zero;
        
        int fileRows = lines.Length;
        int fileCols = lines[0].Length;

        for (int y = 0; y < lines.Length; y++)
        {
            string currentLineFromFile = lines[y];
            for (int x = 0; x < currentLineFromFile.Length; x++)
            {
                char currentChar = currentLineFromFile[x];
                GameObject newObject = null;

                switch (currentChar)
                {
                    case 'A': 
                        newObject = Instantiate<GameObject>(pointA); 
                        newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
                        break;
                    case 'B': 
                        newObject = Instantiate<GameObject>(pointB); 
                        newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
                        break;
                    case 'C': 
                        newObject = Instantiate<GameObject>(pointC); 
                        newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
                        break;
                    case 'D': 
                        newObject = Instantiate<GameObject>(pointD); 
                        newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
                        break;
                    case 'E': 
                        newObject = Instantiate<GameObject>(pointE); 
                        newObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
                        break;
                    case 'S': 
                        newObject = Instantiate<GameObject>(speed); 
                        break;
                    case 'M': 
                        newObject = Instantiate<GameObject>(minus); 
                        break;
                }

                if (newObject != null)
                {//generate objects considering the player in the center
                    float spawnX = Mathf.Round(centerPos.x) + (x - fileCols / 2);
                    float spawnZ = Mathf.Round(centerPos.z) - (y - fileRows / 2);

                    newObject.transform.position = new Vector3(spawnX, 0, spawnZ);
                    newObject.transform.SetParent(loadedLevel.transform);
                }
            }
        }
    }
}