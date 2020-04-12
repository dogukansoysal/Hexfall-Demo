using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hexagon : MonoBehaviour
{
    public int ColorIndex;
    public Vector2Int GridIndex;
    public Vector2Int[] NeighbourIndices;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="colorIndex"></param>
    public void ChangeHexagonColor(Sprite sprite, int colorIndex)
    {
        transform.GetComponent<SpriteRenderer>().sprite = sprite;
        ColorIndex = colorIndex;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void CalculateNeighbourIndices()
    {
        Vector2Int[] cornerNeighbourIndex = new Vector2Int[6];
        cornerNeighbourIndex[0] = new Vector2Int(GridIndex.x, GridIndex.y + 1);    // Top
        cornerNeighbourIndex[1] = new Vector2Int(GridIndex.x, GridIndex.y - 1);    // Bottom

        if (GridIndex.x % 2 == 0)
        {
            cornerNeighbourIndex[2] = new Vector2Int(GridIndex.x - 1, GridIndex.y);    // Top Left
            cornerNeighbourIndex[3] = new Vector2Int(GridIndex.x + 1, GridIndex.y);    // Top Right
            cornerNeighbourIndex[4] = new Vector2Int(GridIndex.x - 1, GridIndex.y - 1);    // Bottom Left
            cornerNeighbourIndex[5] = new Vector2Int(GridIndex.x + 1, GridIndex.y - 1);    // Bottom Right
        }
        else
        {
            cornerNeighbourIndex[2] = new Vector2Int(GridIndex.x - 1, GridIndex.y + 1);    // Top Left
            cornerNeighbourIndex[3] = new Vector2Int(GridIndex.x + 1, GridIndex.y + 1);    // Top Right
            cornerNeighbourIndex[4] = new Vector2Int(GridIndex.x - 1, GridIndex.y);    // Bottom Left
            cornerNeighbourIndex[5] = new Vector2Int(GridIndex.x + 1, GridIndex.y);    // Bottom Right
        }

        NeighbourIndices = cornerNeighbourIndex;
    }

    public IEnumerator ExplosionSequence()
    {
        // TODO: Spawn particle
        
        MakeInvisible();

        yield return StartCoroutine(Explode());
        
        var colorIndex = Random.Range(0, GameManager.Instance.CurrentLevel.ColorCount);
        ChangeHexagonColor(GridManager.Instance.HexagonSprite[colorIndex], colorIndex);

        MoveToTop();
        MakeVisible();
        yield return StartCoroutine(DropHexagon());
        

        /*
         * + Make sprite invisible
         * + Make Explosion (Particle burst)
         * +   After explosion;
         * + Set new color
         * + Move transform to top
         * + Make sprite visible
         * + DOTween to old position (Falling effect)
         *
         * Bu fonksiyonun çağrıldığı yerden callback beklensin. Tüm bu fonksiyonlar bitene kadar oyun oynanamasın.
         *     After Falling;
         * Update Grid Map
         * + Check new grid map for any possible explosion
         * 
         * + If not;
         * + GameState = Playable
         *
         * 
         */
    }

    private void MakeInvisible()
    {
        transform.GetComponent<SpriteRenderer>().enabled = false;

    }
    
    private void MakeVisible()
    {
        transform.GetComponent<SpriteRenderer>().enabled = true;
    }
    
    private void MoveToTop()
    {
        transform.position = new Vector3(transform.position.x, GameConstants.DropPosition, transform.position.z);

    }

    private IEnumerator Explode()
    {
        var ps = transform.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.SetSprite(0, transform.GetComponent<SpriteRenderer>().sprite);
        ps.Play();
        while(ps.isPlaying)
            yield return null;
        
    }
    
    private IEnumerator DropHexagon()
    {
        var targetPosition = GridManager.Instance.CalculateWorldPosition(GridIndex);
        
        var dropDuration = Mathf.Clamp(transform.position.y - targetPosition.y,0.25f, GameConstants.MaxDropDuration) * GameConstants.DropDuration;
        dropDuration *= Random.Range(0.8f, 1.2f);
        
        var tween = transform.DOMove(targetPosition, dropDuration).SetEase(Ease.OutBounce);
        
        while (tween.IsActive())
        {
            yield return null;
        }
    }
    
    
}
