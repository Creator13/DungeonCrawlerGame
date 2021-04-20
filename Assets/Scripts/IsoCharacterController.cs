using System;
using UnityEngine;

namespace Dungen
{
    public class IsoCharacterController : MonoBehaviour
    {
        public enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public IsoGrid grid;

        private Vector2Int currentTile = Vector2Int.zero;
        
        private void Start()
        {
            transform.position = grid.StartTilePosition;
        }

        public void Move(MoveDirection dir)
        {
            var moveVector = GetMoveVector(dir);
            transform.position = grid.GetTilePosition(currentTile + moveVector);
            currentTile += moveVector;
        }

        private Vector2Int GetMoveVector(MoveDirection dir)
        {
            var moveVector = Vector2Int.zero;
            
            switch (dir)
            {
                case MoveDirection.Up:
                    moveVector.y = 1;
                    break;
                case MoveDirection.Down:
                    moveVector.y = -1;
                    break;
                case MoveDirection.Left:
                    moveVector.x = -1;
                    break;
                case MoveDirection.Right:
                    moveVector.x = 1;
                    break;
            }

            return moveVector;
        }
    }
}
