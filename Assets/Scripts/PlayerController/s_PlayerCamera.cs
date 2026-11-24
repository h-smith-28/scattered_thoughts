/**
* Handles all the camera movement for the player
* Also handles the raycasts for interacting with objects, as camera is the center for all raycasts
*/
using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class s_PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private float cameraSens = 0.2f;
    [SerializeField]
    private float interactRange = 2.5f;
    private float cameraConstraint = 90.0f;
    private Camera mainCamera;
    private float lookAngle = 0.0f;

    private bool holdingObj = false;
    private GameObject currentHeldObj;
    [Header("Interaction Feedback / 交互提示")]
    [Tooltip("Shader Graph material for the outline. Edit its Color and Outline Width in the Material Inspector.")]
    [SerializeField] private Material outlineMaterial;
    [Tooltip("Shader Graph material for the sticky note footprint. Edit its color and opacity in the Material Inspector.")]
    [SerializeField] private Material placementPreviewMaterial;
    private s_InteractionFeedback interactionFeedback;

    public bool usingPhone = false;
    [SerializeField]
    private GameObject phone;

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        InitializeFeedback();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!mainCamera) InitializeFeedback();
        if (!mainCamera) return;
        if (!usingPhone){
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if(!InputSystem.actions.FindAction("Secondary").IsPressed())
                HandleCamera(mouseDelta);
            HandleObjectInteraction(mouseDelta);
            HandleStationInteraction();
        }
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (!usingPhone)
            {
                phone.GetComponent<s_PhoneController>().StartUsing();
                usingPhone = true;
            }
            else
            {
                phone.GetComponent<s_PhoneController>().EndUsing();
                usingPhone = false;
            }
        }
    }

    private void LateUpdate()
    {
        // Also recover after script reloads when Enter Play Mode skips scene reload.
        if (interactionFeedback == null) InitializeFeedback();
        if (interactionFeedback == null || Time.timeScale <= 0f) return;
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        if (holdingObj)
        {
            if (currentHeldObj && currentHeldObj.GetComponent<s_StickyNoteController>())
                interactionFeedback.DrawPlacement(ray, currentHeldObj);
            else
                interactionFeedback.DrawFocus(null);
            return;
        }
        if(!usingPhone){
            Transform target = null;
            if (Physics.Raycast(ray, out RaycastHit hit, 50f, ~0, QueryTriggerInteraction.Ignore))
            {
                s_Grabbable grabbable = hit.collider.GetComponentInParent<s_Grabbable>();
                s_ComputerStation station = hit.collider.GetComponentInParent<s_ComputerStation>();
                s_MinigameObject minigame = hit.collider.GetComponentInParent<s_MinigameObject>();
                if (grabbable && grabbable.isActiveAndEnabled)
                    target = grabbable.transform;
                else if (hit.distance <= interactRange)
                {
                    if (station && station.isActiveAndEnabled && !station.IsBusy) target = station.transform;
                    else if (minigame && minigame.isActiveAndEnabled && !minigame.IsBusy) target = minigame.transform;
                }
            }
            interactionFeedback.DrawFocus(target);
        }
    }

    private void InitializeFeedback()
    {
        if (!mainCamera) mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera && interactionFeedback == null)
            interactionFeedback = new s_InteractionFeedback(mainCamera, outlineMaterial, placementPreviewMaterial);
    }

    private void OnDisable() => interactionFeedback?.DrawFocus(null);
    private void OnDestroy() => interactionFeedback?.Dispose();

    private void HandleStationInteraction()
    {
        if (!Keyboard.current.eKey.wasPressedThisFrame || holdingObj)
            return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, ~0, QueryTriggerInteraction.Ignore))
        {
            s_ComputerStation station = hit.collider.GetComponentInParent<s_ComputerStation>();
            if (station != null && station.isActiveAndEnabled)
                station.Use(gameObject);
            
            s_MinigameObject obj = hit.collider.GetComponentInParent<s_MinigameObject>();
            if (obj != null && obj.isActiveAndEnabled)
                obj.Use(gameObject);
        }
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
                if (Physics.Raycast(ray, out hit, 50, ~0, QueryTriggerInteraction.Ignore))
                {
                    s_Grabbable grabbedObj = hit.collider.GetComponentInParent<s_Grabbable>();
                    if(grabbedObj && grabbedObj.isActiveAndEnabled)
                    {
                        //Debug.Log("Interacted with: " + hit.collider.gameObject.name);
                        currentHeldObj = grabbedObj.gameObject;
                        grabbedObj.Grab(gameObject);
                        holdingObj = true;
                    }
                }
            }
            else
            {
                if (currentHeldObj.GetComponent<s_StickyNoteController>())
                {
                    if (s_InteractionFeedback.TryGetPlacement(ray, currentHeldObj, out Vector3 position, out Quaternion rotation))
                    {
                        currentHeldObj.transform.SetPositionAndRotation(position, rotation);
                        currentHeldObj.GetComponent<s_Grabbable>().Drop(false);
                        //Debug.Log("Sticking to: " + hit.collider.gameObject.name);
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
