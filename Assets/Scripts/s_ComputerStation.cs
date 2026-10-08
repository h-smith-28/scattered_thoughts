using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class s_ComputerStation : MonoBehaviour
{
    public Transform seatPoint;   // where the camera goes when seated
    public CanvasGroup screen;    // the monitor's UI
    public float blendTime = 0.6f;

    s_PlayerMovement playerMovement;
    s_PlayerCamera playerCamera;
    Transform cam;
    Vector3 savedLocalPos;
    Quaternion savedLocalRot;
    bool inUse, moving;
    public bool IsBusy => inUse || moving;

    void Start() => SetScreenInteractive(false);

    public void Use(GameObject player)
    {
        if (inUse || moving) return;
        playerMovement = player.GetComponent<s_PlayerMovement>();
        playerCamera = player.GetComponent<s_PlayerCamera>();
        cam = player.GetComponentInChildren<Camera>().transform;
        inUse = true;
        SetPlayerControls(false);

        savedLocalPos = cam.localPosition;
        savedLocalRot = cam.localRotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(MoveCamera(seatPoint.position, seatPoint.rotation,
            () => SetScreenInteractive(true)));
    }

    void Update()
    {
        if (inUse && !moving && Keyboard.current.escapeKey.wasPressedThisFrame)
            Exit();
    }

    void Exit()
    {
        inUse = false;
        SetScreenInteractive(false);
        Transform parent = cam.parent;
        StartCoroutine(MoveCamera(parent.TransformPoint(savedLocalPos),
            parent.rotation * savedLocalRot,
            () => SetPlayerControls(true)));
    }


    void SetPlayerControls(bool on)
    {
        playerMovement.enabled = on;
        playerCamera.enabled = on;
        if (on)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    IEnumerator MoveCamera(Vector3 toPos, Quaternion toRot, Action onDone)
    {
        moving = true;
        Vector3 fromPos = cam.position;
        Quaternion fromRot = cam.rotation;

        for (float t = 0f; t < 1f; t += Time.deltaTime / blendTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);
            cam.SetPositionAndRotation(Vector3.Lerp(fromPos, toPos, s),
                                       Quaternion.Slerp(fromRot, toRot, s));
            yield return null;
        }
        cam.SetPositionAndRotation(toPos, toRot);
        moving = false;
        onDone?.Invoke();
    }

    void SetScreenInteractive(bool on)
    {
        screen.interactable = on;
        screen.blocksRaycasts = on;
    }
}