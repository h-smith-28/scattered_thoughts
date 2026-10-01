using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.5f;
    [SerializeField]
    private float gravity = 20.0f;
    private CharacterController controller;
    private InputAction moveInput;
    private Vector3 moveDir = Vector3.zero;

    public void Start()
    {
        controller = GetComponentInChildren<CharacterController>();
        moveInput = InputSystem.actions.FindAction("Move");
    }

    public void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();
        HandleMovement(moveVector);
    }

    private void HandleMovement(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDir.y;
        Vector2 newSpeed = new Vector2(moveVector.y * moveSpeed, moveVector.x * moveSpeed);
        moveDir = (forward * newSpeed.x) + (right *newSpeed.y);
        moveDir.y = oldY;

        if(!controller.isGrounded)
            moveDir.y -= gravity * Time.deltaTime;

        controller.Move(moveDir * Time.deltaTime);
    }

}
