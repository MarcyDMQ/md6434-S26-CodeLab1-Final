using System.Collections.Generic; //  Queue
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class DialogManager : MonoBehaviour
{
    public Button transitionButton; // 在 Inspector 里拖入你的“跳转/下一步”按钮
    public TextMeshProUGUI transitionButtonText; // 按钮上的文字
    // 用于显示背景图片的 UI Image 组件
    public Image backgroundDisplay;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;      // 对话框的父物体（用于显示/隐藏）
    public TextMeshProUGUI dialogueText;  // 显示对话内容的组件

    public Location startingLocation;
    public Location currentLocation;

    public GameObject NorthButton;
    public GameObject EastButton;
    public GameObject WestButton;
    public GameObject SouthButton;

    public static DialogManager instance;

    // Dialog Queue
    private Queue<string> sentences = new Queue<string>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //startingLocation.name = "Yard";//don't do this. just to show you you will overwrite data when you change it in playmode
        Debug.Log("Current Location:"+startingLocation);

        //locationNameDisplay.text = startingLocation.name;
        //locationDescriptionDisplay.text = startingLocation.description;
        
        startingLocation.UpdateLocationDisplay(this);

        currentLocation = startingLocation;

        // display dialog when first started
        StartDialogue(currentLocation.dialogueSentences);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // --- 新增：给跳转按钮绑定的函数 ---
    public void LoadTargetScene()
    {
        if (currentLocation != null && !string.IsNullOrEmpty(currentLocation.sceneToLoad))
        {
            Debug.Log("Loading Scene: " + currentLocation.sceneToLoad);
            SceneManager.LoadScene(currentLocation.sceneToLoad);
        }
    }
    // Dialog logic
    public void StartDialogue(List<string> newSentences)
    {
        // hide the dialog panel if there is no dialog
        if (newSentences == null || newSentences.Count == 0)
        {
            dialoguePanel.SetActive(false);
            return;
        }

        dialoguePanel.SetActive(true);
        sentences.Clear();

        // Put List data into Queue
        foreach (string sentence in newSentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            dialoguePanel.SetActive(false); // hide the dialog panel if there is no dialog
            return;
        }

        string sentence = sentences.Dequeue(); // display dialog through Queue
        dialogueText.text = sentence;
    }

    //North=0 East=1 West=2 South=3

    public void MoveDirection(int direction)
    {
        //make the location actually change when this function is called
        //currentLocation = currentLocation.northLocation;
        switch (direction)
        {
            case 0://North
                // currentLocation.northLocation.southLocation = currentLocation; 
                currentLocation = currentLocation.northLocation;
                break;
            case 1://East
                // currentLocation.eastLocation.westLocation = currentLocation;
                currentLocation = currentLocation.eastLocation;
                break;
            case 2://West
                // currentLocation.westLocation.eastLocation = currentLocation;
                currentLocation = currentLocation.westLocation;
                break;
            case 3://South
                // currentLocation.southLocation.northLocation = currentLocation;
                currentLocation = currentLocation.southLocation;
                break;
        }
        currentLocation.UpdateLocationDisplay(this);

        // when scene changes start over the dialog
        StartDialogue(currentLocation.dialogueSentences);
    }
}