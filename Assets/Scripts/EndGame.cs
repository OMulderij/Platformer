using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EndGame : MonoBehaviour
{
    private Label scoreLabel;
    private TextField nameField;
    private Button confirmButton;
    private Label errorPrint;
    private UIDocument UIDoc;
    private Label leaderboard;
    private Label leaderboardTitle;
    private Label instructions;
    private string currentScore;
    private List<KeyValuePair<string, string>> allScores;
    private PlayerControl playerControl;
    private string leaderboardPreferenceName = "LeaderBoard";
    private string nameBorder = "<nameborder>";
    private string scoreBorder = "<scoreborder>";
    private int nameLengthLimit = 12;

    void Awake()
    {
        UIDoc = GetComponent<UIDocument>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerControl player))
        {
            playerControl = player;
            UIDoc.enabled = true;
            Initialize();
            leaderboard.visible = false;
            leaderboardTitle.visible = false;
            instructions.visible = false;

            string secondsTaken = playerControl.EndScreenTrigger();
            currentScore = secondsTaken;
            scoreLabel.text = "It took " + secondsTaken + " Seconds to win!";

            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void Initialize()
    {
        scoreLabel = UIDoc.rootVisualElement.Q<Label>("ScoreLabel");
        nameField = UIDoc.rootVisualElement.Q<TextField>("SetName");
        confirmButton = UIDoc.rootVisualElement.Q<Button>("ConfirmScore");
        errorPrint = UIDoc.rootVisualElement.Q<Label>("ErrorPrint");
        leaderboard = UIDoc.rootVisualElement.Q<Label>("LeaderBoard");
        leaderboardTitle = UIDoc.rootVisualElement.Q<Label>("LeaderBoardTitle");
        instructions = UIDoc.rootVisualElement.Q<Label>("Instructions");
        confirmButton.clicked += ButtonPress;
        errorPrint.visible = false;
        nameField.label = "Input name:";
    }

    public void ButtonPress()
    {
        if (nameField.value == "")
        {
            errorPrint.visible = true;
            errorPrint.text = "Please insert a name.";
            return;
        }
        else if (nameField.value.Length > nameLengthLimit)
        {
            errorPrint.visible = true;
            errorPrint.text = $"Please enter a name of {nameLengthLimit} characters or less.";
            return;
        } 
        else if (nameField.value == nameBorder || nameField.value == scoreBorder)
        {
            errorPrint.visible = true;
            errorPrint.text = "--_--";
            return;
        }

        playerControl.LeaderboardScreenTrigger();

        leaderboard.visible = true;
        leaderboardTitle.visible = true;
        instructions.visible = true;
        scoreLabel.visible = false;
        nameField.visible = false;
        confirmButton.visible = false;
        errorPrint.visible = false;

        allScores = GetScores();
        Dictionary<string, float> floatScores = new Dictionary<string, float>();

        foreach (KeyValuePair<string, string> s in allScores)
        {
            floatScores.Add(s.Key, (float)Convert.ToDouble(s.Value));
        }
        
        int count = 0;
        foreach (KeyValuePair<string, float> s in floatScores.OrderBy(key => key.Value))
        {
            count++;
            leaderboard.text += count + ". " + s.Key + " = " + s.Value + "<br>";
            if (count >= 10)
            {
                break;
            }
        }
        SaveNewScore(nameField.value, currentScore);
    }

    private List<KeyValuePair<string, string>> GetScores()
    {
        List<KeyValuePair<string, string>> scores = new List<KeyValuePair<string, string>>
        {
            { new KeyValuePair<string, string>(nameField.value, currentScore) }
        };
        string[] scorePairs = PlayerPrefs.GetString(leaderboardPreferenceName).Split(scoreBorder);
        
        foreach (string s in scorePairs)
        {
            if (s == "")
            {
                continue;
            }
            string[] keyValue = s.Split(nameBorder);
            scores.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
        }

        return scores;
    }

    private void SaveNewScore(string newName, string newScore)
    {
        PlayerPrefs.SetString(leaderboardPreferenceName, PlayerPrefs.GetString(leaderboardPreferenceName) + newName + nameBorder + newScore + scoreBorder);
    }
}