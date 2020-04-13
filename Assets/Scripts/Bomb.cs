using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int Life;


    public void Initialize(int life)
    {
        Life = life;
        var hex = transform.GetComponent<Hexagon>();
        transform.GetComponent<SpriteRenderer>().sprite = GridManager.Instance.BombSprite[hex.ColorIndex];
    }


    public void DecreaseRemainingMove()
    {
        if (Life-- <= 0)
        {
            GameManager.Instance.FinishGame();
            GameManager.Instance.OpenMenuScene();
        }
    }
}
