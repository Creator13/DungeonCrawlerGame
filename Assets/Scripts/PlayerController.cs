﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

namespace Dungen
{
    [RequireComponent(typeof(IsoCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private IsoFollowCamera camera;
        [SerializeField] private InputActionAsset playerInputActions;

        private IsoCharacterController characterController;
        private RaycastHit[] hitContainer;

        private Tile currentTargetedTile;
        private List<Tile> currentPath;

        private void Awake()
        {
            characterController = GetComponent<IsoCharacterController>();

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

                characterController.Move(moveVector);
            };

            var click = playerInputActions["PointerClick"];

            var move = playerInputActions["PointerMove"];
            move.performed += HandlePointerMove;

            camera.CameraMoved += HandleCameraMoved;
        }

#if !ENABLE_INPUT_SYSTEM
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                characterController.Move(IsoCharacterController.MoveDirection.Up);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                characterController.Move(IsoCharacterController.MoveDirection.Down);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                characterController.Move(IsoCharacterController.MoveDirection.Left);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                characterController.Move(IsoCharacterController.MoveDirection.Right);
            }
        }
#endif

        private void OnValidate()
        {
            // TODO replace this with a custom editor
            if (playerInputActions != null && !playerInputActions.Any(action => action.name == "Walk"))
            {
                playerInputActions = null;
            }
        }

        private void HandleCameraMoved()
        {
            var screenPos = playerInputActions["PointerMove"].ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandlePointerMove(InputAction.CallbackContext ctx)
        {
            var screenPos = ctx.ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void UpdatePointerWorldPosition(Vector2 pointerScreenPos)
        {
            var ray = Camera.main.ScreenPointToRay(pointerScreenPos);

            if (Physics.Raycast(ray, out var hitInfo))
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

        private void SetMarkedTile(Tile tile)
        {
            if (currentTargetedTile != null)
            {
                currentTargetedTile.SetMarked(false);
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


        private void FindPathTo(Tile targetTile)
        {
            var playerPosition = characterController.CurrentTile;
            var targetPosition = new Vector2Int(targetTile.X, targetTile.Y);

            var path = Astar.FindPathToTarget(playerPosition, targetPosition, characterController.grid.CellGrid);
            if (path.Count > 0)
            {
                if (currentPath != null)
                {
                    foreach (var tile in currentPath)
                    {
                        tile.SetShowPath(false);
                    }
                }

                currentPath = characterController.grid.GetTilesFromPositions(path);

                foreach (var tile in currentPath)
                {
                    tile.SetShowPath(true);
                }
            }
        }
    }
}
