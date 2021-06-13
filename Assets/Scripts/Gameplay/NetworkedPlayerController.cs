using System.Collections.Generic;
using System.Linq;
using Dungen.Gameplay;
using Dungen.Netcode;
using Dungen.World;
using UnityEngine;
using UnityEngine.InputSystem;
using EditorUtils;

namespace Dungen
{
    [RequireComponent(typeof(IsoEntity))]
    public class NetworkedPlayerController : NetworkedBehavior
    {
        [SerializeField] private DungenGame gameController;
        [SerializeField] private IsoFollowCamera followCam;
        [SerializeField] private InputActionAsset playerInputActions;
        [SerializeField] private LayerMask layers;

        private RaycastHit[] hitContainer;

        private Tile currentTargetedTile;
        private List<Tile> currentPath;

        private bool hasTurn;

        private void Awake()
        {
            controllingEntity = GetComponent<IsoEntity>();

            BindActions();
        }

        private void Start()
        {
            EndTurn();
        }

        private void BindActions()
        {
            // var walk = playerInputActions["Walk"];
            // walk.performed += ctx =>
            // {
            //     var moveVectorFloat = -ctx.ReadValue<Vector2>();
            //     var moveVector = new Vector2Int((int) moveVectorFloat.normalized.x, (int) moveVectorFloat.normalized.y);
            //
            //     playerEntity.Move(moveVector);
            // };

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
            if (controllingEntity.IsMoving) return;

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
            if (currentTargetedTile != null && currentPath != null && currentPath.Count > 0 && !controllingEntity.IsMoving)
            {
                HidePath();
                currentTargetedTile.SetMarked(false);
                // playerEntity.MoveOverPath(currentPath);
                // playerEntity.MoveFinished += StartTurn;
                gameController.RequestMove(currentTargetedTile.Data);
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
            if (!hasTurn) return;

            hasTurn = false;

            playerInputActions.Disable();
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
            var playerPosition = controllingEntity.CurrentTile;
            var targetPosition = new Vector2Int(targetTile.X, targetTile.Y);

            if (targetPosition == playerPosition) return;

            var path = Astar.FindPathToTarget(playerPosition, targetPosition, controllingEntity.grid.CellGrid);
            if (path.Count > 0)
            {
                // Un-show the old path
                HidePath();

                currentPath = controllingEntity.grid.GetTilesFromPositions(path);

                ShowPath();
            }
        }

        #endregion
    }
}
