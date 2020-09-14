using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public Vector2Int Location;
    public List<CellData> Neighbours;
    public CellData CameFrom;

    public bool NotTraversable;

    public int G = int.MaxValue;    // Element distance from starting node (left number)
    public int H = -1;              // Heuristic distance from end node (right number)
    public int F = -1;              // G + H

    public void ResetValues()
    {
        CameFrom = null;

        G = int.MaxValue;
        H = -1;
        F = -1;
    }
}

public class Cell
{
    public CellData data;
    public GameObject GO;
    public ElementDebug debug;

    public Cell(GameObject go, Vector2Int location, bool notTraversable = false)
    {
        GO = go;
        data = new CellData { Location = location, NotTraversable = notTraversable };
        debug = go.transform.GetChild(0).GetComponent<ElementDebug>();
    }
}

public class Grid : MonoBehaviour
{
    public bool DebugMode = true;

    public GameObject gridElement;
    public Vector2Int gridSize;
    public float cellSize;

    public Cell StartCell;
    public Cell EndCell;

    public Cell[][] grid;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        new PathFinder();

        // Set grid
        var initGridPos = new Vector3(cellSize / 2f, cellSize / 2f, 0f);
        grid = new Cell[gridSize.x][];
        for (int i = 0; i < gridSize.x; i++)
        {
            var tempPos = initGridPos;
            tempPos.y += i * cellSize;

            grid[i] = new Cell[gridSize.y];
            for (int j = 0; j < gridSize.y; j++)
            {
                var newElement = Instantiate(gridElement, transform);
                newElement.transform.localPosition = tempPos;
                newElement.transform.localScale = Vector3.one * cellSize;
                newElement.name = "Element_" + i.ToString() + "_" + j.ToString();

                grid[i][j] = new Cell(newElement, new Vector2Int(i, j));

                tempPos.x += cellSize;
            }
        }

        StartCell = grid[0][0];

        //if (DebugMode)
        //{
        //    debugGridStates = new List<Cell[][]>();
        //}

        //for (int i = 0; i < gridSize.x; i++)
        //{
        //    for (int j = 0; j < gridSize.y; j++)
        //    {
        //        Debug.Log(grid[i][j].GO.name + " x: " + i + " y: " + j + " " + grid[i][j].data.Location);
        //    }
        //}

        // Set neighbours
        SetNeighbours();
    }

    private void SetNeighbours()
    {
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                grid[i][j].data.Neighbours = getNeighbours(grid[i][j].data);
            }
        }

        List<CellData> getNeighbours(CellData current)
        {
            List<CellData> result = new List<CellData>();

            if (current.Location.x - 1 >= 0)
            {
                // Left
                result.Add(grid[current.Location.x - 1][current.Location.y].data);
                // Left down
                if (current.Location.y - 1 >= 0)
                {
                    result.Add(grid[current.Location.x - 1][current.Location.y - 1].data);
                }
                // Left up
                if (current.Location.y + 1 < gridSize.y)
                {
                    result.Add(grid[current.Location.x - 1][current.Location.y + 1].data);
                }
            }
            if (current.Location.x + 1 < gridSize.x)
            {
                // Right
                result.Add(grid[current.Location.x + 1][current.Location.y].data);
                // Right down
                if (current.Location.y - 1 >= 0)
                {
                    result.Add(grid[current.Location.x + 1][current.Location.y - 1].data);
                }
                // Right up
                if (current.Location.y + 1 < gridSize.y)
                {
                    result.Add(grid[current.Location.x + 1][current.Location.y + 1].data);
                }
            }
            // Down
            if (current.Location.y - 1 >= 0)
            {
                result.Add(grid[current.Location.x][current.Location.y - 1].data);
            }
            // Up
            if (current.Location.y + 1 < gridSize.y)
            {
                result.Add(grid[current.Location.x][current.Location.y + 1].data);
            }

            for (int i = result.Count - 1; i >= 0; --i)
            {
                if (result[i].NotTraversable)
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("left"))
        {
            if (PathFinder.instance.StepIndex > 0)
            {
                --PathFinder.instance.StepIndex;
                debugGrid();
            }
        }
        if (Input.GetKeyDown("right"))
        {
            if (PathFinder.instance.StepIndex < PathFinder.instance.DebugGridSteps.Count - 1)
            {
                ++PathFinder.instance.StepIndex;
                debugGrid();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            setHitLocation(0);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            setHitLocation(1);
        }
    }

    void OnDrawGizmos()
    {
        if (StartCell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(StartCell.GO.transform.position, 0.5f);
        }

        if (EndCell != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(EndCell.GO.transform.position, 0.5f);
        }        
    }

    private void setHitLocation(int mouseClickIndex)
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //Debug.Log(hit.transform.name + " " + hit.point);
            var point = new Vector2(hit.point.x, hit.point.y);

            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    var cell = grid[i][j];
                    if (Vector2.Distance(point, cell.GO.transform.position) < (cellSize * 0.5f))
                    {
                        //Debug.Log("elementHitLocation: " + cell.GO.transform.position);
                        if (mouseClickIndex == 0)
                        {
                            EndCell = cell;

                            PathFinder.instance.ResetGrid(grid);
                            var result = PathFinder.instance.Solve(StartCell.data, EndCell.data);

                            if (result != null)
                            {
                                for (int k = 0; k < grid.Length; k++)
                                {
                                    for (int m = 0; m < grid[0].Length; m++)
                                    {
                                        var elemData = grid[k][m].data;
                                        var elemSpriteRenderer = grid[k][m].GO.GetComponent<SpriteRenderer>();

                                        if (elemData.NotTraversable)
                                        {
                                            elemSpriteRenderer.color = Color.gray;
                                        }
                                        else
                                        {
                                            var color = result.Contains(elemData) ? Color.blue : Color.white;
                                            elemSpriteRenderer.color = color;
                                        }
                                    }
                                }
                            }
                        }
                        else if (mouseClickIndex == 1)
                        {
                            cell.data.NotTraversable = !cell.data.NotTraversable;
                            var color = cell.data.NotTraversable ? Color.gray : Color.white;
                            cell.GO.GetComponent<SpriteRenderer>().color = color;

                            SetNeighbours();
                        }

                        return;
                    }
                }
            }
        }
    }

    private void debugGrid()
    {
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[i].Length; j++)
            {
                var elem = grid[i][j];
                elem.data = PathFinder.instance.DebugGridSteps[PathFinder.instance.StepIndex][i][j];
                elem.debug.G.text = elem.data.G == int.MaxValue ? "" : elem.data.G.ToString();
                elem.debug.H.text = elem.data.H == -1 ? "" : elem.data.H.ToString();
                elem.debug.F.text = elem.data.F == -1 ? "" : elem.data.F.ToString();
            }
        }
    }
}

