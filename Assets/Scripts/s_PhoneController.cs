using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using System;
using UnityEngine.Rendering;

public class s_PhoneController : MonoBehaviour
{
    public TMP_Text timeUIMins;
    public TMP_Text timeUIHours;
    public Image crosshairRef;
    

    private bool inUse;
    public Vector3 usingPosition;
    private Vector3 normalPosition = new Vector3(0.35f, -0.2f, 0.35f);

    s_GameStates gameStatesScript;
    public Material pixelShader;

    private int textStage = 0;
    private bool waitingForResponse = false;
    private float responseTimer = 0.0f;

    [Header("Phone Messages")]
    public GameObject pMessage1;
    public GameObject pMessage2;
    public GameObject pMessage3;

    public GameObject mMessage1;
    public GameObject mMessage2;
    public GameObject mMessage3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        gameStatesScript = gameManager.GetComponent<s_GameStates>();
    }

    // Update is called once per frame
    void Update()
    {
        timeUIMins.text = $"{Mathf.RoundToInt(gameStatesScript.timeMinutes):D2}";
        timeUIHours.text = $"{Mathf.RoundToInt(gameStatesScript.timeHour):D2}";
        if (waitingForResponse)
        {
            responseTimer += Time.deltaTime;
        }
        HandleResponses();
    }

    private void HandleResponses()
    {
        if(textStage == 1 && responseTimer >= 5)
        {
            textStage++;
            waitingForResponse = false;
            responseTimer = 0.0f;
            mMessage1.SetActive(true);
        }
        if(textStage == 3 && responseTimer >= 5)
        {
            textStage++;
            waitingForResponse = false;
            responseTimer = 0.0f;
            mMessage2.SetActive(true);
            gameStatesScript.AdjustStress(20);
        }
        if(textStage == 5 && responseTimer >= 60)
        {
            textStage++;
            waitingForResponse = false;
            responseTimer = 0.0f;
            mMessage3.SetActive(true);
            gameStatesScript.AdjustStress(30);
        }
    }

    public void StartUsing()
    {
        gameObject.transform.localPosition = usingPosition;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        crosshairRef.enabled = false;
        pixelShader.SetVector("_PixelResolution", new Vector2(1600,900));
    }

    public void EndUsing()
    {
        gameObject.transform.localPosition = normalPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairRef.enabled = true;
        pixelShader.SetVector("_PixelResolution", new Vector2(800,450));
    }

    public void NextTextStage()
    {
        if (!waitingForResponse)
        {
            textStage++;
            waitingForResponse = true;
        }
        if(textStage == 1)
        {
            pMessage1.SetActive(true);
        }
        if(textStage == 3)
        {
            pMessage2.SetActive(true);
        }
        if(textStage == 5)
        {
            pMessage3.SetActive(true);
        }
    }
}
