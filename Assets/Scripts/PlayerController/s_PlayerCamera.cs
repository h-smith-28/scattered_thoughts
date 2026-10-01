using UnityEngine.InputSystem;
using UnityEngine;

public class s_PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraSens = 0.2f;
    private float cameraConstraint = 90.0f;
    private Camera mainCamera;
    private float lookAngle = 0.0f;

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        HandleCamera(mouseDelta);
    }

    private void HandleCamera(Vector2 mouseDelta)
    {
        lookAngle += -mouseDelta.y * cameraSens;
        lookAngle = Mathf.Clamp(lookAngle, -cameraConstraint, cameraConstraint);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * cameraSens, 0);
    }

}
