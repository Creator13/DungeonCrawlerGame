using System;
using System.Collections;
using System.Collections.Generic;
using Dungen.World;
using UnityEngine;
using EditorUtils;

namespace Dungen.Gameplay
{
    public class IsoEntity : MonoBehaviour
    {
        public enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public IsoGrid grid;

        private Coroutine moveRoutine;
        public bool IsMoving { get; private set; }

        public Tile CurrentTile { get; private set; }

        public event Action MoveFinished;

        private void Start()
        {
            // transform.position = grid.StartTilePosition;
            // CurrentTile = grid.StartTile;
        }

        private void OnDestroy()
        {
            if (CurrentTile)
            {
                CurrentTile.RemoveEntity(this);
            }
        }

        public void MoveOverPath(Vector2Int destination)
        {
            MoveOverPath(grid.GetTilesFromPositions(Astar.FindPathToTarget(CurrentTile.Data, destination, grid.CellGrid)));
        }

        public void MoveOverPath(List<Tile> path)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
            }

            moveRoutine = StartCoroutine(DoMoveOverPath(path));
        }

        private IEnumerator DoMoveOverPath(List<Tile> path)
        {
            IsMoving = true;

            var enumerator = path.GetEnumerator();

            while (enumerator.MoveNext())
            {
                SetTile(enumerator.Current);
                yield return new WaitForSeconds(.2f);
            }

            enumerator.Dispose();

            MoveFinished?.Invoke();
            IsMoving = false;
        }

        // public void Move(MoveDirection dir)
        // {
        //     Move(GetMoveVector(dir));
        // }
        //
        // public void Move(Vector2Int moveVector)
        // {
        //     CurrentTile = grid.GetTileFromPosition(CurrentTile.Data + moveVector);
        //     transform.position = grid.GetTileWorldPosition(CurrentTile);
        // }

        public void SetTile(Tile tile)
        {
            if (CurrentTile) CurrentTile.RemoveEntity(this);
            
            CurrentTile = tile;
            var pos = grid.GetTileWorldPosition(CurrentTile);
            pos.y = .5f;
            transform.position = pos;
            
            tile.AddEntity(this);
        }

        public void SetTile(Vector2Int position)
        {
            SetTile(grid.GetTileFromPosition(position));
        }

        private static Vector2Int GetMoveVector(MoveDirection dir)
        {
            var moveVector = Vector2Int.zero;

            switch (dir)
            {
                case MoveDirection.Up:
                    moveVector.y = -1;
                    break;
                case MoveDirection.Down:
                    moveVector.y = 1;
                    break;
                case MoveDirection.Left:
                    moveVector.x = 1;
                    break;
                case MoveDirection.Right:
                    moveVector.x = -1;
                    break;
            }

            return moveVector;
        }
    }
}
