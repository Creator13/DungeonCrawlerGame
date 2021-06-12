using System;
using System.Collections;
using System.Collections.Generic;
using Dungen.World;
using UnityEngine;

namespace Dungen
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
        public  bool IsMoving { get; private set; }

        public Vector2Int CurrentTile { get; private set; }

        public event Action MoveFinished;

        private void Start()
        {
            // transform.position = grid.StartTilePosition;
            // CurrentTile = grid.StartTile;
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
                // HidePath();

                SetTile(enumerator.Current);
                yield return new WaitForSeconds(.2f);
            }

            enumerator.Dispose();

            MoveFinished?.Invoke();
            IsMoving = false;
        }

        public void Move(MoveDirection dir)
        {
            Move(GetMoveVector(dir));
        }

        public void Move(Vector2Int moveVector)
        {
            CurrentTile += moveVector;
            transform.position = grid.GetTileWorldPosition(CurrentTile);
        }

        public void SetTile(Tile tile)
        {
            CurrentTile = new Vector2Int(tile.X, tile.Y);
            var pos = grid.GetTileWorldPosition(CurrentTile);
            pos.y = .5f;
            transform.position = pos;
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
