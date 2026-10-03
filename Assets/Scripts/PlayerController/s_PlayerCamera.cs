/**
* Handles all the camera movement for the player
* Also handles the raycasts for interacting with objects, as camera is the center for all raycasts
*/
using UnityEngine.InputSystem;
using UnityEngine;

public class s_PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraSens = 0.2f;
    private float cameraConstraint = 90.0f;
    private Camera mainCamera;
    private float lookAngle = 0.0f;

    private bool holdingObj = false;
    private GameObject currentHeldObj;

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        if(!InputSystem.actions.FindAction("Secondary").IsPressed())
            HandleCamera(mouseDelta);
        HandleObjectInteraction(mouseDelta);
    }

    private void HandleCamera(Vector2 mouseDelta)
    {
        lookAngle += -mouseDelta.y * cameraSens;
        lookAngle = Mathf.Clamp(lookAngle, -cameraConstraint, cameraConstraint);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * cameraSens, 0);
    }

    private void HandleObjectInteraction(Vector2 mouseDelta)
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (InputSystem.actions.FindAction("Attack").WasPressedThisFrame())
        {
            if (!holdingObj)
            {
                if (Physics.Raycast(ray, out hit, 50))
                {
                    if(hit.collider.gameObject.TryGetComponent(out s_Grabbable grabbedObj))
                    {
                        Debug.Log("Interacted with: " + hit.collider.gameObject.name);
                        currentHeldObj = hit.collider.gameObject;
                        grabbedObj.Grab(gameObject);
                        holdingObj = true;
                    }
                }
            }
            else
            {
                if (currentHeldObj.GetComponent<s_StickyNoteController>())
                {
                    if (Physics.Raycast(ray, out hit, 50))
                    {
                        currentHeldObj.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                        currentHeldObj.transform.position = hit.point;
                        currentHeldObj.GetComponent<s_Grabbable>().Drop(false);
                        Debug.Log("Sticking to: " + hit.collider.gameObject.name);
                        holdingObj = false;
                    }
                }
                else
                {
                    currentHeldObj.GetComponent<s_Grabbable>().Drop(true);
                    holdingObj = false;   
                }
            }
        }
    }

}
