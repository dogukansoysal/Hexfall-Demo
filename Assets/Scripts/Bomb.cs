using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int Life;
    public TextMeshProUGUI LifeText;

    private Camera cam;

    private void Update()
    {
        if(LifeText)
            LifeText.transform.position = Camera.main.WorldToScreenPoint(transform.position);
    }

    public void Initialize(int life)
    {
        Life = life;
        var hex = transform.GetComponent<Hexagon>();
        transform.GetComponent<SpriteRenderer>().sprite = GridManager.Instance.BombSprite[hex.ColorIndex];
        LifeText = Instantiate(GridManager.Instance.BombTextPrefab).GetComponent<TextMeshProUGUI>();
        LifeText.transform.SetParent(GameManager.Instance.Canvas.transform);
        LifeText.text = Life.ToString();
    }


    public void DecreaseRemainingMove()
    {
        Life--;
        if (Life <= 0)
        {
            GameManager.Instance.FinishGame();
            GameManager.Instance.OpenMenuScene();
        }
        LifeText.text = Life.ToString();

    }
}
