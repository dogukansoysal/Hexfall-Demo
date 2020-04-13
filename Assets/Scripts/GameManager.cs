using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using ScriptableObject;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameConstants;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static singleton
    public static Camera Cam;
    public Canvas Canvas;
    
    public Level CurrentLevel;
    public Transform HexGroupOutline;
    
    public Vector2Int SelectedCornerIndex;
    public GameState GameState;

        
    /* Touch Management */
    // TODO: move to another script.
    private Vector2 _firstTouchPosition;
    private Vector2 _currentTouchPosition;

    /* Score Management */
    public int Score;
    public int explodedHexagonCount;
    
    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
        }else{
            Instance = this;
        }
        Cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        GridManager.Instance.InitGrid(CurrentLevel);
        GameState = GameState.Playable;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameState == GameState.NotPlayable) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            CalculateCurrentTouchPosition();
            _firstTouchPosition = _currentTouchPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CalculateCurrentTouchPosition();
            if (Vector2.Distance(_firstTouchPosition, _currentTouchPosition) < 2f)
            {
                SelectedCornerIndex = GridManager.Instance.FindClosestCornerIndex(_firstTouchPosition);
                SelectCorner(SelectedCornerIndex);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            CalculateCurrentTouchPosition();
            // Select Corner
            if (Vector2.Distance(_firstTouchPosition, _currentTouchPosition) > 3f)
            {
                if (SelectedCornerIndex.x < 0 || SelectedCornerIndex.y < 0) return;
                if (GridManager.Instance.CornerArray[SelectedCornerIndex.x, SelectedCornerIndex.y] == null) return;
                StartCoroutine(MakeMove(SelectedCornerIndex));
            }
        }
    }



    #region Game Management
    
    public void FinishGame()
    {
        // TODO: Finish Animation etc.
        GameState = GameState.NotPlayable;

        if (Score > PlayerPrefs.GetInt("HighScore", 0))
        {
            UpdateHighScore();
        }
        UpdateLastScore();

    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void OpenMenuScene()
    {
        FinishGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    #endregion


    
    #region Progress Management

    private void UpdateLastScore()
    {
        PlayerPrefs.SetInt("LastScore", Score);
    }
    
    private void UpdateHighScore()
    {
        PlayerPrefs.SetInt("HighScore", Score);
    }

    #endregion
    
    
    
    
    private void CalculateCurrentTouchPosition(){
        _currentTouchPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        _currentTouchPosition = Cam.ScreenToWorldPoint(_currentTouchPosition);
    }
    
    private IEnumerator MakeMove(Vector2Int selectedCornerIndex)
    {
        GameState = GameState.NotPlayable;
        var direction = GetRotationDirection();
        // Loop to 3 rotation, check every rotation if a corner reach requirement.
        for (var i = 0; i < 3; i++)
        {
            StartCoroutine(RotateHexGroupOutline(direction));
            yield return StartCoroutine(RotateHexGroup(GridManager.Instance.FindHexGroup(selectedCornerIndex), direction));

            var hexList = GridManager.Instance.CheckEveryCorner();
            
            if( hexList.Count > 0)
            {
                ReleaseCorner();
                i = 3;
                
                // Found at least one matched group
                while (hexList.Count > 0)
                {
                
                    foreach (var hex in hexList)
                    {
                        explodedHexagonCount++;
                        StartCoroutine(hex.ExplosionSequence());
                        // Clear hexagon from grid
                        GridManager.Instance.ClearGridCell(hex.GridIndex);
                    }
                
                    UserInterfaceManager.Instance.UpdateScoreText();
                    yield return GridManager.Instance.CheckEveryHexagonIfUnderIsEmpty();
                    
                    Coroutine coroutine = null;
                    foreach (var hex in hexList)
                    {
                        coroutine = StartCoroutine(hex.SpawnNewHexagon());
                        GridManager.Instance.FillGridCell(hex.GridIndex, hex);
                    }
                    
                    yield return coroutine;
                    hexList = GridManager.Instance.CheckEveryCorner();
                    yield return new WaitForSeconds(CornerCheckDuration);
                }
            }
        }
        
        GameState = GameState.Playable;
    }


    private IEnumerator RotateHexGroup(Hexagon[] hexGroup, int direction)
    {

        var sequence = DOTween.Sequence();
        
        if (direction == (int)RotationDirection.Clockwise)
        {
            for (var i = 0; i < hexGroup.Length; i++)
            {
                sequence.Insert(0,hexGroup[i].transform.DOMove(hexGroup[(i + 1) % hexGroup.Length].transform.position, GameConstants.RotationDuration));
            }

            var tempGridIndex = hexGroup[0].GridIndex;
            hexGroup[0].GridIndex = hexGroup[1].GridIndex;
            hexGroup[1].GridIndex = hexGroup[2].GridIndex;
            hexGroup[2].GridIndex = tempGridIndex;

            hexGroup[0].CalculateNeighbourIndices();
            hexGroup[1].CalculateNeighbourIndices();
            hexGroup[2].CalculateNeighbourIndices();
            
            // Update HexArray
            GridManager.Instance.HexArray[hexGroup[0].GridIndex.x, hexGroup[0].GridIndex.y] = hexGroup[0];
            GridManager.Instance.HexArray[hexGroup[1].GridIndex.x, hexGroup[1].GridIndex.y] = hexGroup[1];
            GridManager.Instance.HexArray[hexGroup[2].GridIndex.x, hexGroup[2].GridIndex.y] = hexGroup[2];

        }
        else if (direction == (int)RotationDirection.AntiClockwise)
        {
            for (var i = 0; i < hexGroup.Length; i++)
            {
                sequence.Insert(0,hexGroup[i].transform.DOMove(hexGroup[(i - 1 + 3) % hexGroup.Length].transform.position, GameConstants.RotationDuration));
            }
            
            var tempGridIndex = hexGroup[2].GridIndex;
            hexGroup[2].GridIndex = hexGroup[1].GridIndex;
            hexGroup[1].GridIndex = hexGroup[0].GridIndex;
            hexGroup[0].GridIndex = tempGridIndex;
            
            hexGroup[0].CalculateNeighbourIndices();
            hexGroup[1].CalculateNeighbourIndices();
            hexGroup[2].CalculateNeighbourIndices();

            // Update HexArray
            GridManager.Instance.HexArray[hexGroup[0].GridIndex.x, hexGroup[0].GridIndex.y] = hexGroup[0];
            GridManager.Instance.HexArray[hexGroup[1].GridIndex.x, hexGroup[1].GridIndex.y] = hexGroup[1];
            GridManager.Instance.HexArray[hexGroup[2].GridIndex.x, hexGroup[2].GridIndex.y] = hexGroup[2];

        }

        while (sequence.active)
        {
            yield return null;
        }

        yield return new WaitForSeconds(RotationDuration/4);
    }
    
    private IEnumerator RotateHexGroupOutline(int direction)
    {
        var angle = direction == (int) RotationDirection.Clockwise ? -120 : 120;
        
        var tween = HexGroupOutline.DORotate(new Vector3(0, 0, angle), RotationDuration, RotateMode.LocalAxisAdd);
        while (tween.active)
        {
            yield return null;
        }
    }
    
    
    private int GetRotationDirection()
    {
        var selectedCorner = GridManager.Instance.CornerArray[SelectedCornerIndex.x, SelectedCornerIndex.y];
        if (!selectedCorner) 
            return -1;
        
        // Released on left side of hex.
        if (selectedCorner.position.x > _currentTouchPosition.x)
        {
            if (_firstTouchPosition.y > _currentTouchPosition.y)
            {
                return (int)RotationDirection.AntiClockwise;
            }
            return (int)RotationDirection.Clockwise;
        }
        // Released on Right side of hex.
        else
        {
            if (_firstTouchPosition.y > _currentTouchPosition.y)
            {
                return (int)RotationDirection.Clockwise;
            }
            return (int)RotationDirection.AntiClockwise;
        }
    }


    public void SelectCorner(Vector2Int cornerIndex)
    {
        ShowHexGroupOutline(cornerIndex);
    }
    
    public void ReleaseCorner()
    {
        SelectedCornerIndex = new Vector2Int(-1, -1);
        HideHexGroupOutline();
    }
    
    public void ShowHexGroupOutline(Vector2Int cornerIndex)
    {
        HexGroupOutline.gameObject.SetActive(true);
            
        HexGroupOutline.position = GridManager.Instance.CornerArray[cornerIndex.x, cornerIndex.y].position;
        if ((cornerIndex.y % 2 != 0 && cornerIndex.x % 2 != 0) || (cornerIndex.y % 2 == 0 && cornerIndex.x % 2 == 0))
        {
            HexGroupOutline.rotation = Quaternion.Euler(new Vector3(0,0,0));
        }
        else
        {
            HexGroupOutline.rotation = Quaternion.Euler(new Vector3(0,0,60));
        }
    }

    public void HideHexGroupOutline()
    {
        HexGroupOutline.gameObject.SetActive(false);
    }

    
    /// <summary>
    /// Functions related to score management.
    /// </summary>
    #region Score Management

    public void CalculateScore()
    {
        Score = explodedHexagonCount * ExplosionScore;
    }


    #endregion
    
    
    
    
    
}
