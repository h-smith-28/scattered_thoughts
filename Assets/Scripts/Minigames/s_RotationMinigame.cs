using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class s_RotationMinigame : MonoBehaviour
{
    // Angles per second
    public float rotationSpeed;
    // Degrees of circle covered in success zone, so 90 gives success zone equal to half of circle
    public float successZoneSize;
    public float successCountNeeded;
    private float currentSuccesses;

    private bool gameStarted;

    private s_MinigameObject owner;

    private float rotationDir = -1.0f;

    public GameObject canvas;
    private Transform successZone;
    private Transform needle;

    private float successPoint;

    public void StartGame(s_MinigameObject ownerObj)
    {
        canvas.SetActive(true);
        needle = canvas.transform.Find("Needle");
        successZone = canvas.transform.Find("CorrectRing");

        successZone.GetComponent<UnityEngine.UI.Image>().fillAmount = successZoneSize/360;
        successPoint = UnityEngine.Random.Range(0f, 360f);
        successZone.localRotation = Quaternion.Euler(0,0, -successPoint + successZoneSize / 2f);
        needle.localRotation = Quaternion.identity;

        owner = ownerObj;

        currentSuccesses = 0;
        gameStarted = true;
    }

    private void Update()
    {
        if (gameStarted)
        {
            needle.Rotate(0, 0, rotationSpeed * Time.deltaTime * rotationDir);
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                float needleAngle = -needle.localEulerAngles.z;

                float difference = Mathf.Abs(
                    Mathf.DeltaAngle(needleAngle, successPoint)
                );

                if (difference <= successZoneSize / 2f){
                    currentSuccesses++;
                    Debug.Log("Successful hit: " + currentSuccesses);

                    if (currentSuccesses >= successCountNeeded)
                    {
                        EndGame(true);
                    }
                }else{
                    EndGame(false);
                }
                rotationDir *= -1.0f;
                successPoint = UnityEngine.Random.Range(0f, 360f);
                successZone.localRotation = Quaternion.Euler(0,0, -successPoint + successZoneSize / 2f);
            }
        }
    }

    public void EndGame(bool success)
    {
        if (success)
        {
            //tbd
            Debug.Log("Succeeded rotation qte");
        }
        else
        {
            //tbd
            Debug.Log("Failed rotation qte");
        }
        canvas.SetActive(false);
        owner.Exit();
        gameStarted = false;
    }
}
