using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungen
{
    [RequireComponent(typeof(IsoCharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private IsoCharacterController characterController;
        
        private void Awake()
        {
            characterController = GetComponent<IsoCharacterController>();
        }

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
    }
}
