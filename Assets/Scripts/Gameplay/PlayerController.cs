using System.Collections.Generic;
using System.Linq;
using Dungen.Netcode;
using Dungen.World;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Dungen
{
    [RequireComponent(typeof(IsoEntity))]
    public class PlayerController : MonoBehaviour, IEntity
    {
        [SerializeField] private IsoFollowCamera followCam;
        [SerializeField] private InputActionAsset playerInputActions;
        [SerializeField] private LayerMask layers;

        private IsoEntity playerEntity;
        private RaycastHit[] hitContainer;

        private Tile currentTargetedTile;
        private List<Tile> currentPath;

        private bool hasTurn;

        private void Awake()
        {
            playerEntity = GetComponent<IsoEntity>();

            BindActions();
        }

        private void BindActions()
        {
            if (!playerInputActions.enabled) playerInputActions.Enable();

            var walk = playerInputActions["Walk"];
            walk.performed += ctx =>
            {
                var moveVectorFloat = -ctx.ReadValue<Vector2>();
                var moveVector = new Vector2Int((int) moveVectorFloat.normalized.x, (int) moveVectorFloat.normalized.y);

                playerEntity.Move(moveVector);
            };

            playerInputActions["PointerClick"].performed += HandleClick;
            playerInputActions["PointerMove"].performed += HandlePointerMove;

            followCam.CameraMoved += OnCameraMoved;
        }

        private void OnValidate()
        {
            // TODO replace this with a custom editor that shows all the input actions
            if (playerInputActions != null && !playerInputActions.Any(action => action.name == "Walk"))
            {
                playerInputActions = null;
            }
        }

        private void OnCameraMoved()
        {
            if (playerEntity.IsMoving) return;

            var screenPos = playerInputActions["PointerMove"].ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandlePointerMove(InputAction.CallbackContext ctx)
        {
            if (!hasTurn) return;

            var screenPos = ctx.ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandleClick(InputAction.CallbackContext ctx)
        {
            if (currentTargetedTile != null && currentPath != null && currentPath.Count > 0 && !playerEntity.IsMoving)
            {
                HidePath();
                currentTargetedTile.SetMarked(false);
                playerEntity.MoveOverPath(currentPath);
                playerEntity.MoveFinished += StartTurn;
                EndTurn();
            }
        }

        private void UpdatePointerWorldPosition(Vector2 pointerScreenPos)
        {
            var ray = followCam.Camera.ScreenPointToRay(pointerScreenPos);

            if (Physics.Raycast(ray, out var hitInfo, 100, layers))
            {
                var tile = hitInfo.collider.GetComponent<Tile>();
                SetMarkedTile(tile);
            }
            else
            {
                ClearPath();
                currentTargetedTile?.SetMarked(false);
            }
        }

        public void StartTurn()
        {
            hasTurn = true;

            playerInputActions.Enable();
        }

        public void EndTurn()
        {
            hasTurn = false;
            
            playerInputActions.Disable();
        }

        public void InitializeFromNetwork(PlayerStartData data)
        {
            playerEntity.SetTileDirect(data.position);
        }
        

        #region Pathfinding

        private void SetMarkedTile(Tile tile)
        {
            if (currentTargetedTile != null)
            {
                currentTargetedTile.SetMarked(false);
                ClearPath();
            }

            currentTargetedTile = tile;

            if (currentTargetedTile != null)
            {
                currentTargetedTile.SetMarked(true);
                FindPathTo(tile);
            }
        }

        private void ClearPath()
        {
            if (currentPath != null)
            {
                foreach (var pathTile in currentPath)
                {
                    pathTile.SetShowPath(false);
                }

                currentPath = null;
            }
        }

        private void ShowPath()
        {
            foreach (var tile in currentPath)
            {
                tile.SetShowPath(true);
            }
        }

        private void HidePath()
        {
            if (currentPath != null)
            {
                foreach (var tile in currentPath)
                {
                    tile.SetShowPath(false);
                }
            }
        }

        private void FindPathTo(Tile targetTile)
        {
            var playerPosition = playerEntity.CurrentTile;
            var targetPosition = new Vector2Int(targetTile.X, targetTile.Y);

            if (targetPosition == playerPosition) return;

            var path = Astar.FindPathToTarget(playerPosition, targetPosition, playerEntity.grid.CellGrid);
            if (path.Count > 0)
            {
                // Un-show the old path
                HidePath();

                currentPath = playerEntity.grid.GetTilesFromPositions(path);

                ShowPath();
            }
        }

        #endregion
    }
}
