using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Start()
    {
        UserInterfaceManager.Instance.UpdateHighScoreText();
        UserInterfaceManager.Instance.UpdateLastScoreText();
    }

    /// <summary>
    /// Game Scene Index = MenuScene Index + 1
    /// </summary>
    public void OpenGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
