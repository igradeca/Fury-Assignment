﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public static PathFinder instance;

    public PathFinder()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private List<CellData> open;
    private List<CellData> closed;

    private const int DIAGONAL_COST = 14;
    private const int HORIZONTAL_OR_VERTICAL_COST = 10;

    // debug stuff
    public List<CellData[][]> DebugGridSteps;
    public int StepIndex = -1;

    // A* algorithm
    public List<CellData> Solve(CellData startLocation, CellData endLocation)
    {
        open = new List<CellData>() { startLocation }; // green
        closed = new List<CellData>();  // red

        DebugGridSteps = new List<CellData[][]>();
        StepIndex = 0;

        startLocation.G = 0;
        startLocation.H = CalculateDistance(startLocation.Location, endLocation.Location);
        startLocation.F = startLocation.G + startLocation.H;

        while (open.Count > 0)
        {
            var current = GetSmallestFCell();

            // End of algorithm - path found
            if (current == endLocation)
            {
                // Debug 
                //DebugSave(grid);
                return GetPath(current);
            }

            open.Remove(current);
            closed.Add(current);

            foreach (var cell in current.Neighbours)
            {
                if (closed.Contains(cell))
                {
                    continue;
                }
                if (cell.NotTraversable)
                {
                    closed.Add(cell);
                    continue;
                }

                var tempG = current.G + CalculateDistance(current.Location, cell.Location);
                if (tempG < cell.G)
                {
                    cell.CameFrom = current;
                    cell.G = tempG;
                    cell.H = CalculateDistance(cell.Location, endLocation.Location);
                    cell.F = cell.G + cell.H;

                    if (!open.Contains(cell))
                    {
                        open.Add(cell);
                    }
                }
            }

            // Debug 
            //DebugSave(grid);
        }

        // Fail
        return null;
    }

    public void ResetGrid(Cell[][] grid)
    {
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid[0].Length; j++)
            {
                grid[i][j].data.ResetValues();
            }
        }
    }

    private int CalculateDistance(Vector2Int start, Vector2Int end)
    {
        var xDistance = Mathf.Abs(start.x - end.x);
        var yDistance = Mathf.Abs(start.y - end.y);
        var remaining = Mathf.Abs(xDistance - yDistance);

        var result = DIAGONAL_COST * Mathf.Min(xDistance, yDistance);
        result += HORIZONTAL_OR_VERTICAL_COST * remaining;

        return result;
    }

    private List<CellData> GetPath(CellData endCell)
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

    private CellData GetSmallestFCell()
    {
        CellData result = open[0];

        foreach (var cell in open)
        {
            if (cell.F < result.F)
            {
                result = cell;
            }
        }

        return result;
    }

    public void DebugSave(Cell[][] grid)
    {
        var newGridState = new CellData[grid.Length][];
        for (int i = 0; i < grid.Length; i++)
        {
            newGridState[i] = new CellData[grid[i].Length];
            for (int j = 0; j < grid[i].Length; j++)
            {
                var gridElem = grid[i][j];

                var newElem = new CellData();
                newElem.G = gridElem.data.G;
                newElem.H = gridElem.data.H;
                newElem.F = gridElem.data.F;

                newGridState[i][j] = newElem;
            }
        }

        DebugGridSteps.Add(newGridState);
    }

}
