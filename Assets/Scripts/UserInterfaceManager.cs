using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    public static UserInterfaceManager Instance;
    
    public TextMeshProUGUI ScoreText;

    public Transform FloatingTextPrefab;
    
    public TextMeshProUGUI HighScoreText;
    public TextMeshProUGUI LastScoreText;
    public GameObject WarningPopUpPanel;

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
        }else{
            Instance = this;
        }
    }
    
    public void UpdateScoreText()
    {
        GameManager.Instance.CalculateScore();
        ScoreText.text = GameManager.Instance.Score.ToString();
    }

    public void UpdateHighScoreText()
    {
        HighScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
    }
    
    public void UpdateLastScoreText()
    {
        LastScoreText.text = "Last Score: " + PlayerPrefs.GetInt("LastScore", 0);
    }

    public void ShowWarningPopUp()
    {
        Time.timeScale = 0;
        GameManager.Instance.GameState = GameConstants.GameState.NotPlayable;
        WarningPopUpPanel.SetActive(true);
    }
    
    public void HideWarningPopUp()
    {
        Time.timeScale = 1;
        GameManager.Instance.GameState = GameConstants.GameState.Playable;
        WarningPopUpPanel.SetActive(false);
    }
    
    
    public void SpawnFloatingText(Vector3 position)
    {
        var floatingText = Instantiate(FloatingTextPrefab, GameManager.Instance.Canvas.transform);
        floatingText.GetComponent<FloatingText>().Initialize(position);
    }
}
