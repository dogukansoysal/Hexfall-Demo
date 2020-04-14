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
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static singleton
    public static Camera Cam;
    public Canvas Canvas;
    
    public Level CurrentLevel;
    public Transform HexagonGroupOutline;
    
    public Vector2Int SelectedCornerIndex;
    public GameState GameState;

        
    /* Input Management */
    // TODO: move to another script.
    private Vector2 _firstInputPosition;
    private Vector2 _currentInputPosition;

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
            CalculateCurrentInputPosition();
            _firstInputPosition = _currentInputPosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CalculateCurrentInputPosition();
            if (Vector2.Distance(_firstInputPosition, _currentInputPosition) < 2f)
            {
                SelectedCornerIndex = GridManager.Instance.FindClosestCornerIndex(_firstInputPosition);
                SelectCorner(SelectedCornerIndex);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            CalculateCurrentInputPosition();
            // Select Corner
            if (Vector2.Distance(_firstInputPosition, _currentInputPosition) > 3f)
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
        FinishGame();
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
    
    
    
    private void CalculateCurrentInputPosition(){
        _currentInputPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        _currentInputPosition = Cam.ScreenToWorldPoint(_currentInputPosition);
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

                    var scoreBeforeUpdate = Score/1000;
                    
                    UserInterfaceManager.Instance.UpdateScoreText();
                    yield return GridManager.Instance.CheckEveryHexagonIfUnderIsEmpty();
                    
                    var bombSpawnCount = (Score/1000) - scoreBeforeUpdate;
                    Coroutine coroutine = null;
                    foreach (var hex in hexList)
                    {
                        var spawnAsBomb = false;
                        if (bombSpawnCount > 0)
                        {
                            spawnAsBomb = true;
                            bombSpawnCount--;
                        }
                        coroutine = StartCoroutine(hex.SpawnNewHexagon(spawnAsBomb));
                        GridManager.Instance.FillGridCell(hex.GridIndex, hex);
                    }

                    yield return coroutine;
                    hexList = GridManager.Instance.CheckEveryCorner();
                    yield return new WaitForSeconds(CornerCheckDuration);
                }
                GridManager.Instance.CheckEveryHexagonIfBomb();
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
        
        var tween = HexagonGroupOutline.DORotate(new Vector3(0, 0, angle), RotationDuration, RotateMode.LocalAxisAdd);
        while (tween.active)
        {
            yield return null;
        }
    }
    
    
    
    /// <summary>
    /// Calculate the rotation direction with angles.
    /// 
    /// RULE:
    ///     If last input angle > first input angle then the rotation is anti-clockwise
    /// EXCEPTION:
    ///     Due to 360 degree coordinate system, angle between 0 to 355 is actually 1 degree, not 355.
    ///     So the function assumes that, if SIGNED angle between inputs greater then 180, it will be clockwise rotation.
    /// </summary>
    /// <returns> Rotation Direction in integer.</returns>
    private int GetRotationDirection()
    {
        var selectedCorner = GridManager.Instance.CornerArray[SelectedCornerIndex.x, SelectedCornerIndex.y];
        var heading = (Vector3)_firstInputPosition - selectedCorner.position;
        var direction = heading / heading.magnitude;
        
        var firstTouchAngle = Vector3.SignedAngle(selectedCorner.right, direction, Vector3.forward);
        firstTouchAngle += firstTouchAngle < 0 ? 360 : 0;
            
        heading = (Vector3)_currentInputPosition - selectedCorner.position;
        direction = heading / heading.magnitude;
        
        var currentTouchAngle = Vector3.SignedAngle(selectedCorner.right, direction, Vector3.forward);
        currentTouchAngle += currentTouchAngle < 0 ? 360 : 0;

        // Touch started from top right and finished bottom right
        if (currentTouchAngle - firstTouchAngle > 180)
            return (int)RotationDirection.Clockwise;
        
        // Touch started from bottom right and finished top right
        if(currentTouchAngle - firstTouchAngle < -180)
            return (int)RotationDirection.AntiClockwise;
        
        if (currentTouchAngle > firstTouchAngle)
            return (int) RotationDirection.AntiClockwise;
        
        return (int) RotationDirection.Clockwise;
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
        HexagonGroupOutline.gameObject.SetActive(true);
            
        HexagonGroupOutline.position = GridManager.Instance.CornerArray[cornerIndex.x, cornerIndex.y].position;
        if ((cornerIndex.y % 2 != 0 && cornerIndex.x % 2 != 0) || (cornerIndex.y % 2 == 0 && cornerIndex.x % 2 == 0))
        {
            HexagonGroupOutline.rotation = Quaternion.Euler(new Vector3(0,0,0));
        }
        else
        {
            HexagonGroupOutline.rotation = Quaternion.Euler(new Vector3(0,0,60));
        }
    }

    
    
    public void HideHexGroupOutline()
    {
        HexagonGroupOutline.gameObject.SetActive(false);
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
