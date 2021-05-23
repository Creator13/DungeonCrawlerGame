using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.VirtualTexturing;

namespace Dungen
{
    [RequireComponent(typeof(IsoCharacterController))]
    public class PlayerController : MonoBehaviour, IEntity
    {
        [SerializeField] private new IsoFollowCamera camera;
        [SerializeField] private InputActionAsset playerInputActions;
        [SerializeField] private LayerMask layers;
        
        private IsoCharacterController characterController;
        private RaycastHit[] hitContainer;

        private Tile currentTargetedTile;
        private List<Tile> currentPath;

        private Coroutine moveRoutine;
        private bool isMoving;

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
            click.performed += HandleClick;

            var move = playerInputActions["PointerMove"];
            move.performed += HandlePointerMove;

            camera.CameraMoved += HandleCameraMoved;
        }

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
            if (isMoving) return;
            
            var screenPos = playerInputActions["PointerMove"].ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandlePointerMove(InputAction.CallbackContext ctx)
        {
            var screenPos = ctx.ReadValue<Vector2>();
            UpdatePointerWorldPosition(screenPos);
        }

        private void HandleClick(InputAction.CallbackContext ctx)
        {
            if (currentTargetedTile != null && currentPath != null && currentPath.Count > 0)
            {
                if (moveRoutine != null)
                {
                    StopCoroutine(moveRoutine);
                }
                moveRoutine = StartCoroutine(MoveOverPath(currentPath));
            }
        }

        private IEnumerator MoveOverPath(List<Tile> path)
        {
            isMoving = true;
            
            var enumerator = path.GetEnumerator();
            
            while (enumerator.MoveNext())
            {
                HidePath();
                
                characterController.SetTile(enumerator.Current);
                yield return new WaitForSeconds(.2f);
            }

            enumerator.Dispose();

            isMoving = false;
        }
        
        private void UpdatePointerWorldPosition(Vector2 pointerScreenPos)
        {
            var ray = Camera.main.ScreenPointToRay(pointerScreenPos);

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
            var playerPosition = characterController.CurrentTile;
            var targetPosition = new Vector2Int(targetTile.X, targetTile.Y);

            if (targetPosition == playerPosition) return;
            
            var path = Astar.FindPathToTarget(playerPosition, targetPosition, characterController.grid.CellGrid);
            if (path.Count > 0)
            {
                // Un-show the old path
                HidePath();

                currentPath = characterController.grid.GetTilesFromPositions(path);

                ShowPath();
            }
        }
    }
}
