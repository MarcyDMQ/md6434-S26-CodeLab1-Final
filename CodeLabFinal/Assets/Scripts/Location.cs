using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

[CreateAssetMenu(fileName = "Location", menuName = "Scriptable Objects/Location")]
public class Location : ScriptableObject
{
    public string sceneToLoad; // 如果填了名字，就代表点击后会跳转
    public string transitionButtonText = "Start Exam"; // 按钮上显示的文字

    // background img
    public Sprite locationImage;

    // using list to store dialog
    [TextArea(2, 5)]
    public List<string> dialogueSentences;

    public Location northLocation;
    public Location southLocation;
    public Location westLocation;
    public Location eastLocation;

    /*private void OnValidate();
    {
        if (westLocation != null && westLocation.eastLocation != this)
        {
            westLocation.eastLocation = this;
        }

        if (GameManager.instance != null)
        {
            UpdateLocationDisplay(GameManager.instance);
        }
    }*/

    public void UpdateLocationDisplay(DialogManager dm)
    {
        //dm.locationNameDisplay.text = name;
        //dm.locationDescriptionDisplay.text = description;

        // adding img to scene
        if (dm.backgroundDisplay != null && locationImage != null)
        {
            dm.backgroundDisplay.sprite = locationImage;
        }

        if (northLocation == null)
        {
            dm.NorthButton.SetActive(false);
        }
        else
        {
            dm.NorthButton.SetActive(true);
        }

        if (southLocation == null)
        {
            dm.SouthButton.SetActive(false);
        }
        else
        {
            dm.SouthButton.SetActive(true);
        }

        if (westLocation == null)
        {
            dm.WestButton.SetActive(false);
        }
        else
        {
            dm.WestButton.SetActive(true);
        }
        //if westLocation is null then false,turn the button off
        dm.EastButton.SetActive(eastLocation!=null);
        
        if (dm.transitionButton != null)
        {
            // 只有当 sceneToLoad 字符串不为空时，才显示跳转按钮
            bool hasTargetScene = !string.IsNullOrEmpty(sceneToLoad);
            dm.transitionButton.gameObject.SetActive(hasTargetScene);

            // 设置按钮上的文字
            if (hasTargetScene && dm.transitionButtonText != null)
            {
                dm.transitionButtonText.text = transitionButtonText;
            }
        }
    }
}