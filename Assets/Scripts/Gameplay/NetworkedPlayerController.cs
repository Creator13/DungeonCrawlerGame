using System;
using System.Collections.Generic;
using System.Linq;
using Dungen.Netcode;
using Dungen.World;
using UnityEngine;
using UnityEngine.InputSystem;
using EditorUtils;

namespace Dungen.Gameplay
{
    [RequireComponent(typeof(IsoEntity))]
    public class NetworkedPlayerController : NetworkedBehavior
    {
        private enum Mode { Move, Attack }

        [SerializeField] private DungenGame gameController;
        [SerializeField] private IsoFollowCamera followCam;
        [SerializeField] private InputActionAsset playerInputActions;
        [SerializeField] private LayerMask layers;

        private RaycastHit[] hitContainer;

        private Tile currentTargetedTile;
        private List<Tile> currentPath;
        private List<Tile> currentAttackRadius;

        private bool hasTurn;
        [SerializeField] private Mode mode;

        private void Awake()
        {
            controllingEntity = GetComponent<IsoEntity>();

            BindActions();
        }

        private void Start()
        {
            EndTurn();
            mode = Mode.Move;
            currentAttackRadius = controllingEntity.grid.TilesInRadius(controllingEntity.CurrentTile, 2);
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

            playerInputActions["SwitchToMove"].performed += ToggleMode;

            // followCam.CameraMoved += OnCameraMoved;
        }

        private void OnValidate()
        {
            // TODO replace this with a custom editor that shows all the input actions
            if (playerInputActions != null && !playerInputActions.Any(action => action.name == "Walk"))
            {
                playerInputActions = null;
            }
        }

        private void Update() { }

        private void RecalculatePath()
        {
            if (controllingEntity.IsMoving) return;

            var screenPos = playerInputActions["PointerMove"].ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandlePointerMove(InputAction.CallbackContext ctx)
        {
            if (!hasTurn || controllingEntity.IsMoving) return;

            var screenPos = ctx.ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandleClick(InputAction.CallbackContext ctx)
        {
            switch (mode)
            {
                case Mode.Move:
                    HandleMoveClick();
                    break;
                case Mode.Attack:
                    HandleAttackClick();
                    break;
            }
        }

        private void HandleAttackClick()
        {
            if (currentTargetedTile != null)
            {
                if (currentTargetedTile.ContainsEnemy)
                {
                    gameController.RequestAttack(currentTargetedTile.Data);
                }
            }
        }

        private void HandleMoveClick()
        {
            if (currentTargetedTile != null && currentPath != null && currentPath.Count > 0 && !controllingEntity.IsMoving)
            {
                currentTargetedTile.SetMarked(false);
                gameController.RequestMove(currentTargetedTile.Data);
                HidePath();
                EndTurn();
            }
        }

        private void ToggleMode(InputAction.CallbackContext ctx)
        {
            if (!hasTurn || controllingEntity.IsMoving) return;
            
            if (mode == Mode.Move)
            {
                SwitchToAttackMode();
            }
            else
            {
                SwitchToMoveMode();
            }
        }

        private void SwitchToMoveMode()
        {
            mode = Mode.Move;
            HideRadius();
        }

        private void SwitchToAttackMode()
        {
            currentAttackRadius = controllingEntity.grid.TilesInRadius(controllingEntity.CurrentTile, 2);
            mode = Mode.Attack;
            HidePath();
            ShowRadius();
        }

        private void UpdatePointerWorldPosition(Vector2 pointerScreenPos)
        {
            switch (mode)
            {
                case Mode.Move:
                    MovePointerUpdate(pointerScreenPos);
                    break;
                case Mode.Attack:
                    AttackPointerUpdate(pointerScreenPos);
                    break;
            }
        }

        private void AttackPointerUpdate(Vector2 pointerScreenPos)
        {
            var ray = followCam.Camera.ScreenPointToRay(pointerScreenPos);

            if (Physics.Raycast(ray, out var hitInfo, 100, layers))
            {
                var tile = hitInfo.collider.GetComponent<Tile>();

                if (currentAttackRadius.Contains(tile))
                {
                    if (currentTargetedTile) currentTargetedTile.SetMarked(false);
                    currentTargetedTile = tile;
                    currentTargetedTile.SetMarked(true);
                }
                else
                {
                    if (currentTargetedTile) currentTargetedTile.SetMarked(false);
                    currentTargetedTile = null;
                }
            }
            else
            {
                SetMarkedTile(null);
                currentTargetedTile = null;
            }
        }

        private void MovePointerUpdate(Vector2 pointerScreenPos)
        {
            var ray = followCam.Camera.ScreenPointToRay(pointerScreenPos);

            if (Physics.Raycast(ray, out var hitInfo, 100, layers))
            {
                var tile = hitInfo.collider.GetComponent<Tile>();

                if (tile.ContainsEnemy)
                {
                    SetMarkedTile(null);
                    ClearPath();
                    return;
                }

                SetMarkedTile(tile);
            }
            else
            {
                SetMarkedTile(null);
                ClearPath();
                currentTargetedTile = null;
            }
        }

        public void StartTurn()
        {
            hasTurn = true;

            playerInputActions.Enable();

            switch (mode)
            {
                case Mode.Move:
                    HideRadius();
                    RecalculatePath();
                    break;
                case Mode.Attack:
                    HidePath();
                    ShowRadius();
                    break;
            }
        }

        public void EndTurn()
        {
            hasTurn = false;

            playerInputActions.Disable();
            HidePath();
            HideRadius();
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

        private void ShowRadius()
        {
            foreach (var tile in currentAttackRadius)
            {
                tile.SetShowRadius(true);
            }
        }

        private void HideRadius()
        {
            if (currentAttackRadius == null) return;

            foreach (var tile in currentAttackRadius)
            {
                tile.SetShowRadius(false);
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
            var playerPosition = (Vector2Int) controllingEntity.CurrentTile.Data;
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
