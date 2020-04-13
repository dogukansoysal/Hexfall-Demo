using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using static GameConstants;
public class FloatingText : MonoBehaviour
{
    public void Initialize(Vector3 position)
    {
        var cam = Camera.main;
        transform.GetComponent<TextMeshProUGUI>().text = GameConstants.ExplosionScore.ToString();
        transform.position = cam.WorldToScreenPoint(position);
        transform.SetParent(GameManager.Instance.Canvas.transform);
        transform.DOMove(cam.WorldToScreenPoint(position + new Vector3(0, Random.Range(0.75f,1.25f) * FloatingTextDistance, 0)), FloatingTextDuration).SetEase(Ease.InOutSine)
            .SetDelay(FloatingTextDelay)
            .OnComplete(() => Destroy(gameObject));
    }
}
