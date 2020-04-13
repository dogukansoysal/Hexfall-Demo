using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using ScriptableObject;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; // static singleton

    public Sprite[] HexagonSprite;
    public Sprite[] BombSprite;
    
    [SerializeField]
    public Transform HexPrefab;
    [SerializeField]
    public Transform CornerPrefab;
    [SerializeField]
    public Transform BombTextPrefab;

    
    private float _hexWidth = 1.732f;
    private float _hexHeight = 1f;

    private Transform _gridHolder;
    private Transform _cornerHolder;

    private Hexagon[,] _hexArray;
    private Transform[,] _cornerArray;

    void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
        }else{
            Instance = this;
        }
    }
    
    // Initialize Grid
    public void InitGrid(Level level)
    {
        // TODO: this scale will be re-calculated depending on hex width and height to fit in screen.
        FixHexagonScale();
        
        _hexWidth *= HexPrefab.transform.lossyScale.x;
        _hexHeight *= HexPrefab.transform.lossyScale.y;
        
        // Grid Holder is being used as start position of the grid. 
        _gridHolder = transform.GetChild(0);
        // Corner Holder is being used for organized display.
        _cornerHolder = transform.GetChild(1);

        _hexArray = new Hexagon[level.GridWidth, level.GridHeight];
        _cornerArray = new Transform[level.GridWidth, level.GridHeight * 2];

        AddGap(level.Gap);
        CreateGrid(level.GridWidth, level.GridHeight, level.ColorCount);
        CreateCornerGrid(level.GridWidth, level.GridHeight);
        SpawnCheckForEveryCorner(level.ColorCount);

        
    }

    public void FixHexagonScale()
    {
        var scaleRatio = GameConstants.ScaleRatio / GameManager.Instance.CurrentLevel.GridWidth;
        HexPrefab.localScale = Vector3.one * scaleRatio;
        CornerPrefab.localScale = Vector3.one * scaleRatio;
        BombTextPrefab.localScale = Vector3.one * scaleRatio;
        GameManager.Instance.HexagonGroupOutline.localScale = Vector3.one * scaleRatio;
    }
    
    public Vector3 CalculateWorldPosition(Vector2Int gridPosition)
    {
        var offset = 0f;
        if ((int)gridPosition.x % 2 != 0)
            offset = _hexHeight / 2;

        var x = _gridHolder.position.x + gridPosition.x * (_hexWidth/2);
        var y = _gridHolder.position.y + gridPosition.y * _hexHeight + offset;

        return new Vector3(x, y, 0);
    }
    
    private void AddGap(float gap)
    {
        _hexWidth += _hexWidth * gap;
        _hexHeight += _hexHeight * gap;
    }

    private void CreateGrid(int gridWidth, int gridHeight, int colorCount)
    {
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                CreateHexagon(new Vector2Int(x,y), Random.Range(0, colorCount));
            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <param name="colorIndex"></param>
    public void CreateHexagon(Vector2Int gridPosition, int colorIndex)
    {
        var hex = Instantiate(HexPrefab);
        hex.GetComponent<Hexagon>().GridIndex = gridPosition;
        hex.GetComponent<Hexagon>().CalculateNeighbourIndices();
        hex.GetComponent<Hexagon>().ChangeHexagonColor(HexagonSprite[colorIndex], colorIndex);
        hex.position = CalculateWorldPosition(new Vector2Int(gridPosition.x, gridPosition.y));
        hex.parent = _gridHolder;
        //hex.name = "Hexagon" + gridPosition.x + "|" + gridPosition.y;
        
        if(_hexArray[gridPosition.x, gridPosition.y] != null)
            Destroy(_hexArray[gridPosition.x, gridPosition.y].gameObject);
        
        _hexArray[gridPosition.x, gridPosition.y] = hex.GetComponent<Hexagon>();
    }

    private void CreateCornerGrid(int gridWidth, int gridHeight)
    {
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                if(y == 0 && x % 2 == 0)
                    continue;
                if(y == gridHeight - 1 && x % 2 != 0)
                    continue;

                //Init Left
                if (x != 0)
                {
                    var corner = Instantiate(CornerPrefab);
                    corner.position = CalculateWorldPosition(new Vector2Int(x,y));
                    corner.position = new Vector2(corner.position.x - (_hexWidth / 3), corner.position.y);
                    corner.parent = _cornerHolder;

                    if (x % 2 == 0)
                    {
                        _cornerArray[x, (y * 2)] = corner;
                        corner.name = "Corner" + x + "|" + (y * 2) ;
                    }
                    else
                    {
                        _cornerArray[x, (y * 2) + 1] = corner;
                        corner.name = "Corner" + x + "|" + ((y * 2) + 1);
                    }
                }
        
                //Init Right
                if (x != gridWidth - 1)
                {
                    var corner = Instantiate(CornerPrefab);
                    corner.position = CalculateWorldPosition(new Vector2Int(x,y));
                    corner.position = new Vector2(corner.position.x + (_hexWidth / 3), corner.position.y);
                    corner.parent = _cornerHolder;

                    if (x % 2 == 0)
                    {
                        _cornerArray[(x + 1), (y * 2)] = corner;
                        corner.name = "Corner" + (x + 1)  + "|" +  (y * 2);
                    }
                    else
                    {
                        _cornerArray[(x + 1), ((y * 2) + 1)] = corner;
                        corner.name = "Corner" + (x + 1) + "|" + ((y * 2) + 1);
                    }
                }
            }
        }
    }

    public Vector2Int FindClosestCornerIndex(Vector2 touchPosition)
    {
        var closestDistance = float.MaxValue;
        var closestCornerIndex = new Vector2Int(-1,-1);
        
        for (var x = 0; x < _cornerArray.GetLength(0); x++)
        {
            for (var y = 0; y < _cornerArray.GetLength(1); y++)
            {
                if (!_cornerArray[x, y]) continue;
                
                var tempDistance = Vector2.Distance(touchPosition, _cornerArray[x, y].position);
                if (tempDistance < closestDistance)
                {
                    closestDistance = tempDistance;
                    closestCornerIndex = new Vector2Int(x, y);
                }
            }
        }
        return closestCornerIndex;
    }
    
    
    

    /// <summary>
    /// If Corner Index Y is odd;
    ///    and If Corner Index X is odd then there is 2 left of it
    /// Else vice versa.
    /// </summary>
    /// <param name="cornerIndex"> </param>
    /// <returns>Hexagon Group with Clockwise Order</returns>
    public Hexagon[] FindHexGroup(Vector2Int cornerIndex)
    {
        Hexagon[] hexGroup = new Hexagon[3];

        if (cornerIndex.y % 2 != 0)
        {
            if (cornerIndex.x % 2 != 0)
            {
                hexGroup[0] = _hexArray[cornerIndex.x - 1, cornerIndex.y / 2];    // Left Bottom
                hexGroup[1] = _hexArray[cornerIndex.x - 1, (cornerIndex.y / 2) + 1];    // Left Top
                hexGroup[2] = _hexArray[cornerIndex.x, cornerIndex.y / 2];    // Right
            }
            else
            {
                hexGroup[0] = _hexArray[cornerIndex.x - 1, (cornerIndex.y / 2)];    // Left
                hexGroup[1] = _hexArray[cornerIndex.x, (cornerIndex.y / 2) + 1];    // Right Top
                hexGroup[2] = _hexArray[cornerIndex.x, cornerIndex.y / 2];    // Right bottom
            }
        }
        else
        {
            if (cornerIndex.x % 2 != 0)
            {
                hexGroup[0] = _hexArray[cornerIndex.x - 1, (cornerIndex.y / 2)];    // Left
                hexGroup[1] = _hexArray[cornerIndex.x, cornerIndex.y / 2];    // Right top
                hexGroup[2] = _hexArray[cornerIndex.x, (cornerIndex.y / 2) - 1];    // Right bottom
            }
            else
            {
                hexGroup[0] = _hexArray[cornerIndex.x - 1, (cornerIndex.y / 2) - 1];    // Left bottom
                hexGroup[1] = _hexArray[cornerIndex.x - 1, (cornerIndex.y / 2)];    // Left Top
                hexGroup[2] = _hexArray[cornerIndex.x, cornerIndex.y / 2];    // Right
            }
        }
        
        return hexGroup;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colorCount"></param>
    public void SpawnCheckForEveryCorner(int colorCount)
    {

        var hexList = CheckEveryCorner();

        while (hexList.Count > 0)
        {
            foreach (var hex in hexList)
            {
                var colorIndex = Random.Range(0, colorCount);
                hex.ChangeHexagonColor(HexagonSprite[colorIndex], colorIndex);
            }
            
            hexList = CheckEveryCorner();
        }
        
        for (var y = 0; y < _cornerArray.GetLength(1); y++)
        {
            for (var x = 0; x < _cornerArray.GetLength(0); x++)
            {
                if (_cornerArray[x, y] == null) continue;

                var hexGroup = FindHexGroup(new Vector2Int(x,y));
                if (hexGroup[0].GetComponent<Hexagon>().ColorIndex == hexGroup[1].GetComponent<Hexagon>().ColorIndex &&
                    hexGroup[1].GetComponent<Hexagon>().ColorIndex == hexGroup[2].GetComponent<Hexagon>().ColorIndex)
                {
                    var exceptionColorIndex = hexGroup[0].GetComponent<Hexagon>().ColorIndex;
                    var newColorIndex = GenerateRandomIntegerWithException(0, colorCount, exceptionColorIndex);
                    
                    hexGroup[Random.Range(0,3)].GetComponent<Hexagon>().ChangeHexagonColor(HexagonSprite[newColorIndex], newColorIndex);

                    y = 0;
                    x = 0;
                }
            }
        }
    }
    
    public List<Hexagon> CheckEveryCorner()
    {
        var hexList = new List<Hexagon>();
        
        for (var y = 0; y < _cornerArray.GetLength(1); y++)
        {
            for (var x = 0; x < _cornerArray.GetLength(0); x++)
            {
                if (!_cornerArray[x, y]) continue;

                var hexGroup = FindHexGroup(new Vector2Int(x,y));
                if (hexGroup[0].ColorIndex == hexGroup[1].ColorIndex &&
                    hexGroup[1].ColorIndex == hexGroup[2].ColorIndex)
                {
                    foreach (var hex in hexGroup)
                    {
                        if(!hexList.Exists(match => match == hex))
                            hexList.Add(hex);
                    }
                }
            }
        }
        return hexList;
    }

    public IEnumerator CheckEveryHexagonIfUnderIsEmpty()
    {
        Coroutine coroutine = null;
        for (var y = 1; y < _hexArray.GetLength(1); y++)
        {
            for (var x = 0; x < _hexArray.GetLength(0); x++)
            {
                // if current is empty continue..
                if (!_hexArray[x, y]) continue;
                
                // if under hex is empty
                if (!_hexArray[x, y - 1])
                {
                    coroutine = StartCoroutine(_hexArray[x, y].DropHexagon(new Vector2Int(x, y - 1), false));

                    FillGridCell(new Vector2Int(x, y - 1), _hexArray[x, y]);
                    ClearGridCell(new Vector2Int(x, y));
                    x = 0;
                    y = 1;
                }
            }
        }

        if (coroutine != null)
            yield return StartCoroutine(CheckEveryHexagonIfUnderIsEmpty());
    }
    

    public void CheckEveryHexagonIfBomb()
    {
        for (var y = 1; y < _hexArray.GetLength(1); y++)
        {
            for (var x = 0; x < _hexArray.GetLength(0); x++)
            {
                // if current is empty continue..
                if (!_hexArray[x, y]) continue;
                
                if (_hexArray[x,y].transform.GetComponent<Bomb>())
                {
                    _hexArray[x,y].transform.GetComponent<Bomb>().DecreaseRemainingMove();
                }
            }
        }
    }

    public void FillGridCell(Vector2Int gridIndex, Hexagon hexagon)
    {
        HexArray[gridIndex.x, gridIndex.y] = hexagon;
    }
    
    public void ClearGridCell(Vector2Int gridIndex)
    {
        HexArray[gridIndex.x, gridIndex.y] = null;
    }
    
    
    
    
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool CheckEveryCornerIfPossibleMove()
    {
        for (var y = 0; y < _cornerArray.GetLength(1); y++)
        {
            for (var x = 0; x < _cornerArray.GetLength(0); x++)
            {
                if (_cornerArray[x, y] == null) continue;

                var hexGroup = FindHexGroup(new Vector2Int(x,y));
                var sameColorIndex = -1;
                
                if (hexGroup[0].GetComponent<Hexagon>().ColorIndex == 
                    hexGroup[1].GetComponent<Hexagon>().ColorIndex)
                {
                    
                }
                else if (hexGroup[0].GetComponent<Hexagon>().ColorIndex ==
                         hexGroup[1].GetComponent<Hexagon>().ColorIndex)
                {
                    
                }else if (hexGroup[0].GetComponent<Hexagon>().ColorIndex ==
                          hexGroup[1].GetComponent<Hexagon>().ColorIndex)
                {
                    
                        
                }
                    
                    
            }
        }

        return false;
    }
    
    public bool CheckEveryNeighbourHex(Transform[] hexGroup, Transform centerHex, int colorIndex)
    {
        var neighbourIndices = centerHex.GetComponent<Hexagon>().NeighbourIndices;
        
        var hexGroupIndices = new Vector2Int[3];
        hexGroupIndices[0] = hexGroup[0].GetComponent<Hexagon>().GridIndex;
        hexGroupIndices[1] = hexGroup[1].GetComponent<Hexagon>().GridIndex;
        hexGroupIndices[2] = hexGroup[2].GetComponent<Hexagon>().GridIndex;

        
        foreach (var neighbourIndex in neighbourIndices)
        {
            if (neighbourIndex.x < 0 || neighbourIndex.y < 0) continue;
            if (_hexArray[neighbourIndex.x, neighbourIndex.y] == null) continue;
            if(neighbourIndex == hexGroupIndices[0]) continue;
            if(neighbourIndex == hexGroupIndices[1]) continue;
            if(neighbourIndex == hexGroupIndices[2]) continue;
            if (_hexArray[neighbourIndex.x, neighbourIndex.y].ColorIndex == colorIndex)
                return true;
        }

        return false;
    }

    
    
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cornerIndex"></param>
    /// <returns></returns>
    public Vector3Int GetHexGroupColorsByCorner(Vector2Int cornerIndex)
    {
        var hexGroup = FindHexGroup(cornerIndex);
        return new Vector3Int(hexGroup[0].GetComponent<Hexagon>().ColorIndex, 
            hexGroup[1].GetComponent<Hexagon>().ColorIndex, 
            hexGroup[2].GetComponent<Hexagon>().ColorIndex);
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cornerIndex"></param>
    /// <returns></returns>
    public Vector2Int[] GetHexGroupGridIndicesByCorner(Vector2Int cornerIndex)
    {
        var hexGroup = FindHexGroup(cornerIndex);
        Vector2Int[] hexGroupGridIndices = new Vector2Int[3];
        
        hexGroupGridIndices[0] = hexGroup[0].GetComponent<Hexagon>().GridIndex;
        hexGroupGridIndices[1] = hexGroup[1].GetComponent<Hexagon>().GridIndex;
        hexGroupGridIndices[2] = hexGroup[2].GetComponent<Hexagon>().GridIndex;

        return hexGroupGridIndices;
    }




    /// <summary>
    /// A Test function for grid.
    /// </summary>
    public void PrintGrid()
    {
        for (var x = 0; x < _hexArray.GetLength(0); x++)
        {
            var tempString = "";
            for (var y = 0; y < _hexArray.GetLength(1); y++)
            {
                if (_hexArray[x, y])
                {
                    tempString += " X ";
                }
                else
                {
                    tempString += " - ";
                }
            }
            Debug.Log(tempString);
        }
    }
    
    
    
    #region Helper Functions

    /// <summary>
    /// Generate a random integer in desired range with exception value.
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    /// <param name="exceptionValue"></param>
    /// <returns></returns>
    public static int GenerateRandomIntegerWithException(int minRange, int maxRange, int exceptionValue)
    {
        var value = exceptionValue;
        while (value == exceptionValue)
        {
            value = Random.Range(minRange, maxRange);
        }
        
        return value;
    }



    #endregion




    #region Test Functions

        /// <summary>
        /// Test Function for functionality
        /// </summary>
        /// <param name="index">Corner Index</param>
        public void TempDestroyFunc(Vector2Int index)
        {
            var hexGroup = FindHexGroup(index);
            foreach (var hex in hexGroup)
            {
                if(hex != null)
                    Destroy(hex.gameObject);
            }
        }

    #endregion



    
    
    
    #region Getters & Setters

        public Hexagon[,] HexArray
        {
            get => _hexArray;
            set => _hexArray = value;
        }
    
        public Transform[,] CornerArray
        {
            get => _cornerArray;
            set => _cornerArray = value;
        }

    #endregion
    
}
