using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EditorUtils
{
    public static class Astar
    {
        /// <summary>
        /// TODO: Implement this function so that it returns a list of Vector2Int positions which describes a path
        /// Note that you will probably need to add some helper functions
        /// from the startPos to the endPos
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static List<Vector2Int> FindPathToTarget(Vector2Int startPos, Vector2Int endPos, Cell[,] grid)
        {
            var nodeGrid = CreateNodeGrid(grid, endPos);

            var openList = new List<Node>();
            var closedList = new List<Node>();

            // Create a node from the starting cell
            var startNode = nodeGrid[startPos.x, startPos.y];
            startNode.GScore = 0;
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // FIND NODE WITH LOWEST FSCORE
                if (openList.Count > 1) openList = openList.OrderBy(node => node.FScore).ToList();
                var current = openList[0];

                // CHECK IF END REACHED
                if (current.position == endPos)
                {
                    return BacktrackPositions(current, startNode);
                }

                // VISIT CURRENT NODE
                closedList.Add(current);
                openList.Remove(current);

                // GET NEIGHBOUR NODES
                var neighbours = new List<Node>();
                foreach (var cell in FindAvailableNeighbours(grid[current.position.x, current.position.y], grid).ToList())
                {
                    // Get nodes corresponding to neighbour cells
                    neighbours.Add(nodeGrid[cell.gridPosition.x, cell.gridPosition.y]);
                }

                foreach (var node in neighbours)
                {
                    if (closedList.Contains(node))
                    {
                        continue;
                    }

                    var tentativeG = current.GScore + 1;

                    if (tentativeG < node.GScore)
                    {
                        node.parent = current;
                        node.GScore = tentativeG;
                        if (!openList.Contains(node))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }

            return new List<Vector2Int>();
        }

        public static bool HasPath(Vector2Int from, Vector2Int to, Cell[,] grid)
        {
            var nodeGrid = CreateNodeGrid(grid, to);

            var openList = new List<Node>();
            var closedList = new List<Node>();

            // Create a node from the starting cell
            var startNode = nodeGrid[from.x, from.y];
            startNode.GScore = 0;
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                // FIND NODE WITH LOWEST FSCORE
                if (openList.Count > 1) openList = openList.OrderBy(node => node.FScore).ToList();
                var current = openList[0];

                // CHECK IF END REACHED
                if (current.position == to)
                {
                    return true;
                }

                // VISIT CURRENT NODE
                closedList.Add(current);
                openList.Remove(current);

                // GET NEIGHBOUR NODES
                var neighbours = new List<Node>();
                foreach (var cell in FindAvailableNeighbours(grid[current.position.x, current.position.y], grid).ToList())
                {
                    // Get nodes corresponding to neighbour cells
                    neighbours.Add(nodeGrid[cell.gridPosition.x, cell.gridPosition.y]);
                }

                foreach (var node in neighbours)
                {
                    if (closedList.Contains(node))
                    {
                        continue;
                    }

                    var tentativeG = current.GScore + 1;

                    if (tentativeG < node.GScore)
                    {
                        node.parent = current;
                        node.GScore = tentativeG;
                        if (!openList.Contains(node))
                        {
                            openList.Add(node);
                        }
                    }
                }
            }

            return false;
        }

        private static List<Vector2Int> BacktrackPositions(Node start, Node end)
        {
            var positions = new List<Vector2Int>();

            var currentNode = start;
            do
            {
                positions.Add(currentNode.position);
                currentNode = currentNode.parent;
            } while (currentNode != end);

            positions.Reverse();
            return positions;
        }

        private static Node[,] CreateNodeGrid(Cell[,] cellGrid, Vector2Int endPos)
        {
            var nodeGrid = new Node[cellGrid.GetLength(0), cellGrid.GetLength(1)];

            foreach (var cell in cellGrid)
            {
                var pos = cell.gridPosition;
                nodeGrid[pos.x, pos.y] = new Node(pos, null, Int32.MaxValue, CalcHScore(pos, endPos));
            }

            return nodeGrid;
        }

        private static IEnumerable<Cell> FindAvailableNeighbours(Cell cell, Cell[,] grid)
        {
            var neighbours = new List<Cell>();

            if (!cell.HasWall(Wall.UP))
            {
                neighbours.Add(grid[cell.gridPosition.x, cell.gridPosition.y - 1]);
            }

            if (!cell.HasWall(Wall.DOWN))
            {
                neighbours.Add(grid[cell.gridPosition.x, cell.gridPosition.y + 1]);
            }

            if (!cell.HasWall(Wall.LEFT))
            {
                neighbours.Add(grid[cell.gridPosition.x + 1, cell.gridPosition.y]);
            }

            if (!cell.HasWall(Wall.RIGHT))
            {
                neighbours.Add(grid[cell.gridPosition.x - 1, cell.gridPosition.y]);
            }

            return neighbours;
        }

        private static int CalcHScore(Vector2Int from, Vector2Int to)
        {
            // Using manhattan distance as heuristic
            return Mathf.Abs(to.x - from.x) + Mathf.Abs(to.y - from.y);
        }

        /// <summary>
        /// This is the Node class you can use this class to store calculated FScores for the cells of the grid, you can leave this as it is
        /// </summary>
        public class Node
        {
            public Vector2Int position; //Position on the grid
            public Node parent; //Parent Node of this node

            public float FScore
            {
                //GScore + HScore
                get { return GScore + HScore; }
            }

            public float GScore; //Current Travelled Distance
            public float HScore; //Distance estimated based on Heuristic

            public Node() { }

            public Node(Vector2Int position, Node parent, int GScore, int HScore)
            {
                this.position = position;
                this.parent = parent;
                this.GScore = GScore;
                this.HScore = HScore;
            }
        }
    }
}

[Serializable]
public class Cell
{
    public Vector2Int gridPosition;
    public Wall walls; //bit Encoded

    public void RemoveWall(Wall wallToRemove)
    {
        walls = (walls & ~wallToRemove);
    }

    public int GetNumWalls()
    {
        int numWalls = 0;
        if ((walls & Wall.DOWN) != 0)
        {
            numWalls++;
        }

        if ((walls & Wall.UP) != 0)
        {
            numWalls++;
        }

        if ((walls & Wall.LEFT) != 0)
        {
            numWalls++;
        }

        if ((walls & Wall.RIGHT) != 0)
        {
            numWalls++;
        }

        return numWalls;
    }

    public bool HasWall(Wall wall)
    {
        return (walls & wall) != 0;
    }
}

[Flags]
public enum Wall
{
    LEFT = 0x1,
    UP = 0x2,
    RIGHT = 0x4,
    DOWN = 0x8
}
