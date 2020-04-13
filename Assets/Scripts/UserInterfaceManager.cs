using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInterfaceManager : MonoBehaviour
{
    public static UserInterfaceManager Instance;
    
    public TextMeshProUGUI ScoreText;

    public Transform FloatingTextPrefab;
    
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


    public void SpawnFloatingText(Vector3 position)
    {
        var floatingText = Instantiate(FloatingTextPrefab);
        floatingText.GetComponent<FloatingText>().Initialize(position);
    }
}
