using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class s_MinigameObject : MonoBehaviour
{
    private Transform viewPoint;   // where the camera goes when seated
    public Vector3 viewPointOffset;
    public float blendTime = 0.6f;
    public Image crosshairRef;
    public Material pixelShader;

    s_PlayerMovement playerMovement;
    s_PlayerCamera playerCamera;
    Transform cam;
    Vector3 savedLocalPos;
    Quaternion savedLocalRot;
    bool inUse, moving;
    public bool IsBusy => inUse || moving;

    public void Start()
    {
        GameObject point = new GameObject("ViewPoint");
        viewPoint = point.transform;

        viewPoint.SetParent(transform);
        viewPoint.localPosition = viewPointOffset;
        viewPoint.localRotation = Quaternion.Euler(0, 180, 0);
    }

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
        pixelShader.SetVector("_PixelResolution", new Vector2(1600,900));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshairRef.enabled = false;


        StartCoroutine(MoveCamera(viewPoint.position, viewPoint.rotation,
            () =>PlayGame(true)));
    }

    void Update()
    {
        if (inUse && !moving && Keyboard.current.escapeKey.wasPressedThisFrame)
            Exit();
    }

    public void Exit()
    {
        inUse = false;
        Transform parent = cam.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairRef.enabled = true;
        pixelShader.SetVector("_PixelResolution", new Vector2(800,450));

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

    void PlayGame(bool on)
    {
        s_RotationMinigame rotateGame = gameObject.GetComponent<s_RotationMinigame>();
        if(rotateGame)
            rotateGame.StartGame(this);
    }
}