using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class GridLogic : MonoBehaviour
{
    public GameObject gridElement;
    public Vector2Int gridSize;
    public float cellSize;

    public Cell StartCell;
    public Cell EndCell;

    public Cell[][] grid;

    private Camera mainCamera;
    private GameObject floor;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        floor = transform.Find("Floor").gameObject;

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

        //for (int i = 0; i < gridSize.x; i++)
        //{
        //    for (int j = 0; j < gridSize.y; j++)
        //    {
        //        Debug.Log(grid[i][j].GO.name + " x: " + i + " y: " + j + " " + grid[i][j].data.Location);
        //    }
        //}

        // set neighbours
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                grid[i][j].data.Neighbours = getNeighbours(grid[i][j].data);
            }
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

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        //var linePos = transform.position;
        //for (int i = 0; i <= gridSize.x; i++)
        //{
        //    var xLine = linePos;
        //    for (int j = 0; j <= gridSize.y; j++)
        //    {
        //        Debug.DrawLine(xLine, new Vector3(xLine.x, gridSize.y, 0f), Color.white);
        //        xLine.x += cellSize;
        //    }
        //    Debug.DrawLine(linePos, new Vector3(linePos.x , linePos.y + gridSize.y * cellSize, 0f), Color.white);
        //    linePos.y += cellSize;
        //}

        if (Input.GetMouseButtonDown(0))
        {
            setHitLocation(0);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            setHitLocation(1);
        }

        void setHitLocation(int mouseClickIndex)
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
                        if (Vector2.Distance(point, cell.GO.transform.position) < (cellSize * 0.5))
                        {
                            //Debug.Log("elementHitLocation: " + cell.GO.transform.position);

                            if (mouseClickIndex == 0)
                            {
                                StartCell = cell;
                            }
                            else
                            {
                                EndCell = cell;
                            }

                            if (StartCell != null && EndCell != null)
                            {
                                var result = AStarSolver.Solve(grid, StartCell.data, EndCell.data);
                                Debug.Log("Something");

                                for (int k = 0; k < grid.Length; k++)
                                {
                                    for (int m = 0; m < grid[0].Length; m++)
                                    {
                                        var gridElem = grid[k][m];
                                        var color = result.Contains(gridElem.data) ? Color.blue : Color.white;
                                        gridElem.GO.GetComponent<SpriteRenderer>().color = color;
                                    }
                                }
                            }

                            return;
                        }
                    }
                }
            }
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
}

public class CellData
{
    public Vector2Int Location;
    public List<CellData> Neighbours;
    public CellData CameFrom;

    public bool NotTraversable;

    public int G = -1; // Element distance from starting node, diagonal 14, hor/vert 10 (left)
    public int H = -1; // Heuristic distance from end node (right)
    public int F = -1; // G + H

    public void ResetValues()
    {
        CameFrom = null;

        G = -1;
        H = -1;
        F = -1;
    }
}

public class Cell
{
    public CellData data;
    public GameObject GO;

    public Cell(GameObject go, Vector2Int location, bool notTraversable = false)
    {
        GO = go;
        data = new CellData { Location = location, NotTraversable = notTraversable };
    }
}

public static class AStarSolver
{
    public static List<CellData> Open;
    public static List<CellData> Closed;

    private const int DIAGONAL_COST = 14;
    private const int HORIZONTAL_OR_VERTICAL_COST = 10;

    public static List<CellData> Solve(Cell[][] grid, CellData startLocation, CellData endLocation)
    {
        Open = new List<CellData>() { startLocation }; // zeleni
        Closed = new List<CellData>(); // crveni

        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[0].Length; j++)
            {
                grid[i][j].data.ResetValues();
            }
        }

        startLocation.G = 0;
        startLocation.H = CalculateDistance(startLocation.Location, endLocation.Location);
        startLocation.F = startLocation.G + startLocation.H;

        while (Open.Count > 0)
        {
            var current = GetSmallestFCell();

            // Kraj algoritma, nasli smo put
            if (current == endLocation)
            {
                return GetPath(current);
            }
            
            Open.Remove(current);
            Closed.Add(current);

            // Pogledaj svakog susjeda od current i izracunaj njegove vrijednosti
            foreach (var cell in current.Neighbours)
            {
                if (Closed.Contains(cell)) {
                    continue;
                }
                if (cell.NotTraversable)
                {
                    Closed.Add(cell);
                    continue;
                }

                var tempG = CalculateDistance(current.Location, cell.Location);
                if (tempG < cell.G || cell.G == -1)
                {
                    cell.CameFrom = current;
                    cell.G = tempG;
                    cell.H = CalculateDistance(cell.Location, endLocation.Location);
                    cell.F = cell.G + cell.H;

                    if (!Open.Contains(cell))
                    {
                        Open.Add(cell);
                    }
                }
            }

        }

        // Fail
        return null;
    }
    private static int CalculateDistance(Vector2Int start, Vector2Int end)
    {
        var xDistance = Mathf.Abs(start.x - end.x);
        var yDistance = Mathf.Abs(start.y - end.y);
        var remaining = Mathf.Abs(xDistance - yDistance);

        var result = DIAGONAL_COST * Mathf.Min(xDistance, yDistance);
        result += HORIZONTAL_OR_VERTICAL_COST * remaining;
        
        return result;
    }

    private static List<CellData> GetPath(CellData endCell)
    {
        var result = new List<CellData>();

        var currentCell = endCell;
        while (currentCell.CameFrom != null)
        {
            result.Add(currentCell);
            currentCell = currentCell.CameFrom;
        }

        result.Add(currentCell);

        result.Reverse();
        return result;
    }

    private static CellData GetSmallestFCell()
    {
        CellData result = Open[0];

        foreach (var cell in Open)
        {
            if (cell.F < result.F)
            {
                result = cell;
            }
        }

        return result;
    }

}
