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

        public Vector2Int CurrentTile { get; private set; }
        
        private void Start()
        {
            transform.position = grid.StartTilePosition;
            CurrentTile = grid.StartTile;
        }

        public void Move(MoveDirection dir)
        {
            Move(GetMoveVector(dir));
        }

        public void Move(Vector2Int moveVector)
        {
            transform.position = grid.GetTilePosition(CurrentTile + moveVector);
            CurrentTile += moveVector;
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
